using System.Collections.Generic;

namespace Nitou.TCC.CharacterControl.Interfaces.Components{

    public interface IComponentCondition{
        void OnConditionCheck(List<string> messageList);
    }
}