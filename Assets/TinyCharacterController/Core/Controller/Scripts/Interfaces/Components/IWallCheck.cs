using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components {

    /// <summary>
    /// Interface for accessing wall detection.
    /// </summary>
    public interface IWallCheck {

        /// <summary>
        /// 壁と接触しているかどうか．
        /// </summary>
        public bool IsContact { get; }

        /// <summary>
        /// 壁の法線ベクトル．
        /// </summary>
        public Vector3 Normal { get; }
    }
}