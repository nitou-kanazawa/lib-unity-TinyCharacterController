using Nitou.TCC.Controller.Shared;
using UnityEngine;

namespace Nitou.TCC.Controller.Core
{
    [DefaultExecutionOrder(Order.EarlyUpdateBrain)]
    [AddComponentMenu(MenuList.MenuBrain + "Early Fixed Update")]
    public sealed class EarlyFixedUpdateBrain : EarlyUpdateBrainBase
    {
        private void FixedUpdate() => OnUpdate();
    }
}