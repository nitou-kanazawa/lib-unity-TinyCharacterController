using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Inputs
{
    public enum UpdateMode
    {
        /// <summary>
        /// FixedUpdateで入力を扱う場合のモード（デフォルト）
        /// </summary>
        FixedUpdate,

        /// <summary>
        /// Updateで入力を扱う場合のモード
        /// </summary>
        Update
    }


    /// <summary>
    /// 入力処理とActorActionsの更新を管理する基底コンポーネント
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class ActorBrain : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [Tooltip("Indicates when actions should be consumed.\n\n" +
                 "FixedUpdate (recommended): use this when the gameplay logic needs to run during FixedUpdate.\n\n" +
                 "Update: use this when the gameplay logic needs to run every frame during Update.")]
        [SerializeField, Indent]
        [DisableInPlayMode]  // 実行時変更による状態不整合を防止
        UpdateMode _updateMode = UpdateMode.FixedUpdate;

        [TitleGroup("Actor Actions")] [HideLabel, ReadOnly] [SerializeField, Indent]
        protected ActorActions _characterActions = new();

        // FixedUpdateモード用の初回フラグ
        private bool _firstUpdateFlag = false;


        /// <summary>
        /// 現在設定されているアクション
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
            // ゲームが一時停止中の場合は処理をスキップ
            if (Time.timeScale == 0) return;

            float dt = Time.deltaTime;

            if (_updateMode == UpdateMode.FixedUpdate)
            {
                // FixedUpdateモードの場合、FixedUpdateの最初のUpdate呼び出しでリセット
                if (_firstUpdateFlag)
                {
                    _firstUpdateFlag = false;
                    _characterActions.Reset();
                }
            }
            else
            {
                // Updateモードの場合、毎フレームリセット
                _characterActions.Reset();
            }

            // 更新処理
            UpdateBrainValues(dt);
        }

        protected virtual void FixedUpdate()
        {
            _firstUpdateFlag = true;
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                // FixedUpdateモードの場合、正しいdeltaTimeを渡す
                UpdateBrainValues(Time.fixedDeltaTime);
            }
        }


        // ----------------------------------------------------------------------------
        // Protected Method

        /// <summary>
        /// 更新処理の実行
        /// </summary>
        protected abstract void UpdateBrainValues(float dt);
    }
}