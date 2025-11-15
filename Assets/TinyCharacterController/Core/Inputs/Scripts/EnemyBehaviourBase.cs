using UnityEngine;

namespace Nitou.TCC.Inputs
{
    public abstract class EnemyBehaviourBase : MonoBehaviour
    {
        protected ActorActions _actions = new();

        /// <summary>
        /// アクションマップ
        /// </summary>
        public ActorActions CharacterActions => _actions;

        // ----------------------------------------------------------------------------
        // MonoBehaviour Method

        protected virtual void Awake()
        {
            // ActorActionsの初期化を保証
            _actions.InitializeActions();
        }

        protected virtual void OnEnable()
        {
            _actions.Reset();
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 移動入力を設定する
        /// </summary>
        public void SetMovement(Vector3 direction)
        {
            // TODO: 実装が必要な場合はここに追加
            // 現在は未実装
        }

        /// <summary>
        /// 攻撃入力を設定する
        /// </summary>
        public void RaiseAttack()
        {
            _actions.attack1.value = true;
        }
    }
}