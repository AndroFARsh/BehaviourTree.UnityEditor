using System.IO;
using BTree.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace BTree.Editor
{
  public class Constants
  {
    private static readonly string FolderPath = GetFolderPath();
    
    public const string Extension = "b3";
    
    public static readonly string B3EditorWindow = $"{FolderPath}/UIBuilder/B3EditorWindow.uxml";
    public static readonly string B3EditorWindowStyle = $"{FolderPath}/UIBuilder/B3EditorWindowStyle.uss";
    public static readonly string NewOverlay = $"{FolderPath}/UIBuilder/NewOverlay.uxml";
    public static readonly string NodeView = $"{FolderPath}/UIBuilder/NodeView.uxml";
    public static readonly string Icon = $"{FolderPath}/Icon.png";
    
    public static readonly string ActionNodeScriptTemplate = $"{FolderPath}/ScriptTemplates/101-B3__ActionNode-NewActionNode.cs.txt";
    public static readonly string CompositeNodeScriptTemplate = $"{FolderPath}/ScriptTemplates/101-B3__CompositeNode-NewCompositeNode.cs.txt";
    public static readonly string DecoratorNodeScriptTemplate = $"{FolderPath}/ScriptTemplates/101-B3__DecoratorNode-NewDecoratorNode.cs.txt";
    public static readonly string B3ScriptTemplate = $"{FolderPath}/ScriptTemplates/101-B3__NewB3.cs.txt";

    private static string GetFolderPath()
    {
      // hack: its only one way to get current editor script path. :(
      var pathHelper = ScriptableObject.CreateInstance<B3EditorSettings>();
      
      var monoScript = MonoScript.FromScriptableObject(pathHelper);
      var monoPath = AssetDatabase.GetAssetPath(monoScript);
      var rootDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(monoPath)));
      
      ScriptableObject.DestroyImmediate(pathHelper);
      return rootDir;
    }
  }
}