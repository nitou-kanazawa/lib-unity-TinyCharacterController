using System.Collections.Generic;
using Nitou.TCC.Controller.Interfaces.Core;
using UnityEngine;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// Brain の更新前に実行される EarlyUpdate の基底クラス．
    /// </summary>
    public abstract class EarlyUpdateBrainBase : MonoBehaviour
    {
        private readonly List<IEarlyUpdateComponent> _updates = new();

        private void Awake()
        {
            GatherComponents();
        }

        protected void GatherComponents()
        {
            GetComponentsInChildren(_updates);
            _updates.Sort((a, b) => a.Order - b.Order);
        }

        protected void OnUpdate()
        {
            // FixedUpdate のタイミングで実行される場合、deltaTime は FixedUpdate の値を返す
            var deltaTime = Time.deltaTime;

            foreach (var update in _updates)
            {
                update.OnUpdate(deltaTime);
            }
        }
    }
}