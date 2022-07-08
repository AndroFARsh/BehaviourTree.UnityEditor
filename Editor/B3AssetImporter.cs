using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace BTree.Editor
{
  [ScriptedImporter( 1, Constants.Extension)]
  public class B3AssetImporter : ScriptedImporter {
    public override void OnImportAsset( AssetImportContext context ) {
      var texture = (Texture2D)EditorGUIUtility.Load(Constants.Icon);
      var asset = new TextAsset(File.ReadAllText(context.assetPath));
      
      context.AddObjectToAsset( "b3", asset, texture);
      context.SetMainObject(asset);
    }
  }
}