using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// 
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
    [DisallowMultipleComponent]
    public sealed class PlayerBrain : ActorBrain
    {
        [TitleGroup("Settings")] [DisableInPlayMode] [SerializeField, Indent]
        InputHandler _inputHandler = null;


        // ----------------------------------------------------------------------------
        // Public Method 

        /// <summary>
        /// ���̓n���h���[��ݒ肷��
        /// </summary>
        public void SetInputHandler(InputHandler inputHandler)
        {
            if (inputHandler == null) return;

            _inputHandler = inputHandler;
            _characterActions.Reset();
        }


        // ----------------------------------------------------------------------------
        // Protected Method 

        /// <summary>
        /// �X�V�����̎��s
        /// </summary>
        protected override void UpdateBrainValues(float dt)
        {
            if (Time.timeScale == 0) return;

            // �l�̍X�V
            _characterActions.SetValues(_inputHandler);
            _characterActions.Update(dt);
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_inputHandler == null)
            {
                _inputHandler = gameObject.GetComponent<InputSystemHandler>();
            }
        }
#endif
    }
}