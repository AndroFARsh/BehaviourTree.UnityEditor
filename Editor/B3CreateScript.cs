using System.Collections.Generic;
using System.IO;
using System.Text;
using BTree.Editor.Settings;
using BTree.Runtime.Composites;
using BTree.Runtime.Decorators;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace BTree.Runtime
{
  public class B3CreateScript
  {
    private struct ScriptTemplate
    {
      public TextAsset templateFile;
      public string subFolder;
    }

    private static readonly ScriptTemplate[] scriptFileAssets =
      CreateScriptFileAssets(B3EditorSettings.GetOrCreateSettings());

    public static void CreateNewB3Script(TextAsset asset, B3 tree)
    { 
      var data = scriptFileAssets[0];
      var path = Path.Join(data.subFolder, $"{asset.name}.cs");
      var icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
      var content = data.templateFile.text;
      content = content.Replace("#SCRIPTNAME#", asset.name);
      content = content.Replace("#CONTENT#", ToContent(tree));

      AssetDatabase.DeleteAsset(path);
      ProjectWindowUtil.CreateAssetWithContent(path, content, icon);
    }

    private static string ToContent(B3 tree)
    {
      var nodeToIndex = new Dictionary<Node, int>();
      var builder = new StringBuilder();
      var index = 0;
      var rootIndex = 0;

      // construct node
      foreach (var node in tree)
      {
        var nodeJson = JsonUtility.ToJson(node);
        var guardedNodeJson = nodeJson
          .Replace("\"", "\\\"")
          .Replace("\n", "\\\n");
        builder.Append($"    var node{index} = CreateNode<{node.GetType().FullName}>(\"")
          .Append(guardedNodeJson)
          .Append("\"); \n");
        nodeToIndex.Add(node, index);

        if (node is RootNode)
        {
          rootIndex = index;
        }

        index++;
      }

      // link node
      foreach (var node in tree)
      {
        switch (node)
        {
          case RootNode rootNode:
            if (rootNode.Child != null)
              builder.Append($"    node{nodeToIndex[node]}.AddChild(node{nodeToIndex[rootNode.Child]});\n");
            break;
          case DecoratorNode decoratorNode:
            if (decoratorNode.Child != null)
              builder.Append($"    node{nodeToIndex[node]}.AddChild(node{nodeToIndex[decoratorNode.Child]});\n");
            break;
          case CompositeNode compositeNode:
            foreach (var child in compositeNode.Children)
              builder.Append($"    node{nodeToIndex[node]}.AddChild(node{nodeToIndex[child]});\n");
            break;
        }
      }

      builder.Append($"    root = node{rootIndex};");
      return builder.ToString();
    }

    private static ScriptTemplate[] CreateScriptFileAssets(B3EditorSettings settings)
      => new ScriptTemplate[]
      {
        new()
        {
          templateFile = settings.scriptTemplateB3,
          subFolder = $"{settings.newNodeBasePath}"
        },
      };
  }
}