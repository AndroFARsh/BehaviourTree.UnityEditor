using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BTree.Editor.Settings;
using BTree.Runtime;
using BTree.Runtime.Actions;
using BTree.Runtime.Composites;
using BTree.Runtime.Decorators;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTree.Editor.Views
{
  public static class BehaviourTreeContextualMenu
  {
    private struct ScriptTemplate
    {
      public TextAsset templateFile;
      public string defaultFileName;
      public string subFolder;
    }

    private static readonly ScriptTemplate[] scriptFileAssets =
      CreateScriptFileAssets(B3EditorSettings.GetOrCreateSettings());

    public static void BuildNewScriptMenu(this B3GraphView graphView, ContextualMenuPopulateEvent evt)
    {
      if (evt.menu.MenuItems() != null && evt.menu.MenuItems().Count > 0)
        evt.menu.AppendSeparator();
      
      // New script functions
      evt.menu.AppendAction("Create Script.../New Action Node", (a) => CreateNewActionNodeScript());
      evt.menu.AppendAction("Create Script.../New Composite Node", (a) => CreateNewCompositeNodeScript());
      evt.menu.AppendAction("Create Script.../New Decorator Node", (a) => CreateNewDecoratorNodeScript());
    }
    
    public static void BuildAddNodeMenu(this B3GraphView graphView, ContextualMenuPopulateEvent evt,
      Action<Type, Vector2> handler)
    {
      if (evt.menu.MenuItems() != null && evt.menu.MenuItems().Count > 0)
        evt.menu.AppendSeparator();
      
      var nodePosition = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, evt.localMousePosition);

      CreateNodeMenuItem<ActionNode>(evt, "Actions", nodePosition, handler);
      CreateNodeMenuItem<CompositeNode>(evt, "Composites", nodePosition, handler);
      CreateNodeMenuItem<DecoratorNode>(evt, "Decorators", nodePosition, handler);
    }

    [MenuItem("Assets/Create/BehTree/New Decorator Node")]
    private static void CreateNewDecoratorNodeScript() => CreateNewScript(scriptFileAssets[2]);
    
    [MenuItem("Assets/Create/BehTree/New Composite Node")]
    private static void CreateNewCompositeNodeScript() => CreateNewScript(scriptFileAssets[1]);
    
    [MenuItem("Assets/Create/BehTree/New Action Node")]
    private static void CreateNewActionNodeScript() => CreateNewScript(scriptFileAssets[0]);

    private static ScriptTemplate[] CreateScriptFileAssets(B3EditorSettings settings)
      => new ScriptTemplate[]
      {
        new()
        {
          templateFile = settings.scriptTemplateActionNode,
          defaultFileName = "NewActionNode.cs",
          subFolder = $"{settings.newNodeBasePath}/Actions"
        },
        new()
        {
          templateFile = settings.scriptTemplateCompositeNode,
          defaultFileName = "NewCompositeNode.cs",
          subFolder = $"{settings.newNodeBasePath}/Composites"
        },
        new()
        {
          templateFile = settings.scriptTemplateDecoratorNode,
          defaultFileName = "NewDecoratorNode.cs",
          subFolder = $"{settings.newNodeBasePath}/Decorators"
        }
      };

    private static void CreateNodeMenuItem<T>(ContextualMenuPopulateEvent evt, string baseName, Vector2 nodePosition,
      Action<Type, Vector2> handler)
      where T : Node
    {
      var types = TypeCache.GetTypesDerivedFrom<T>();
      foreach (var type in types)
        evt.menu.AppendAction($"{baseName}/{type.Name}", (a) => handler.Invoke(type, nodePosition));
    }

    private static void CreateNewScript(ScriptTemplate template)
    {
      SelectFolder($"{template.subFolder}");
      var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
      ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
    }

    private static void SelectFolder(string path)
    {
      // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
      // Check the path has no '/' at the end, if it does remove it,
      // Obviously in this example it doesn't but it might
      // if your getting the path some other way.
      if (path[^1] == '/')
        path = path[..^1];

      // Load object
      var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

      // Select the object in the project folder
      Selection.activeObject = obj;

      // Also flash the folder yellow to highlight it
      EditorGUIUtility.PingObject(obj);
    }
  }
}