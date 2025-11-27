using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Core
{
    [DefaultExecutionOrder(Order.EarlyUpdateBrain)]
    [AddComponentMenu(MenuList.MenuBrain + "Early Update")]
    public sealed class EarlyUpdateBrain : EarlyUpdateBrainBase
    {
        private void Update() => OnUpdate();
    }
}