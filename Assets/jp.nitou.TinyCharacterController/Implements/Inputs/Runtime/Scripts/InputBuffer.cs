using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// 入力バッファリングシステム
    /// 先行入力、コマンド入力検出などをサポート
    /// </summary>
    public class InputBuffer
    {
        /// <summary>
        /// 入力イベント
        /// </summary>
        private struct InputEvent
        {
            public string actionName;
            public float timestamp;
            public bool pressed;  // true: pressed, false: released

            public InputEvent(string name, float time, bool isPressed)
            {
                actionName = name;
                timestamp = time;
                pressed = isPressed;
            }
        }

        // バッファ管理
        private readonly Queue<InputEvent> _buffer = new();
        private readonly float _bufferDuration;


        // ----------------------------------------------------------------------------
        // Constructor

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bufferDuration">バッファ保持時間（秒）</param>
        public InputBuffer(float bufferDuration = 0.2f)
        {
            _bufferDuration = bufferDuration;
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 入力イベントを記録
        /// </summary>
        public void RecordPressed(string actionName)
        {
            _buffer.Enqueue(new InputEvent(actionName, Time.time, true));
        }

        /// <summary>
        /// 入力解放イベントを記録
        /// </summary>
        public void RecordReleased(string actionName)
        {
            _buffer.Enqueue(new InputEvent(actionName, Time.time, false));
        }

        /// <summary>
        /// 更新処理（古い入力を削除）
        /// </summary>
        public void Update()
        {
            float cutoffTime = Time.time - _bufferDuration;

            while (_buffer.Count > 0 && _buffer.Peek().timestamp < cutoffTime)
            {
                _buffer.Dequeue();
            }
        }

        /// <summary>
        /// バッファをクリア
        /// </summary>
        public void Clear()
        {
            _buffer.Clear();
        }

        /// <summary>
        /// 指定時間内にアクションが押されたかチェック
        /// </summary>
        /// <param name="actionName">アクション名</param>
        /// <param name="withinTime">許容時間（秒）</param>
        /// <returns>押されていればtrue</returns>
        public bool WasPressed(string actionName, float withinTime = 0.2f)
        {
            float cutoffTime = Time.time - withinTime;

            return _buffer.Any(e =>
                e.actionName == actionName &&
                e.pressed &&
                e.timestamp >= cutoffTime);
        }

        /// <summary>
        /// 指定時間内にアクションが解放されたかチェック
        /// </summary>
        public bool WasReleased(string actionName, float withinTime = 0.2f)
        {
            float cutoffTime = Time.time - withinTime;

            return _buffer.Any(e =>
                e.actionName == actionName &&
                !e.pressed &&
                e.timestamp >= cutoffTime);
        }

        /// <summary>
        /// 最後にアクションが押された時刻を取得
        /// </summary>
        /// <returns>タイムスタンプ（見つからない場合は-1）</returns>
        public float GetLastPressedTime(string actionName)
        {
            var lastEvent = _buffer
                .Where(e => e.actionName == actionName && e.pressed)
                .OrderByDescending(e => e.timestamp)
                .FirstOrDefault();

            return lastEvent.timestamp > 0 ? lastEvent.timestamp : -1f;
        }

        /// <summary>
        /// コマンドシーケンスを検出
        /// </summary>
        /// <param name="sequence">アクション名のシーケンス</param>
        /// <param name="maxDuration">シーケンス全体の許容時間（秒）</param>
        /// <returns>シーケンスが検出されたらtrue</returns>
        public bool DetectSequence(string[] sequence, float maxDuration = 1.0f)
        {
            if (sequence == null || sequence.Length == 0)
                return false;

            float cutoffTime = Time.time - maxDuration;
            var recentEvents = _buffer
                .Where(e => e.pressed && e.timestamp >= cutoffTime)
                .OrderBy(e => e.timestamp)
                .ToList();

            if (recentEvents.Count < sequence.Length)
                return false;

            // シーケンスマッチング（後方一致）
            int matchIndex = sequence.Length - 1;
            for (int i = recentEvents.Count - 1; i >= 0 && matchIndex >= 0; i--)
            {
                if (recentEvents[i].actionName == sequence[matchIndex])
                {
                    matchIndex--;
                }
            }

            return matchIndex < 0;  // 全てマッチした
        }

        /// <summary>
        /// デバッグ用：バッファの内容を文字列で取得
        /// </summary>
        public string GetDebugInfo()
        {
            var events = _buffer.ToList();
            if (events.Count == 0)
                return "Buffer: Empty";

            var info = $"Buffer ({events.Count} events):\n";
            foreach (var e in events)
            {
                float age = Time.time - e.timestamp;
                string status = e.pressed ? "PRESS" : "RELEASE";
                info += $"  [{age:F2}s ago] {e.actionName} {status}\n";
            }

            return info;
        }
    }
}
