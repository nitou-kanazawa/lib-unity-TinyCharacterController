using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Core
{
    [DefaultExecutionOrder(Order.EarlyUpdateBrain)]
    [AddComponentMenu(MenuList.MenuBrain + "Early Fixed Update")]
    public sealed class EarlyFixedUpdateBrain : EarlyUpdateBrainBase
    {
        private void FixedUpdate() => OnUpdate();
    }
}