using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou.TCC.Integration
{
    /// <summary>
    /// Bool型の入力アクション．
    /// ※値は管理クラスによって毎フレーム更新される．
    /// </summary>
    [System.Serializable]
    public struct BoolAction
    {
        /// <summary>現在の値．</summary>
        public bool value;

        // 前回値のキャッシュ
        private bool _previousValue;
        private bool _previousStarted;
        private bool _previousCanceled;


        // ----------------------------------------------------------------------------

        #region Property

        /// <summary>
        /// このフレームにボタンが押下されたかどうか．
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// このフレームにボタンが解放されたかどうか．
        /// </summary>
        public bool Canceled { get; private set; }

        /// <summary>
        /// 最後に"Started"フラグが立ってからの経過時間．
        /// </summary>
        public float StartedElapsedTime { get; private set; }

        /// <summary>
        /// 最後に"Canceled"フラグが立ってからの経過時間．
        /// </summary>
        public float CanceledElapsedTime { get; private set; }

        // ----- 

        /// <summary>
        /// アクションがtrueに設定されてからの経過時間．
        /// </summary>
        public float ActiveTime { get; private set; }

        /// <summary>
        /// アクションがfalseに設定されてからの経過時間．
        /// </summary>
        public float InactiveTime { get; private set; }

        /// <summary>
        /// The last "ActiveTime" value registered by this action (on Canceled).
        /// </summary>
        public float LastActiveTime { get; private set; }

        /// <summary>
        /// The last "InactiveTime" value registered by this action (on Started).
        /// </summary>
        public float LastInactiveTime { get; private set; }

        #endregion


        // ----------------------------------------------------------------------------

        #region Public Method

        /// <summary>
        /// 値の初期化
        /// </summary>
        public void Initialize()
        {
            StartedElapsedTime = Mathf.Infinity;
            CanceledElapsedTime = Mathf.Infinity;

            value = false;
            _previousValue = false;
            _previousStarted = false;
            _previousCanceled = false;
        }

        /// <summary>
        /// 入力アクションのリセット
        /// </summary>
        public void Reset()
        {
            Started = false;
            Canceled = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float dt)
        {
            // 状態変化の検出（フレーム間の遷移を検出）
            // |= を使用することで、Reset()が呼ばれるまでフラグを保持
            // これにより、Update() が複数回呼ばれてもStarted/Canceledイベントを見逃さない
            Started |= !_previousValue && value;      // 前回false → 今回true
            Canceled |= _previousValue && !value;     // 前回true → 今回false

            // 経過時間の更新
            StartedElapsedTime += dt;
            CanceledElapsedTime += dt;

            // このフレームに押下された場合
            if (Started)
            {
                StartedElapsedTime = 0f;

                if (!_previousStarted)
                {
                    LastActiveTime = 0f;
                    LastInactiveTime = InactiveTime;
                }
            }

            // このフレームに解放された場合
            if (Canceled)
            {
                CanceledElapsedTime = 0f;

                if (!_previousCanceled)
                {
                    LastActiveTime = ActiveTime;
                    LastInactiveTime = 0f;
                }
            }

            // 経過時間の更新
            if (value)
            {
                ActiveTime += dt;
                InactiveTime = 0f;
            }
            else
            {
                ActiveTime = 0f;
                InactiveTime += dt;
            }

            // 前フレーム情報として格納
            _previousValue = value;
            _previousStarted = Started;
            _previousCanceled = Canceled;
        }

        #endregion
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BoolAction))]
    internal class BoolActionEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                SerializedProperty value = property.FindPropertyRelative("value");

                // label
                Rect fieldRect = position;
                fieldRect.height = EditorGUIUtility.singleLineHeight;
                fieldRect.width = 100;
                EditorGUI.LabelField(fieldRect, label);

                // field
                fieldRect.x += 110;
                EditorGUI.PropertyField(fieldRect, value, GUIContent.none);
            }
        }
    }
#endif
}