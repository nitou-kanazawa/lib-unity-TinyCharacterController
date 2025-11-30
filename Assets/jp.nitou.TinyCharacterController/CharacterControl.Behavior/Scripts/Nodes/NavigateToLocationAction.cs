using System;
using Unity.Properties;
using Unity.Behavior;
using UnityEngine;
using Nitou.TCC.CharacterControl.Control;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Action = Unity.Behavior.Action;

namespace Nitou.TinyCharacterController.CharacterControl.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Navigate To Location (TCC)",
        description: "Navigates a GameObject to a specified position using NavMeshAgent." +
                     "\nIf NavMeshAgent is not available on the [Character] or its children, moves the Agent using its transform.",
        story: "[Character] Navigates to [Location]",
        category: "Action/Navigation",
        id: "e3a0faff55347cf4876aa2a189a93462"
    )]
    public partial class NavigateToLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<CharacterSettings> Character;
        [SerializeReference] public BlackboardVariable<Vector3> Location;

        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);

        // Priority
        [SerializeReference] public BlackboardVariable<int> ActivePriority = new(1);
        [SerializeReference] public BlackboardVariable<int> DeactivePriority = new(-1);

        private ITransform _characterTransform;
        private MoveNavmeshControl _moveControl;
        private bool _isInitialized = false;


        #region Lifecycle Events

        protected override Status OnStart()
        {
            if (Character.Value == null || Location.Value == null)
            {
                return Status.Failure;
            }

            // Initialize components on first execution
            if (!_isInitialized)
            {
                if (!InitializeComponents())
                    return Status.Failure;
            }

            // Check if already at destination
            float distance = GetDistanceToLocation();
            if (distance <= DistanceThreshold.Value)
                return Status.Success;

            // Activate navigation
            _moveControl.MovePriority = ActivePriority.Value;
            _moveControl.TurnPriority = ActivePriority.Value;
            _moveControl.SetTargetPosition(Location.Value);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Character.Value == null || Location.Value == null)
            {
                return Status.Failure;
            }

            // Check if reached destination
            float distance = GetDistanceToLocation();
            if (distance <= DistanceThreshold.Value)
                return Status.Success;

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (_moveControl != null)
            {
                _moveControl.MovePriority = DeactivePriority.Value;
                _moveControl.TurnPriority = DeactivePriority.Value;
            }
        }

        #endregion


        /// <summary>
        /// Initialize component references (called once on first execution)
        /// </summary>
        private bool InitializeComponents()
        {
            if (!Character.Value.TryGetComponent(out _characterTransform))
            {
                Debug.LogError($"[NavigateToLocationAction] ITransform not found on {Character.Value.name}");
                return false;
            }

            if (!Character.Value.TryGetActorComponent(CharacterComponent.Control, out _moveControl))
            {
                Debug.LogError($"[NavigateToLocationAction] MoveNavmeshControl not found on {Character.Value.name}");
                return false;
            }

            _isInitialized = true;
            return true;
        }

        /// <summary>
        /// Calculate horizontal (XZ plane) distance to target location
        /// </summary>
        private float GetDistanceToLocation()
        {
            Vector3 agentPos = _characterTransform.Position;
            Vector3 targetPos = Location.Value;

            // Calculate distance on XZ plane only (ignore Y axis for top-down games)
            return Vector2.Distance(
                new Vector2(agentPos.x, agentPos.z),
                new Vector2(targetPos.x, targetPos.z)
            );
        }
    }
}
