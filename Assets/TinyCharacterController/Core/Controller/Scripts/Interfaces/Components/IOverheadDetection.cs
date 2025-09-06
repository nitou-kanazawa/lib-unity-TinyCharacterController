using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components{
    
    /// <summary>
    /// This interface returns the behavior when an object above is in contact.
    ///
    /// It returns whether the object is in contact or not, the position of the contact with the object,
    /// and if implemented, it returns the GameObject above.
    /// </summary>
    public interface IOverheadDetection{

        /// <summary>
        /// 頭上の接触があるかどうか．
        /// </summary>
        bool IsHeadContact { get;  }

        /// <summary>
        /// Returns True if the collider is within the range of the decision.
        /// Different from IsHitCollision, it is used to determine if there is an object overhead.
        /// </summary>
        bool IsObjectOverhead { get; }

        /// <summary>
        /// Return the collider that the head is in contact with.
        /// If nothing is in contact, return null.
        /// </summary>
        GameObject ContactedObject { get;  }
    
        /// <summary>
        /// 接触位置．
        /// If no contact is made, the maximum distance at which contact can be made.
        /// </summary>
        Vector3 ContactPoint { get; }
    }
}
