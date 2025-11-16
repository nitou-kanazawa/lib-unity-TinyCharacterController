using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// 入力の記録と再生システム
    /// デバッグ、テスト、チュートリアル用
    /// </summary>
    [Serializable]
    public class InputRecorder
    {
        /// <summary>
        /// 入力フレーム（特定時刻の入力状態スナップショット）
        /// </summary>
        [Serializable]
        private struct InputFrame
        {
            public float timestamp;
            public ActorActionsSnapshot actions;

            public InputFrame(float time, ActorActions actions)
            {
                timestamp = time;
                this.actions = new ActorActionsSnapshot(actions);
            }
        }

        /// <summary>
        /// ActorActionsのシリアライズ可能なスナップショット
        /// </summary>
        [Serializable]
        private struct ActorActionsSnapshot
        {
            // Bool values
            public bool jump;
            public bool attack1;
            public bool attack2;
            public bool lockon;
            public bool interact;
            public bool dodge;
            public bool guard;
            public bool run;

            // Float values
            public float pitch;
            public float roll;

            // Vector2 values
            public Vector2 movement;

            public ActorActionsSnapshot(ActorActions actions)
            {
                jump = actions.jump.value;
                attack1 = actions.attack1.value;
                attack2 = actions.attack2.value;
                lockon = actions.lockon.value;
                interact = actions.interact.value;
                dodge = actions.dodge.value;
                guard = actions.guard.value;
                run = actions.run.value;

                pitch = actions.pitch.value;
                roll = actions.roll.value;

                movement = actions.movement.value;
            }

            public void ApplyTo(ref ActorActions actions)
            {
                actions.jump.value = jump;
                actions.attack1.value = attack1;
                actions.attack2.value = attack2;
                actions.lockon.value = lockon;
                actions.interact.value = interact;
                actions.dodge.value = dodge;
                actions.guard.value = guard;
                actions.run.value = run;

                actions.pitch.value = pitch;
                actions.roll.value = roll;

                actions.movement.value = movement;
            }
        }

        /// <summary>
        /// 記録データ
        /// </summary>
        [Serializable]
        private class RecordingData
        {
            public List<InputFrame> frames = new();
            public float startTime;
            public float duration;
        }

        // 記録管理
        private RecordingData _recording = new();
        private bool _isRecording;
        private bool _isPlaying;
        private int _playbackIndex;
        private float _playbackStartTime;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// 記録中かどうか
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// 再生中かどうか
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 記録されているフレーム数
        /// </summary>
        public int FrameCount => _recording.frames.Count;

        /// <summary>
        /// 記録の長さ（秒）
        /// </summary>
        public float Duration => _recording.duration;


        // ----------------------------------------------------------------------------
        // Recording

        /// <summary>
        /// 記録を開始
        /// </summary>
        public void StartRecording()
        {
            _recording = new RecordingData
            {
                startTime = Time.time
            };
            _isRecording = true;
            _isPlaying = false;
        }

        /// <summary>
        /// 記録を停止
        /// </summary>
        public void StopRecording()
        {
            if (_isRecording)
            {
                _recording.duration = Time.time - _recording.startTime;
                _isRecording = false;
            }
        }

        /// <summary>
        /// 入力を記録
        /// </summary>
        public void Record(ActorActions actions)
        {
            if (!_isRecording) return;

            float timestamp = Time.time - _recording.startTime;
            _recording.frames.Add(new InputFrame(timestamp, actions));
        }


        // ----------------------------------------------------------------------------
        // Playback

        /// <summary>
        /// 再生を開始
        /// </summary>
        public void StartPlayback()
        {
            if (_recording.frames.Count == 0)
            {
                Debug.LogWarning("[InputRecorder] No recording to playback");
                return;
            }

            _isPlaying = true;
            _isRecording = false;
            _playbackIndex = 0;
            _playbackStartTime = Time.time;
        }

        /// <summary>
        /// 再生を停止
        /// </summary>
        public void StopPlayback()
        {
            _isPlaying = false;
            _playbackIndex = 0;
        }

        /// <summary>
        /// 現在の再生位置に対応する入力を取得
        /// </summary>
        /// <param name="actions">出力先のActorActions</param>
        /// <returns>再生継続中ならtrue、終了したらfalse</returns>
        public bool TryGetPlaybackActions(ref ActorActions actions)
        {
            if (!_isPlaying || _recording.frames.Count == 0)
                return false;

            float playbackTime = Time.time - _playbackStartTime;

            // 再生終了チェック
            if (playbackTime > _recording.duration)
            {
                StopPlayback();
                return false;
            }

            // 現在時刻に最も近いフレームを探す
            while (_playbackIndex < _recording.frames.Count - 1 &&
                   _recording.frames[_playbackIndex + 1].timestamp <= playbackTime)
            {
                _playbackIndex++;
            }

            // フレームを適用
            if (_playbackIndex < _recording.frames.Count)
            {
                _recording.frames[_playbackIndex].actions.ApplyTo(ref actions);
                return true;
            }

            return false;
        }


        // ----------------------------------------------------------------------------
        // File I/O

        /// <summary>
        /// 記録をJSON形式でファイルに保存
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                string json = JsonUtility.ToJson(_recording, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"[InputRecorder] Recording saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[InputRecorder] Failed to save recording: {e.Message}");
            }
        }

        /// <summary>
        /// JSON形式のファイルから記録を読み込み
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"[InputRecorder] File not found: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                _recording = JsonUtility.FromJson<RecordingData>(json);
                _isRecording = false;
                _isPlaying = false;

                Debug.Log($"[InputRecorder] Recording loaded: {FrameCount} frames, {Duration:F2}s");
            }
            catch (Exception e)
            {
                Debug.LogError($"[InputRecorder] Failed to load recording: {e.Message}");
            }
        }


        // ----------------------------------------------------------------------------
        // Utility

        /// <summary>
        /// 記録をクリア
        /// </summary>
        public void Clear()
        {
            _recording = new RecordingData();
            _isRecording = false;
            _isPlaying = false;
            _playbackIndex = 0;
        }

        /// <summary>
        /// デバッグ情報を取得
        /// </summary>
        public string GetDebugInfo()
        {
            string status = _isRecording ? "RECORDING" : _isPlaying ? "PLAYING" : "IDLE";
            return $"InputRecorder [{status}]: {FrameCount} frames, {Duration:F2}s";
        }
    }
}
