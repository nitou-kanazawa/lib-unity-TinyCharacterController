using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou.DesignPattern
{
    /// <summary>
    /// ステートの初期化パラメータ．
    /// </summary>
    public class StateSetupParam { }


    /// <summary>
    /// <see cref="MonoBehaviour"/> ベースのシンプルなステートマシン．
    /// </summary>
    public abstract partial class SimpleFMS<TContext, TParam> : MonoBehaviour
        where TContext : class
        where TParam : StateSetupParam
    {
        /// <summary>
        /// ステートマシンが保持しているコンテキスト
        /// </summary>
        public TContext Context { get; private set; }


        // ----------------------------------------------------------------------------
        // ステート

        /// <summary>
        ///  初期ステート
        /// </summary>
        [TitleGroup("State Information")]
        [SerializeField, Indent]
        protected State<TContext, TParam> _initialState = null;

        /// <summary>
        ///  現在のステート
        /// </summary>
        [TitleGroup("State Information")]
        [ShowInInspector, ReadOnly, Indent]
        public State<TContext, TParam> CurrentState { get; private set; }

        /// <summary>
        ///  前回のステート
        /// </summary>
        [TitleGroup("State Information")]
        [ShowInInspector, ReadOnly, Indent]
        public State<TContext, TParam> PreviousState { get; private set; }


        /// <summary>
        /// 現在のステートを開始してからの時間
        /// </summary>
        public float StateElapsedTime => (Time.time - _stateEnteredTime);

        /// <summary>
        /// ステート遷移時の通知
        /// </summary>
        public System.IObservable<(State<TContext, TParam> current, State<TContext, TParam> next)> OnStateChange => _stateChangeSubject;

        protected Subject<(State<TContext, TParam> current, State<TContext, TParam> next)> _stateChangeSubject = new();


        // ----------------------------------------------------------------------------
        // その他


        /// <summary>
        /// 初期化が完了しているかどうか
        /// </summary>
        [TitleGroup("Others")]
        [ShowInInspector, ReadOnly, Indent]
        public bool IsInitialized { get; private set; }

        // 内部処理用
        protected float _stateEnteredTime = float.MinValue; // 現在のステートを開始した時刻

        // 登録ステート
        protected Dictionary<string, State<TContext, TParam>> _states = new();

        // 積まれている遷移リクエスト
        protected Queue<State<TContext, TParam>> _transitionsQueue = new();


        /// ----------------------------------------------------------------------------
        // Lifecycle Events
        private void OnDestroy()
        {
            _stateChangeSubject.Dispose();
        }


        // ----------------------------------------------------------------------------
        // Public Method 

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(TContext context, TParam param)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("the state machine has already been set upp.");
                return;
            }

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // コンポーネント
            this.Context = context;

            // ステート登録
            RegisterStates(param);

            if (_states.Count == 0 || _initialState == null)
            {
                Debug.LogWarning("State registration failed.");
                IsInitialized = false;
                return;
            }

            // 派生クラスの初期化処理
            OnInitialize(param);

            // 初期ステートの開始
            CurrentState = _initialState;
            _stateEnteredTime = Time.time;
            CurrentState.EnterBehaviour(0f, CurrentState);

            // フラグ更新
            IsInitialized = true;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public virtual void UpdateProcess()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Statemachin is not initialized yet.");
                return;
            }

            ;
            if (CurrentState == null || !CurrentState.isActiveAndEnabled)
            {
                Debug.LogWarning("Current state is invalid.");
                return;
            }

            // ステート遷移
            CheckAndExecuteStateTransition(); // 遷移の実行
            _transitionsQueue.Clear(); // 遷移リクエストのクリア

            // ステート更新処理
            var dt = Time.deltaTime;
            CurrentState.PreUpdateBehaviour(dt);
            CurrentState.UpdateBehaviour(dt);
            CurrentState.PostUpdateBehaviour(dt);
        }


        // ----------------------------------------------------------------------------
        // Public Method (ステート情報)

        /// <summary>
        /// Searches for a particular state.
        /// </summary>
        public State<TContext, TParam> GetState(string stateName)
        {
            _states.TryGetValue(stateName, out State<TContext, TParam> state);
            return state;
        }

        /// <summary>
        /// Searches for a particular state.
        /// </summary>
        public TDerivedState GetState<TDerivedState>() where TDerivedState : State<TContext, TParam>
        {
            string stateName = typeof(TDerivedState).Name;
            return GetState(stateName) as TDerivedState;
        }


        // ----------------------------------------------------------------------------
        // Public Method (ステート遷移)

        /// <summary>
        /// 指定したステートへの遷移リクエスト
        /// </summary>
        public void EnqueueTransition(State<TContext, TParam> state)
        {
            if (state == null) return;

            _transitionsQueue.Enqueue(state);
        }

        /// <summary>
        /// ステート遷移リクエストを積む
        /// </summary>
        public void EnqueueTransition<TDerivedState>() where TDerivedState : State<TContext, TParam> =>
            EnqueueTransition(GetState<TDerivedState>());

        /// <summary>
        /// 前回のステートへの遷移リクエスト
        /// </summary>
        public void EnqueueTransitionToPreviousState() =>
            EnqueueTransition(PreviousState);

        /// <summary>
        /// 強制的に指定したステートへ遷移する
        /// </summary>
        public void ForceState(State<TContext, TParam> state)
        {
            if (state == null) return;

            // イベント通知
            _stateChangeSubject.OnNext((CurrentState, state));

            // ステート情報の更新
            PreviousState = CurrentState;
            CurrentState = state;
            _stateEnteredTime = Time.time;

            // ステート遷移処理
            var dt = Time.deltaTime;
            PreviousState.ExitBehaviour(dt, CurrentState);　// 現ステートの終了処理
            CurrentState.EnterBehaviour(dt, PreviousState);　// 新ステートの終了処理
        }

        /// <summary>
        /// 強制的に指定したステートへ遷移する
        /// </summary>
        public void ForceState<TDerivedState>() where TDerivedState : State<TContext, TParam> =>
            ForceState(GetState<TDerivedState>());


        // ----------------------------------------------------------------------------
        // Protected Method (ステート)

        /// <summary>
        /// ステートマシンンにステートを登録する
        /// </summary>
        protected void RegisterStates(TParam param)
        {
            // ※自身gameobjectから取得する
            var statesArray = gameObject.GetComponents<State<TContext, TParam>>();

            for (int i = 0; i < statesArray.Length; i++)
            {
                var state = statesArray[i];
                string stateName = state.GetType().Name;

                // The state is already included, ignore it!
                if (GetState(stateName) != null)
                {
                    Debug.Log("Warning: GameObject " + state.gameObject.name + " has the state " + stateName + " repeated in the hierarchy.");
                    continue;
                }

                state.Initialize(this, param);
                _states.Add(stateName, state);
            }
        }

        /// <summary>
        /// 遷移フラグを確認して遷移を実行する
        /// ※FixedUpdate()で呼び出される
        /// </summary>
        protected void CheckAndExecuteStateTransition()
        {
            // 遷移リクエストの取得
            CurrentState.CheckExitTransition(); // ※現在ステート側

            // 遷移リクエストの検証
            while (_transitionsQueue.Count != 0)
            {
                var newState = _transitionsQueue.Dequeue();
                if (newState == null || !newState.enabled) continue;

                // 遷移可能かの検証
                bool success = newState.CheckEnterTransition(CurrentState); // ※新ステート側
                if (success)
                {
                    // ステート遷移
                    ForceState(newState);
                    return;
                }
            }
        }


        /// ----------------------------------------------------------------------------
        // Protected Method 
        protected virtual void OnInitialize(TParam param) { }


        /// ----------------------------------------------------------------------------
