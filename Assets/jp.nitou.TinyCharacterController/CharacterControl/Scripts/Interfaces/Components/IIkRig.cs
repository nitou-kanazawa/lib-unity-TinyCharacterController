using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    public interface IIkRig
    {
        /// <summary>
        /// リグの重み．
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// リグが使用可能かどうか．
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 初期化処理．
        /// </summary>
        void Initialize(Animator animator);

        /// <summary>
        /// IK 計算前に重みなどを計算する．
        /// オブジェクトが非アクティブの場合でも IK が機能するよう Update は使用しない．
        /// </summary>
        void OnPreProcess(float deltaTime);

        /// <summary>
        /// OnAnimatorIk のタイミングで呼び出される．
        /// </summary>
        void OnIkProcess(Vector3 offset);
    }
}