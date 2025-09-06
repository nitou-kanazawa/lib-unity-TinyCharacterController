using Nitou.TCC.Controller.Core;
using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components {

    /// <summary>
    /// Callback called when CharacterSettings values change.
    /// Mainly used for changing CharacterController or Collider sizes.
    /// </summary>
    public interface IActorSettingUpdateReceiver {

        /// <summary>
        /// CharacterSettings values have changed
        /// </summary>
        void OnUpdateSettings(ActorSettings settings);
    }
}