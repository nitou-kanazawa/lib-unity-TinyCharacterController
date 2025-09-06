using System.Collections.Generic;
using Nitou.TCC.Controller.Interfaces.Core;
using UnityEngine;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// 
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
            // If executed at the timing of FixedUpdate, deltaTime returns the value of FixedUpdate.
            var deltaTime = Time.deltaTime;

            foreach (var update in _updates)
            {
                update.OnUpdate(deltaTime);
            }
        }
    }
}