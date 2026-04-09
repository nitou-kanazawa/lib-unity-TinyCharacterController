using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Nitou.BatchProcessor;

namespace Nitou.TCC.UI.UI
{
    /// <summary>
    /// <see cref="IndicatorPin"/>コンポーネントをバッチ処理するシステム。
    /// <see cref="IndicatorPin"/>コンポーネントを集約し、<see cref="Camera.main"/>の情報に基づいて一括で座標を計算します。
    /// 座標は非同期でTransformに適用されます。
    /// </summary>
    [BurstCompile]
    public sealed class IndicatorPinSystem : SystemBase<IndicatorPin, IndicatorPinSystem>,
                                             IPostUpdate
    {
        /// <summary>
        /// Transformの更新を適用する。
        /// </summary>
        [BurstCompile]
        private struct ApplyUiPositionJob : IJobParallelForTransform
        {
            public NativeArray<float3>.ReadOnly positions;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = positions[index];
            }
        }

        private Matrix4x4 _preFrameCameraMatrix;
        private NativeList<float3> _positions;
        private NativeList<float2> _uiSize;
        private TransformAccessArray _transforms;
        private JobHandle _handle;

        int ISystemBase.Order => 0;


        // ----------------------------------------------------------------------------

        #region Unity Lifecycle Events

        private void Awake()
        {
            _transforms = new TransformAccessArray(0);
            _positions = new NativeList<float3>(Allocator.Persistent);
            _uiSize = new NativeList<float2>(Allocator.Persistent);
        }

        private void OnDestroy()
        {
            _handle.Complete();
            UnregisterAllComponents();
            _transforms.Dispose();
            _positions.Dispose();
            _uiSize.Dispose();
        }

        void IPostUpdate.OnLateUpdate()
        {
            _handle.Complete();

            if (CameraUtility.TryGetMainCamera(out var camera) == false)
                return;

            // 計算に必要な要素を準備する
            var uiPositions = new NativeArray<float3>(Components.Count, Allocator.TempJob);
            var uiVisible = new NativeArray<bool>(Components.Count, Allocator.Temp);
            PrepareProcess(camera, out var screenSize, out var cameraWorldToCameraMatrix);

            // UI座標を計算する
            CalculateUi(
                screenSize, camera.projectionMatrix, cameraWorldToCameraMatrix,
                ref uiPositions, ref uiVisible);

            // UIの表示状態と座標を更新する
            _handle = ApplyUi(cameraWorldToCameraMatrix, uiPositions.AsReadOnly(), uiVisible);

            // バッファを解放する
            uiPositions.Dispose(_handle);
            uiVisible.Dispose();
        }

        #endregion


        /// <summary>
        /// 特定の要素を更新する。
        /// </summary>
        /// <param name="index">要素のID</param>
        /// <param name="position">ターゲットの新しいワールド座標</param>
        public void SetPosition(int index, in Vector3 position)
        {
            // ジョブ処理中の場合は強制完了させる
            _handle.Complete();

            // 要素を上書きする
            _positions[index] = position;
        }

        protected override void OnRegisterComponent(IndicatorPin component, int index)
        {
            // ジョブ処理中の場合は強制完了させる
            _handle.Complete();

            // 要素を追加する
            _transforms.Add(component.transform);
            _positions.Add(component.CorrectedPosition);
            _uiSize.Add(component.UiSize);
        }

        protected override void OnUnregisterComponent(IndicatorPin component, int index)
        {
            // ジョブ処理中の場合は強制完了させる
            _handle.Complete();

            // 要素を削除する
            _transforms.RemoveAtSwapBack(index);
            _positions.RemoveAtSwapBack(index);
            _uiSize.RemoveAtSwapBack(index);
        }


        /// <summary>
        /// UIの座標を計算する。
        /// </summary>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="projectionMatrix">投影行列</param>
        /// <param name="cameraWorldToCameraMatrix">カメラのワールド行列</param>
        /// <param name="uiPositions">UI座標のリスト</param>
        /// <param name="uiVisible">UI表示フラグのリスト</param>
        private void CalculateUi(in int2 screenSize,
                                 in Matrix4x4 projectionMatrix, in Matrix4x4 cameraWorldToCameraMatrix,
                                 ref NativeArray<float3> uiPositions, ref NativeArray<bool> uiVisible)
        {
            CalculatePositionSampler.Begin();
            // UI座標を計算する
            CalculateUiPosition(
                _positions.AsArray(),
                screenSize, projectionMatrix, cameraWorldToCameraMatrix,
                ref uiPositions);
            // UIの表示状態を判定する
            CalculateUiVisible(uiPositions, _uiSize.AsArray(), screenSize, ref uiVisible);
            CalculatePositionSampler.End();
        }

