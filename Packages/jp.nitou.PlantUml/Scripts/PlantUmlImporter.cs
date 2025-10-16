using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

// REF:
// - Unity Manual: [ Managing importers with scripts](https://docs.unity3d.com/6000.2/Documentation/Manual/ScriptedImporters.html)
// - LIGHT11: [【Unity】Unity非対応の拡張子のファイルをアセットとして取り扱えるScripted Importerの使い方](https://light11.hatenadiary.com/entry/2021/06/14/202730)

namespace Nitou.PlantUml
{
    [ScriptedImporter(version: 1, ext: "puml")]
    internal class PlantUmlImporter : ScriptedImporter
    {
        public string AdditionalText = string.Empty;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var text = File.ReadAllText(ctx.assetPath);
            
            // 
            var data = ScriptableObject.CreateInstance<PlantUmlAsset>();
            data.content = text;
            
            // アセット登録
            ctx.AddObjectToAsset(identifier: "MainAsset", obj: data);
            ctx.SetMainObject(data);
            
        }
    }
}