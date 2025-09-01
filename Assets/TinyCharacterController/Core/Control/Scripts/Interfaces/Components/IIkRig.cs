using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components
{
    public interface IIkRig
    {
        /// <summary>
        /// リグの重み．
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// Determines if the rig is usable.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 初期化処理．
        /// </summary>
        void Initialize(Animator animator);

        /// <summary>
        /// Calculate weights and other factors before calculating IK.
        /// Update is not used to ensure IK works even when the object is inactive.
        /// </summary>
        void OnPreProcess(float deltaTime);

        /// <summary>
        /// Called at the timing of OnAnimatorIk.
        /// </summary>
        void OnIkProcess(Vector3 offset);
    }
}