using System;
using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components
{
    /// <summary>
    /// 地面オブジェクトに関する情報にアクセスするためのインターフェース.
    /// </summary>
    public interface IGroundObject
    {
        /// <summary>
        /// 地面オブジェクトが変化したときに true を返す.
        /// </summary>
        bool IsChangeGroundObject { get; }

        /// <summary>
        /// 現在の地面オブジェクト.
        /// </summary>
        GameObject GroundObject { get; }

        /// <summary>
        /// 現在の地面コライダー.
        /// </summary>
        Collider GroundCollider { get; }

        /// <summary>
        /// 地面オブジェクトの変化を通知するストリーム.
        /// </summary>
        IObservable<GameObject> OnGrounObjectChanged { get; }
    }
}