using UnityEngine;

namespace Nitou.PlantUml
{
    public sealed class PlantUmlAsset : ScriptableObject
    {
        public string content = string.Empty;
        
        [HideInInspector]
        public Texture2D cachedDiagram;
    }
}
