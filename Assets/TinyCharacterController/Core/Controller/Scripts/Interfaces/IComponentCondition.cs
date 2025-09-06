using System.Collections.Generic;

namespace Nitou.TCC.Controller.Interfaces.Components{

    public interface IComponentCondition{
        void OnConditionCheck(List<string> messageList);
    }
}