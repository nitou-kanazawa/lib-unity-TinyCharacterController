using UnityEngine;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// This abstract class contains all the input methods that are used by the character brain. 
    /// This is the base class for all the input detection methods available.
    /// </summary>
    public abstract class InputHandler : MonoBehaviour
    {
        // ----------------------------------------------------------------------------
        // Public Method (値の取得)

        /// <summary>
        /// Bool型アクションの取得
        /// </summary>
        public abstract bool GetBool(string actionName);

        /// <summary>
        /// Float型アクションの取得
        /// </summary>
        public abstract float GetFloat(string actionName);

        /// <summary>
        /// Vector2型アクションの取得
        /// </summary>
        public abstract Vector2 GetVector2(string actionName);
    }
}