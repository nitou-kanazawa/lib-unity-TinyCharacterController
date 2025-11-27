using UnityEngine;

namespace Nitou.TCC.Integration
{
    /// <summary>
    /// この抽象クラスは，キャラクターブレインで使用されるすべての入力メソッドを含んでいます．
    /// これは，利用可能なすべての入力検出メソッドの基底クラスです．
    /// </summary>
    public abstract class InputHandler : MonoBehaviour
    {
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