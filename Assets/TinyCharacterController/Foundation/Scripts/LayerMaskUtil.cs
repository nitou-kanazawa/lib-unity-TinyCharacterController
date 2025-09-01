using UnityEngine;

namespace Nitou.TCC.Utils
{
    public static class LayerMaskUtil
    {
        public static LayerMask AllIn => -1;

        public static LayerMask Empty => 1;

        public static LayerMask Only(int layer) => 1 << layer;

        public static LayerMask Only(string layerName) => 1 << LayerMask.NameToLayer(layerName);

        public static LayerMask OnlyDefault()=> Only("Default");
    }
}
