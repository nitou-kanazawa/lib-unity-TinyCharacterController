#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

// REF:
//  - hatena: TypeCacheを使って指定したアトリビュートが付いている型を高速に取得する https://light11.hatenadiary.com/entry/2021/04/26/202054
//  - はなちる: TypeCacheを用いて"特定の属性でマークされている型やメソッド" や "特定のクラスやインターフェイスから派生する型"に素早くアクセスする https://www.hanachiru-blog.com/entry/2023/12/08/120000

namespace Nitou.CustomHierarchy.EditorSctipts
{
    internal static class HierarchyDrawerInitializer
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // 指定した型を継承またはまたはインターフェースを実装している型を取得
            var drawers = TypeCache.GetTypesDerivedFrom<HierarchyDrawer>()
                                   .Where(x => !x.IsAbstract)
                                   .Select(x => (HierarchyDrawer)Activator.CreateInstance(x));

            // 一括で描画処理を登録
            foreach (var drawer in drawers)
            {
                EditorApplication.hierarchyWindowItemOnGUI += drawer.OnGUI;
            }
        }
    }
}
#endif