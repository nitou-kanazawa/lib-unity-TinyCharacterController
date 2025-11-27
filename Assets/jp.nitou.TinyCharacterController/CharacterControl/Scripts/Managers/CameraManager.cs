using System.Collections.Generic;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using UnityEngine;
using Nitou.TCC.Foundation;

namespace Nitou.TCC.CharacterControl.Core {

    internal sealed class CameraManager {

        private readonly List<ICameraUpdate> _cameraUpdates = new();

        /// <summary>
        /// 初期化処理．
        /// </summary>
        /// <param name="obj">ルートオブジェクト．</param>
        public void Initialize(GameObject obj) {
            obj.GetComponentsInChildren(_cameraUpdates);
        }

        /// <summary>
        /// 更新処理．
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Process(float deltaTime) {
            using var _ = new ProfilerScope("Camera Update");

            // No limitation by priority.
            // The final orientation is determined by Cinemachine.
            foreach (var cameraUpdate in _cameraUpdates) {
                cameraUpdate.OnUpdate(deltaTime);
            }
        }
    }
}