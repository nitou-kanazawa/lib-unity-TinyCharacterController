using UnityEngine;

namespace Nitou.TCC.Inputs
{
    public abstract class EnemyBehaviourBase : MonoBehaviour
    {
        protected ActorActions _actions;

        /// <summary>
        /// �A�N�V�����}�b�v
        /// </summary>
        public ActorActions CharacterActions => _actions;

        /// <summary>
        /// 
        /// </summary>
        //public LevelActor CharacterActor { get; private set; }

        // ----------------------------------------------------------------------------
        // MonoBehaviour Method 
        protected virtual void Awake()
        {
            //CharacterActor = gameObject.GetComponentInBranch<CharacterActor>();
        }

        protected virtual void Reset()
        {
            _actions.Reset();
        }


        // ----------------------------------------------------------------------------
        // Public Method 

        /// <summary>
        /// �ړ����͂�ݒ肷��
        /// </summary>
        public void SetMovement(Vector3 direction)
        {
            //(var x, _, var z) = Vector3.ProjectOnPlane(direction, CharacterActor.Up).ClampMagnitude01();
            //_actions.movement.value = new Vector2(x, z);
        }

        /// <summary>
        /// �U�����͂�ݒ肷��
        /// </summary>
        public void RaiseAttack()
        {
            _actions.attack1.value = true;
        }
    }
}