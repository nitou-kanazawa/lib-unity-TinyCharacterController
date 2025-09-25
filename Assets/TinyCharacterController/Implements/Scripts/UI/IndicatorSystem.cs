using System.Runtime.InteropServices;
using Nitou.BatchProcessor;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace Nitou.TCC.Implements.UI
{
    public sealed class IndicatorSystem : SystemBase<Indicator, IndicatorSystem>, 
                                          IPostUpdate
    {
        /// <summary>
        /// Struct for Indicator calculations.
        /// </summary>
        private struct IndicatorData
        {
            [MarshalAs(UnmanagedType.U1)] public bool IsTurnOffscreenIcon;
            [MarshalAs(UnmanagedType.U1)] public bool IsLimitIconInRange;
            public float Bounds;
            public Vector3 Offset;
        }

        [BurstCompile]
        private struct ApplyUiPositionJob : IJobParallelForTransform
        {
            public NativeArray<float3>.ReadOnly Positions;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = Positions[index];
            }
        }

        public float FarDistance = 1000;
        public float NearDistance = 0.01f;

        private TransformAccessArray _targetTransforms;
        private TransformAccessArray _uiTransforms;
        private NativeList<IndicatorData> _indicatorData;
        private JobHandle _handle;

        int ISystemBase.Order => 0;
        

        // ----------------------------------------------------------------------------

        #region Unity Lifecycle Events

        private void Awake()
        {
            _indicatorData = new NativeList<IndicatorData>(Allocator.Persistent);
            _targetTransforms = new TransformAccessArray(0);
            _uiTransforms = new TransformAccessArray(0);
        }

        private void OnDestroy()
        {
            _handle.Complete();
            UnregisterAllComponents();
            _uiTransforms.Dispose();
            _indicatorData.Dispose();
            _targetTransforms.Dispose();
        }

        void IPostUpdate.OnLateUpdate()
        {
            _handle.Complete();

            // If there is no camera, interrupt the process.
            if (CameraUtility.TryGetMainCamera(out var mainCamera) == false)
                return;


            // Detect and unregister deleted targets.
            RemoveNullTarget();

            // Allocate buffers.
            PrepareBufferSampler.Begin();
            var targetPositions = new NativeArray<float3>(Components.Count, Allocator.Temp);
            var uiAngles = new NativeArray<float>(Components.Count, Allocator.Temp);
            var uiPositions = new NativeArray<float3>(Components.Count, Allocator.TempJob);
            var isVisible = new NativeArray<bool>(Components.Count, Allocator.Temp);
            PrepareBufferSampler.End();


            // Get the coordinates of characters and targets.
            GatherTargetPositionSampler.Begin();
            PrepareCalculateScreenPosition(mainCamera, out var screenSize, out var cameraMatrix, out var screenCenter);
            GatherTargetPositionSampler.End();

            CalculatePositionSampler.Begin();

            // Update the world coordinates of the targets.
            // A provisional measure to ensure that IJobParallelForTransform.Run works correctly.
            for (var i = 0; i < Components.Count; i++)
                targetPositions[i] = _targetTransforms[i].position + _indicatorData[i].Offset;

            // Update UI coordinates.
            
#if COLLECTION_2
            var indicatorData = _indicatorData.AsReadOnly();
#else
            var indicatorData = _indicatorData.AsParallelReader();
#endif
            BatchCalculateUiPositions(
                Components.Count,
                indicatorData, targetPositions.AsReadOnly(),
                cameraMatrix, screenSize, screenCenter, mainCamera.orthographic,
                ref uiPositions, ref isVisible, ref uiAngles);
            CalculatePositionSampler.End();

            // Apply UI coordinates.
            ApplyPositionSampler.Begin();
            _handle = new ApplyUiPositionJob { Positions = uiPositions.AsReadOnly() }.Schedule(_uiTransforms);
            uiPositions.Dispose(_handle);

            for (var index = 0; index < Components.Count; index++)
                Components[index].OnUpdate(uiAngles[index], isVisible[index]);

            ApplyPositionSampler.End();

            // Release buffers.
            targetPositions.Dispose();
            isVisible.Dispose();
            uiAngles.Dispose();
        }
        
        #endregion

        
        // ----------------------------------------------------------------------------

        /// <summary>
        /// Directly update the Indicator's information individually.
        /// An error will occur if executed for unregistered components.
        /// </summary>
        /// <param name="component">The target component</param>
        /// <param name="bounds">Bounds</param>
        /// <param name="offset">Offset</param>
        /// <param name="isTurnOffscreenIcon">Determine if the UI rotates off-screen</param>
        /// <param name="isLimitIconInRange">Determine if the UI goes off-screen</param>
        public void SetIndicatorData(Indicator component, float bounds, Vector3 offset, bool isTurnOffscreenIcon, bool isLimitIconInRange)
        {
            _handle.Complete();

            var index = ((IComponentIndex)component).Index;

            Assert.IsTrue(index != -1 || _indicatorData.Length < index);

            var data = _indicatorData[index];
            data.Bounds = bounds;
            data.Offset = offset;
            data.IsTurnOffscreenIcon = isTurnOffscreenIcon;
            data.IsLimitIconInRange = isLimitIconInRange;
            _indicatorData[index] = data;
        }

        
        /// <summary>
        /// Callback when an Indicator is registered with the system.
        /// Register UI and update coordinates.
        /// </summary>
        /// <param name="component">Component to register</param>
        /// <param name="index">Index of the registered component</param>
        protected override void OnRegisterComponent(Indicator component, int index)
        {
            // Complete the job if processing in a job
            _handle.Complete();

            var data = new IndicatorData
            {
                Offset = component.Offset,
                IsLimitIconInRange = component.IsLimitIconRange,
                IsTurnOffscreenIcon = component.IsTurnOffscreenIcon,
                Bounds = component.Bounds
            };

            // Register Transform and data
            _indicatorData.Add(data);
            _targetTransforms.Add(component.Target);
            _uiTransforms.Add(component.transform);

            // If there is no camera, do not update the coordinates.
            if (CameraUtility.TryGetMainCamera(out var mainCamera) == false)
                return;

            // Calculate UI coordinates and apply them immediately
            UpdateUiPositionImmediate(component, mainCamera, data);
        }

        /// <summary>
        /// Callback when unregistering an Indicator from the system.
        /// </summary>
        /// <param name="component">Component to unregister</param>
        /// <param name="index">Index of the component to unregister</param>
        protected override void OnUnregisterComponent(Indicator component, int index)
        {
            _handle.Complete();

            _indicatorData.RemoveAtSwapBack(index);
            _targetTransforms.RemoveAtSwapBack(index);
            _uiTransforms.RemoveAtSwapBack(index);
        }

        /// <summary>
        /// Calculate Indicator information from the component individually.
        /// This process is calculated individually.
        /// </summary>
        /// <param name="component">Component information</param>
        /// <param name="mainCamera">Camera</param>
        /// <param name="data">Indicator data</param>
        private void UpdateUiPositionImmediate(Indicator component, Camera mainCamera, IndicatorData data)
        {
            // Calculate UI coordinates individually
            var targetPosition = component.Target.position;
            var angle = 0f;

            // Execute pre-processing for operations that cannot be batch processed with BurstCompile
            PrepareCalculateScreenPosition(
                mainCamera, out var screenSize, 
                out var cameraMatrix, out var screenCenter);

            // Calculate UI coordinates with BurstCompile
            CalculateScreenPosition(
                targetPosition, data,
                cameraMatrix, screenSize,
                out var screenPosition, out var isVisible);

            // If the UI is off-screen, calculate the UI coordinates to track off-screen targets
            if (isVisible == false && data.IsLimitIconInRange)
            {
                CalculateIndirectPosition(data, screenSize, screenCenter, mainCamera.orthographic,
                    ref screenPosition, out angle);
            }
    
            // Update the UI coordinates and settings
            component.transform.position = screenPosition;
            component.OnUpdate(angle, isVisible);
        }


        /// <summary>
        /// Calculate UI coordinates individually.
        /// </summary>
        /// <param name="targetPosition">Target's world coordinates</param>
        /// <param name="data">Indicator data</param>
        /// <param name="cameraMatrix">Camera matrix</param>
        /// <param name="screenSize">Screen size</param>
        /// <param name="screenPosition">Calculated screen coordinates</param>
        /// <param name="isVisible">Visibility setting for the calculated UI</param>
        [BurstCompile]
        private static void CalculateScreenPosition(
            in Vector3 targetPosition,
            in IndicatorData data,
            in Matrix4x4 cameraMatrix,
            in int2 screenSize,
            out float3 screenPosition,
            out bool isVisible)
        {
            CameraUtility.WorldToScreenPosition(targetPosition, cameraMatrix, screenSize, out screenPosition);
            CameraUtility.CalculateIsTargetVisible(screenPosition, screenSize, data.Bounds, out isVisible);
        }


        [BurstCompile]
        private static void CalculateIndirectPosition(
            in IndicatorData data, in int2 screenSize, in float2 screenCenter, bool isOrthographic,
            ref float3 screenPosition, out float angle)
        {
            angle = 0;
            GetIndicatorPosition(ref screenPosition, screenCenter, data.Bounds, isOrthographic);

            if (data.IsTurnOffscreenIcon)
                CameraUtility.IndirectUiAngle(screenPosition, screenSize, out angle);
        }

        /// <summary>
        /// Extract and precalculate the necessary calculations for component operation.
        /// </summary>
        /// <param name="mainCamera">Camera</param>
        /// <param name="screenSize">Screen size</param>
        /// <param name="cameraMatrix">Camera matrix</param>
        /// <param name="screenCenter">Screen center coordinates</param>
        private void PrepareCalculateScreenPosition(Camera mainCamera, out int2 screenSize,
            out Matrix4x4 cameraMatrix, out float2 screenCenter)
        {
            screenCenter = new float2(Screen.width * 0.5f, Screen.height * 0.5f);
            screenSize = new int2(Screen.width, Screen.height);

            // Calculate a matrix separately from the camera to recognize targets outside the screen.
            var projectionMatrix = CameraUtility.GetCachedProjectionMatrix(mainCamera, NearDistance, FarDistance);
            cameraMatrix = projectionMatrix * mainCamera.worldToCameraMatrix;
        }

        /// <summary>
        /// Calculate Indicator coordinates in batch processing.
        /// </summary>
        /// <param name="count">Number of registered components</param>
        /// <param name="indicatorDataArray">List of Indicator data</param>
        /// <param name="targetPositions">List of target coordinates</param>
        /// <param name="cameraMatrix">Camera matrix</param>
        /// <param name="screenSize">Screen size</param>
        /// <param name="screenCenter">Screen center</param>
        /// <param name="isOrthographic">Orthographic projection</param>
        /// <param name="screenPositions">List of UI coordinates</param>
        /// <param name="isVisibleArray">List of UI visibility status</param>
        /// <param name="angles">List of UI angles</param>
        [BurstCompile]
        private static void BatchCalculateUiPositions(
            int count,
            in NativeArray<IndicatorData>.ReadOnly indicatorDataArray,
            in NativeArray<float3>.ReadOnly targetPositions,
            in Matrix4x4 cameraMatrix,
            in int2 screenSize,
            in float2 screenCenter,
            bool isOrthographic,
            ref NativeArray<float3> screenPositions,
            ref NativeArray<bool> isVisibleArray,
            ref NativeArray<float> angles)
        {
            for (var i = 0; i < count; i++)
            {
                var data = indicatorDataArray[i];
                float angle = 0;

                CalculateScreenPosition(
                    targetPositions[i], data,
                    cameraMatrix, screenSize,
                    out var screenPosition, out var isVisible);

                if (isVisible == false && data.IsLimitIconInRange)
                    CalculateIndirectPosition(
                        data, screenSize, screenCenter, isOrthographic,
                        ref screenPosition, out angle);

                screenPositions[i] = screenPosition;
                isVisibleArray[i] = isVisible;
                angles[i] = angle;
            }
        }

        /// <summary>
        /// Calculate the screen position adjusted to not go off-screen.
        /// </summary>
        /// <param name="screenPosition">Screen coordinates</param>
        /// <param name="screenCenter">Screen center</param>
        /// <param name="bounds">Screen bounds</param>
        /// <param name="isOrthographic">True for orthographic projection</param>
        [BurstCompile]
        private static void GetIndicatorPosition(
            ref float3 screenPosition, in float2 screenCenter, in float bounds, in bool isOrthographic)
        {
            // If it's perspective and the UI is behind the camera, reverse the coordinates.
            var flip = isOrthographic || screenPosition.z > 0 ? 1 : -1;
            var positionFromCenter = new Vector2(
                screenPosition.x - screenCenter.x, screenPosition.y - screenCenter.y) * flip;

            var divX = screenCenter.x / Mathf.Abs(positionFromCenter.x);
            var divY = screenCenter.y / Mathf.Abs(positionFromCenter.y);

            if (divX < divY)
            {
                var angle = Vector2.SignedAngle(Vector2.right, positionFromCenter);
                positionFromCenter.x = Mathf.Sign(positionFromCenter.x) * (screenCenter.x * bounds);
                positionFromCenter.y = Mathf.Tan(Mathf.Deg2Rad * angle) * positionFromCenter.x;
            }
            else
            {
                var angle = Vector2.SignedAngle(Vector2.up, positionFromCenter);
                positionFromCenter.y = Mathf.Sign(positionFromCenter.y) * (screenCenter.y * bounds);
                positionFromCenter.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * positionFromCenter.y;
            }

            screenPosition = new float3(
                positionFromCenter.x + screenCenter.x,
                positionFromCenter.y + screenCenter.y,
                0);
        }

        /// <summary>
        ///     Remove elements from the list if their targets are null.
        /// </summary>
        private void RemoveNullTarget()
        {
            RemoveNullTargetSampler.Begin();
            var listOfNullTarget = ListPool<Indicator>.Get();

            foreach (var component in Components)
                if (component.Target == null)
                    listOfNullTarget.Add(component);

            foreach (var component in listOfNullTarget)
                Unregister(component, Timing);

            ListPool<Indicator>.Release(listOfNullTarget);
            RemoveNullTargetSampler.End();
        }
        
        // ----------------------------------------------------------------------------

        #region Static

        private static readonly CustomSampler RemoveNullTargetSampler = CustomSampler.Create("RemoveNullTargetSampler");
        private static readonly CustomSampler PrepareBufferSampler = CustomSampler.Create("Prepare Buffer");
        private static readonly CustomSampler GatherTargetPositionSampler = CustomSampler.Create("Gather Target Positions");
        private static readonly CustomSampler CalculatePositionSampler = CustomSampler.Create("Calculate Position");
        private static readonly CustomSampler ApplyPositionSampler = CustomSampler.Create("Apply Ui Position");

        #endregion
    }
}