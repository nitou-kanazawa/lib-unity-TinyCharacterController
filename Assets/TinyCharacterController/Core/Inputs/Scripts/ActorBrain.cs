using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Inputs
{
    public enum UpdateMode
    {
        /// <summary>
        /// FixedUpdate�œ��͂��g���ꍇ�̃��[�h�i�f�t�H���g�j
        /// </summary>
        FixedUpdate,

        /// <summary>
        /// 
        /// </summary>
        Update
    }


    /// <summary>
    /// ���͏��<see cref="CharacterActions"/>�̍X�V�����ƊO�����J��S���R���|�[�l���g�D
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class ActorBrain : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [Tooltip("Indicates when actions should be consumed.\n\n" +
                 "FixedUpdate (recommended): use this when the gameplay logic needs to run during FixedUpdate.\n\n" +
                 "Update: use this when the gameplay logic needs to run every frame during Update.")]
        [SerializeField, Indent]
        UpdateMode _updateMode = UpdateMode.FixedUpdate;

        [TitleGroup("Actor Actions")] [HideLabel, ReadOnly] [SerializeField, Indent]
        protected ActorActions _characterActions = new();

        // ���������p
        private bool _firstUpdateFlag = false;


        /// <summary>
        /// ���݁C�ݒ肳��Ă���A�N�V�����D
        /// </summary>
        public ActorActions CharacterActions => _characterActions;


        /// ----------------------------------------------------------------------------
        // Lifecycle Events
        protected virtual void OnEnable()
        {
            _characterActions.InitializeActions();
            _characterActions.Reset();
        }

        protected virtual void OnDisable()
        {
            _characterActions.Reset();
        }

        protected virtual void Update()
        {
            float dt = Time.deltaTime;

            if (_updateMode == UpdateMode.FixedUpdate)
            {
                if (_firstUpdateFlag)
                {
                    _firstUpdateFlag = false;
                    _characterActions.Reset();
                }
            }
            else
            {
                _characterActions.Reset();
            }

            // �X�V����
            UpdateBrainValues(dt);
        }

        protected virtual void FixedUpdate()
        {
            _firstUpdateFlag = true;
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                UpdateBrainValues(0f);
            }
        }


        // ----------------------------------------------------------------------------
        // Protected Method 

        /// <summary>
        /// �X�V�����̎��s�D
        /// </summary>
        protected abstract void UpdateBrainValues(float dt);
    }
}