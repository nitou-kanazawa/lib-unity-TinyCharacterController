using UnityEngine;

namespace Nitou.AnimationModule
{
    internal static class AssetMenu
    {
        // ----------------------------------------------------------------------------
        // 接頭辞
        public static class Prefix
        {
            /// <summary>
            /// スクリプタブルオブジェクト．
            /// </summary>
            public const string ScriptableObject = "Scriptable Objects/";

            /// <summary>
            /// イベントチャンネル．
            /// </summary>
            public const string EventChannel = "Event Channel/";

            /// <summary>
            /// キャラクターモデル．
            /// </summary>
            public const string ActorModelInfo = ScriptableObject + "Actor Model/";

            /// <summary>
            /// クレジット情報．
            /// </summary>
            public const string CreditInfo = ScriptableObject + "Credit Info/";

            /// <summary>
            /// シーン情報．
            /// </summary>
            public const string SceneInfo = ScriptableObject + "Scene Info/";

            // ----- 

            /// <summary>
            /// アニメーションデータ
            /// </summary>
            public const string AnimationData = "Animation Data/";

            /// <summary>
            /// キャラクター操作関連
            /// </summary>
            public const string CharacterControl = "Character Control/";
        }


        // ----------------------------------------------------------------------------
        // 接尾辞
        public static class Suffix {
        }


        // ----------------------------------------------------------------------------
        // 表示順
        public static class Order
        {
            public const int VeryEarly = 100;
            public const int Early = 50;
            public const int Normal = 0;
            public const int Late = -50;
            public const int VeryLate = -100;
        }
    }
}