#if UNITY_EDITOR

        // [参考]
        //  LIGHT11: InspectorからSceneビューのカメラを操作する https://light11.hatenadiary.com/entry/2019/11/16/224438

        //[TitleGroup("Others")]
        [SerializeField, Indent] bool _drawGizmo = true;

        private void OnDrawGizmos()
        {
            if (CurrentState == null || !_drawGizmo) return;
    
            // Unity6では SceneView.currentDrawingSceneView が null になる場合があるため、より安全なチェック
            var currentSceneView = SceneView.currentDrawingSceneView;
            if (currentSceneView == null || currentSceneView.camera == null) return;

            // カメラとオブジェクトの距離を計算
            var cameraPosition = currentSceneView.camera.transform.position;
            var objectPosition = transform.position;
            var distance = Vector3.Distance(cameraPosition, objectPosition);
    
            // 一定距離内なら表示（20ユニット以内）
            if (distance <= 20f)
            {
                // Unity6対応：Handles.colorの設定
                var originalColor = Handles.color;
                Handles.color = CurrentState.GetColor();
        
                // カメラの前方向を考慮した描画位置の計算
                var cameraForward = currentSceneView.camera.transform.forward;
                var drawPos = objectPosition + cameraForward * (-0.5f); // 距離を調整
        
                // ラベルの描画（Unity6でも互換性維持）
                Handles.Label(drawPos, $"{CurrentState.GetName()}");
        
                // 色を元に戻す
                Handles.color = originalColor;
            }
        }
#endif
    }
}