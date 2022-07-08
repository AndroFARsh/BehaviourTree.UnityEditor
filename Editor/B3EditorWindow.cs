using System;
using System.Collections.Generic;
using System.IO;
using BTree.Editor.Views;
using BTree.Editor.Views.Inspector;
using BTree.Editor.Views.Nodes;
using BTree.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static System.String;

namespace BTree.Editor
{
  public class B3EditorWindow : EditorWindow
  {
    private B3GraphView treeGraphView;
    private InspectorView inspectorView;
    private ToolbarButton toolbarButton;
    private ToolbarMenu toolbarMenu;
    private NewOverlay overlay;

    // private TextAsset asset;
    
    private static List<string> GetAllBehTreeFiles(string dir)
    {
      var files = new List<string>(Directory.GetFiles($"{dir}/", $"*.{Constants.Extension}"));
      foreach (var subDir in Directory.GetDirectories(dir))
        files.AddRange(GetAllBehTreeFiles(subDir));  
      
      return files;
    }

    private void CreateGUI()
    {
      // Each editor window contains a root VisualElement object
      var root = rootVisualElement;

      // Import UXML
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.B3EditorWindow);
      visualTree.CloneTree(root);
      
      // Main BehTreeView
      treeGraphView = root.Q<B3GraphView>();
      treeGraphView.nodeSelectListener += OnNodeSelectionChanged;

      // Inspector
      inspectorView = root.Q<InspectorView>();
      
      // New BehTree overlay
      overlay = root.Q<NewOverlay>();

      // Toolbar assets menu
      toolbarMenu = root.Q<ToolbarMenu>();
      
      toolbarButton = root.Q<ToolbarButton>();
      toolbarButton.clicked += () => treeGraphView?.GenerateB3Script();

      OnSelectionChange();
    }


    private void CreateTitle(string assetName)
    {
      titleContent = new GUIContent( !IsNullOrEmpty(assetName) ? $"BehTree Editor ({assetName})" : "BehTree Editor");
    }

    private void CreateAssetMenu()
    {
      if (toolbarMenu == null) return;
      
      toolbarMenu.menu.MenuItems()?.Clear();

      GetAllBehTreeFiles(Application.dataPath)
        .ForEach(filePath =>
        {
          toolbarMenu.menu.AppendAction(
            Path.GetFileName(filePath),
            (a) =>
            {
              var assetPath = Application.dataPath.Replace("/Assets", "");
              var relativePath = Path.GetRelativePath(assetPath, filePath);
              Selection.activeObject = Tools.LoadB3Asset(relativePath);
            });
        });
      toolbarMenu.menu.AppendSeparator();
      toolbarMenu.menu.AppendAction(
        "New Tree...",
        (a) => overlay.Show(filePath =>
        {
          Selection.activeObject = Tools.CreateB3Asset(filePath);
          overlay.Hide();
        })
      );
    }

    private void OnEnable()
    {
      EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
      EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
      switch (obj)
      {
        case PlayModeStateChange.EnteredEditMode:
          OnSelectionChange();
          break;
        case PlayModeStateChange.ExitingEditMode:
          break;
        case PlayModeStateChange.EnteredPlayMode:
          OnSelectionChange();
          break;
        case PlayModeStateChange.ExitingPlayMode:
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
      }
    }

    private void OnSelectionChange()
    {
      EditorApplication.delayCall += () =>
      {
        // if (Application.isPlaying && Selection.activeGameObject)
        // {
        //   var provider = Selection.activeGameObject.GetComponent<IB3Provider>();
        //   if (provider != null)
        //   {
        //     SelectAsset(provider);
        //     return;
        //   }
        // }

        var asset = Selection.activeObject as TextAsset;
        if (!asset || asset.IsB3Asset())
        {
          SelectAsset(asset);
        }
      };
    }

    private void SelectAsset(TextAsset asset)
    {
      if (toolbarButton == null) return;
      
      CreateTitle(asset ? asset.name : "");
      CreateAssetMenu();
      
      toolbarButton.style.visibility = asset ? Visibility.Visible: Visibility.Hidden;
      treeGraphView.PopulateView(asset);

      EditorApplication.delayCall += () => { treeGraphView?.FrameAll(); };
    }
    
    private void OnNodeSelectionChanged(NodeView nodeView) => inspectorView.UpdateSelection(nodeView);
    
    private void OnInspectorUpdate() => treeGraphView?.UpdateNodeStates();
  }
}