using Nitou.TCC.Controller.Shared;
using UnityEngine;

namespace Nitou.TCC.Controller.Core
{
    [DefaultExecutionOrder(Order.EarlyUpdateBrain)]
    [AddComponentMenu(MenuList.MenuBrain + "Early Update")]
    public sealed class EarlyUpdateBrain : EarlyUpdateBrainBase
    {
        private void Update() => OnUpdate();
    }
}