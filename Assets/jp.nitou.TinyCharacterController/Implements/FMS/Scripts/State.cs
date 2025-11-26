using System;
using UnityEngine;

namespace Nitou.TCC.Implements
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> を継承したステート基底クラス．
    /// </summary>
    public abstract class State<TContext, TParam> : MonoBehaviour
        where TContext : class
        where TParam : StateSetupParam
    {
        /// <summary>
        /// このステートが所属するステートマシンが持っているコンテキスト．
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        /// ステートマシン．
        /// </summary>
        protected SimpleFMS<TContext, TParam> StateMachine { get; private set; }

        /// <summary>
        /// 初期化が完了しているかどうか．
        /// </summary>
        public bool IsInitialized { get; private set; } = false;


        // ----------------------------------------------------------------------------
        // Public Method (初期化処理)

        /// <summary>
        /// 初期化処理．
        /// </summary>
        public void Initialize(SimpleFMS<TContext, TParam> machine, TParam param)
        {
            if (machine == null)
                throw new ArgumentNullException(nameof(machine));
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            this.StateMachine = machine;
            this.Context = machine.Context;

            // 派生クラスの初期化処理
            OnInitialized(param);

            // フラグ更新
            IsInitialized = true;
        }

        /// <summary>
        /// 派生クラス用の初期化処理．
        /// </summary>
        protected virtual void OnInitialized(TParam param) { }


        // ----------------------------------------------------------------------------
        // Public Method (ステート遷移条件)

        /// <summary>
        /// ステート遷移条件を満たしているか確認する．
        /// </summary>
        public virtual void CheckExitTransition() { }

        /// <summary>
        /// 他ステートからの遷移を受け入れられるか確認する．
        /// 遷移可能ならtrue，不可能ならfalseを返す．
        /// </summary>  
        public virtual bool CheckEnterTransition(State<TContext, TParam> fromState) => true;


        // ----------------------------------------------------------------------------
        // Public Method (ステート遷移処理)

        /// <summary>
        /// ステート開始時の処理
        /// </summary>
        public virtual void EnterBehaviour(float dt, State<TContext, TParam> fromState) { }

        /// <summary>
        /// ステート終了時の処理
        /// </summary>
        public virtual void ExitBehaviour(float dt, State<TContext, TParam> toState) { }


        // ----------------------------------------------------------------------------
        // Public Method (FixedUpdate処理)

        /// <summary>
        /// ステート更新処理
        /// </summary>
        public virtual void PreUpdateBehaviour(float dt) { }

        /// <summary>
        /// ステート更新処理
        /// </summary>
        public abstract void UpdateBehaviour(float dt);

        /// <summary>
        /// ステート更新処理
        /// </summary>
        public virtual void PostUpdateBehaviour(float dt) { }


        // ----------------------------------------------------------------------------
        // Public Method (Others)
        public virtual string GetName() => this.GetType().Name;

        public virtual string GetInfo() => "";

        public virtual Color GetColor() => Color.white;
    }
}