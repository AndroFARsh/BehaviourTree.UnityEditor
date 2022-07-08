using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BTree.Runtime;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BTree.Editor
{
  public static class Tools
  {
    [MenuItem("Window/AI/BTree Editor...")]
    public static void OpenWindow()
    {
      var wnd = EditorWindow.GetWindow<B3EditorWindow>(
        utility: false,
        title: "BehTree Editor",
        focus: true
      );
      wnd.minSize = new Vector2(800, 600);
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
      var obj = EditorUtility.InstanceIDToObject(instanceId);
      if (!obj.IsB3Asset()) return false;

      Selection.activeObject = obj;
      OpenWindow();
      return true;
    }
    
    public static B3 ToB3(this TextAsset asset) =>
      asset.IsB3Asset()
        ? JsonConvert.DeserializeObject<B3>(asset.text,
          new JsonSerializerSettings
          {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter> { new B3Converter() }
          }
        )
        : null;

    public static bool IsB3Asset(this Object obj) =>
      obj && AssetDatabase.GetAssetPath(obj).EndsWith(Constants.Extension);

    public static TextAsset LoadB3Asset(string path)
    {
      var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
      return asset;;
    } 
    
    public static TextAsset CreateB3Asset(string path) => WriteToFile(CreateB3(), path);

    public static TextAsset UpdateB3Asset(this TextAsset asset, B3 tree) => 
      WriteToFile(tree, AssetDatabase.GetAssetPath(asset));

    public static Node CreateNode(Type type)
    {
      if (Activator.CreateInstance(type) is not Node node)
        throw new Exception($"Create node of type {type}");

      return node;
    }
    
    public static FieldInfo[] GetNodeFields(this Node node, bool excludeHideInInspector = false)
    {
      // BindingFlags.NonPublic |
      return node.GetType()
        .GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Where(f => !Attribute.IsDefined(f, typeof(NonSerializedAttribute)))
        .Where(f => !(excludeHideInInspector && Attribute.IsDefined(f, typeof(HideInInspector))))
        .Reverse()
        .ToArray();
    }

    private static TextAsset WriteToFile(B3 tree, string path)
    {
      using var writer = new StreamWriter(path);
      writer.Write(ToJson(tree));
      writer.Flush();
      
      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
      return asset;
    }

    private static B3 CreateB3()
    {
      var tree = new B3();
      tree.Register(new RootNode(), Vector2.zero);
      return tree;
    }
    
    private static string ToJson(B3 tree) =>
      JsonConvert.SerializeObject(tree, Formatting.Indented,
        new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All,
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
          Converters = new List<JsonConverter> { new B3Converter() }
        }
      );
  }
}