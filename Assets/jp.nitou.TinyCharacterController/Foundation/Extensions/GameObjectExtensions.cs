using UnityEngine;

namespace Nitou.TCC.Foundation
{
    public static class GameObjectExtensions
    {
        // ----------------------------------------------------------------------------
        // レイヤー

        /// <summary>
        /// 対象のレイヤーに含まれているかを調べる拡張メソッド
        /// </summary>
        public static bool IsInLayerMask(this GameObject self, LayerMask layerMask) {
            int objLayerMask = (1 << self.layer);
            return (layerMask.value & objLayerMask) > 0;
        }

        /// <summary>
        /// レイヤーを設定する拡張メソッド
        /// </summary>
        public static void SetLayer(this GameObject self, string layerName) {
            self.layer = LayerMask.NameToLayer(layerName);
        }

        /// <summary>
        /// レイヤーを設定する拡張メソッド
        /// </summary>
        public static void SetLayerRecursively(this GameObject self, int layer) {
            self.layer = layer;

            // 子のレイヤーにも設定する
            foreach (Transform childTransform in self.transform) {
                SetLayerRecursively(childTransform.gameObject, layer);
            }
        }

        /// <summary>
        /// レイヤーを設定する拡張メソッド
        /// </summary>
        public static void SetLayerRecursively(this GameObject self, string layerName) {
            self.SetLayerRecursively(LayerMask.NameToLayer(layerName));
        }


        // ----------------------------------------------------------------------------
        // タグ

        /// <summary>
        /// 指定したタグ群に含まれているか調べる拡張メソッド
        /// </summary>
        public static bool ContainTag(this GameObject self, in string[] tagArray) {
            // ※タグが含まれない場合，trueを返す
            if (tagArray == null || tagArray.Length == 0) return true;

            for (var i = 0; i < tagArray.Length; i++) {
                if (self.CompareTag(tagArray[i])) return true;
            }

            return false;
        }


        // ----------------------------------------------------------------------------
        // コンポーネント

        /// <summary>
        /// 指定した型のコンポーネントを取得する拡張メソッド．
        /// 存在しない場合は追加して返す．
        /// </summary>
        /// <typeparam name="T">取得または追加するコンポーネントの型．</typeparam>
        /// <param name="self">対象のGameObject．</param>
        /// <returns>取得または追加されたコンポーネント．</returns>
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component {
            if (self.TryGetComponent<T>(out var component)) {
                return component;
            }
            return self.AddComponent<T>();
        }
    }
}
