using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Check;
using Nitou.TCC.CharacterControl.Control;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.Integration;
using R3;
using R3.Triggers;

namespace Project.Actor
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ActorCore : MonoBehaviour
    {
        [Title("Behaviour")]
        [SerializeField, Indent] CharacterSettings _settings;

        [SerializeField, Indent] ActorBrain _brain;
        [SerializeField, Indent] ActorFMS _statemachine;
        


        [Title("Animation")]
        [SerializeField, Indent] ActorAnimation _animation;

        //
        private CompositeDisposable _disposables;


        /// ----------------------------------------------------------------------------
        // Property

        public CharacterSettings Settings => _settings;

        public CursorLookControl CursorLookControl { get; private set; }

        public GroundCheck GroundCheck { get; private set; }

        public bool IsSetupped { get; private set; }

        /// <summary>
        /// �ڒn��Ԃ��ǂ����D
        /// </summary>
        public bool IsGrounded => GroundCheck.IsOnGround;

        public bool IsLockOn {get; internal set; }
        

        /// ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Start()
        {
            Setup();
        }

        private void OnDestroy()
        {
            Teardown();
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// ����������
        /// </summary>
        public void Setup()
        {
            if (IsSetupped) return;

            // Checks
            GroundCheck = _settings.GetActorComponent<GroundCheck>(CharacterComponent.Check);
            
            // Controls
            CursorLookControl = _settings.GetActorComponent<CursorLookControl>(CharacterComponent.Control);
            
            // StateMachine
            var param = new ActorFMS.SetupParam(_settings, _brain, _animation);
            _statemachine.Initialize(this, param);

            // �X�V�����̊J�n
            _disposables = new CompositeDisposable();

            // 更新処理
            this.UpdateAsObservable()
                .Subscribe(_ => _statemachine.UpdateProcess())
                .AddTo(_disposables);

            IsSetupped = true;
        }

        public void Teardown()
        {
            if (!IsSetupped) return;

            _disposables?.Dispose();
            _disposables = null;

            IsSetupped = false;
        }

        [Obsolete]
        public void SetBodyColor(Color color)
        {
            var meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                renderer.material.color = color;
            }
        }
    }
}