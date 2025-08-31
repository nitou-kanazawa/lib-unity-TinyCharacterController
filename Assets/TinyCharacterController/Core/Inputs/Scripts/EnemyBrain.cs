using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// 
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
    [DisallowMultipleComponent]
    public class EnemyBrain : ActorBrain
    {
        [TitleGroup("Settings")] public EnemyBehaviourBase _behaviour;


        // ----------------------------------------------------------------------------
        // Private Method 

        /// <summary>
        /// �X�V�����̎��s
        /// </summary>
        protected override void UpdateBrainValues(float dt)
        {
            if (Time.timeScale == 0) return;

            // �l�̍X�V
            _characterActions.SetValues(_behaviour.CharacterActions);
            _characterActions.Update(dt);
        }


        /// ----------------------------------------------------------------------------
#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (_behaviour == null)
            {
                _behaviour = gameObject.GetComponent<EnemyBehaviourBase>();
            }
        }
#endif
    }
}