        /// <summary>
        /// UIの更新を適用する。
        /// </summary>
        /// <param name="cameraWorldToCameraMatrix">カメラ行列</param>
        /// <param name="uiPositions">UI座標のリスト</param>
        /// <param name="uiVisible">UI表示フラグのリスト</param>
        private JobHandle ApplyUi(
            in Matrix4x4 cameraWorldToCameraMatrix,
            in NativeArray<float3>.ReadOnly uiPositions,
            in NativeArray<bool> uiVisible)
        {
            ApplyUiSampler.Begin();

            // UI座標を更新する
            var handle = new ApplyUiPositionJob
            {
                positions = uiPositions
            }.Schedule(_transforms);
            JobHandle.ScheduleBatchedJobs();

            var isChangeCameraMatrix = UpdateCameraMatrix(cameraWorldToCameraMatrix);
            for (var i = 0; i < Components.Count; i++)
            {
                var component = Components[i];

                // カメラ行列が変更されているか、座標が変更されている場合はUIを変更とみなす
                if (component.IsChangePosition || isChangeCameraMatrix)
                    Components[i].ApplyUi(uiVisible[i]);
            }

            ApplyUiSampler.End();

            return handle;
        }

        /// <summary>
        /// カメラ行列を更新する。
        /// </summary>
        /// <param name="cameraWorldToCameraMatrix">新しい行列</param>
        /// <returns>行列が変更されている場合はTrue</returns>
        private bool UpdateCameraMatrix(Matrix4x4 cameraWorldToCameraMatrix)
        {
            var isChangeCameraMatrix = !_preFrameCameraMatrix.Equals(cameraWorldToCameraMatrix);
            _preFrameCameraMatrix = cameraWorldToCameraMatrix;
            return isChangeCameraMatrix;
        }


        // ----------------------------------------------------------------------------

        #region Static

        private static readonly CustomSampler PrepareBufferSampler = CustomSampler.Create("Prepare Buffer");
        private static readonly CustomSampler CalculatePositionSampler = CustomSampler.Create("Calculate Position");
        private static readonly CustomSampler ApplyUiSampler = CustomSampler.Create("Apply Ui");

        /// <summary>
        /// 計算用の要素を準備する。
        /// </summary>
        /// <param name="camera">カメラ</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="cameraWorldToCameraMatrix">カメラのワールド行列</param>
        private static void PrepareProcess(in Camera camera, out int2 screenSize, out Matrix4x4 cameraWorldToCameraMatrix)
        {
            PrepareBufferSampler.Begin();
            screenSize = new int2(Screen.width, Screen.height);
            cameraWorldToCameraMatrix = camera.worldToCameraMatrix;
            PrepareBufferSampler.End();
        }

        /// <summary>
        /// ワールド座標からスクリーン座標をバッチ処理で計算する。
        /// </summary>
        /// <param name="positions">ワールド座標のリスト</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="projectionMatrix">カメラの投影行列</param>
        /// <param name="worldMatrix">カメラのワールド行列</param>
        /// <param name="uiPositions">更新されるUI座標</param>
        [BurstCompile]
        private static void CalculateUiPosition(
            in NativeArray<float3> positions, in int2 screenSize,
            in Matrix4x4 projectionMatrix, in Matrix4x4 worldMatrix,
            ref NativeArray<float3> uiPositions)
        {
            // カメラ行列を準備する
            var matrix = projectionMatrix * worldMatrix;

            for (var i = 0; i < positions.Length; i++)
            {
                // UI座標をスクリーン座標に変換して適用する
                CameraUtility.WorldToScreenPosition(
                    positions[i], matrix, screenSize, out var screenPosition);
                uiPositions[i] = screenPosition;
            }
        }

        /// <summary>
        /// UI要素が画面境界内にあるかどうかをバッチ処理で判定する。
        /// </summary>
        /// <param name="uiPositions">UI座標</param>
        /// <param name="uiSizes">UIサイズ</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="uiVisible">UIの表示状態のリスト</param>
        [BurstCompile]
        private static void CalculateUiVisible(
            in NativeArray<float3> uiPositions, in NativeArray<float2> uiSizes,
            in int2 screenSize, ref NativeArray<bool> uiVisible)
        {
            for (var i = 0; i < uiPositions.Length; i++)
            {
                uiVisible[i] = InRange(uiPositions[i], uiSizes[i], screenSize);
            }
        }

        /// <summary>
        /// UI要素が画面境界内にあるかどうかをチェックする。
        /// </summary>
        /// <param name="screenPosition">UI座標</param>
        /// <param name="uiSize">UIサイズ</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <returns>画面内にある場合はTrue</returns>
        [BurstCompile]
        private static bool InRange(in float3 screenPosition, in float2 uiSize, in int2 screenSize)
        {
            return !(screenPosition.z < 0 ||
                     screenPosition.x + uiSize.x < 0 ||
                     screenPosition.x - uiSize.x > screenSize.x ||
                     screenPosition.y + uiSize.y < 0 ||
                     screenPosition.y - uiSize.y > screenSize.y);
        }

        #endregion
    }
}