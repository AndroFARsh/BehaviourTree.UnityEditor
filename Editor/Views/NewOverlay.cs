using System;
using System.IO;
using BTree.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTree.Editor.Views
{
  public class NewOverlay : VisualElement
  {
    public new class UxmlFactory : UxmlFactory<NewOverlay, UxmlTraits>
    {
    }

    private const string CreateButton = "CreateButton";
    private const string LocationPath = "LocationPath";
    private const string TreeName = "TreeName";

    private TextField treeNameField;
    private TextField locationPathField;
    private Button createNewTreeButton;

    private event Action<string> onSuccess;

    public NewOverlay()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.NewOverlay);
      visualTree.CloneTree(this);

      treeNameField = this.Q<TextField>(TreeName);
      locationPathField = this.Q<TextField>(LocationPath);

      createNewTreeButton = this.Q<Button>(CreateButton);
      createNewTreeButton.clicked += () => onSuccess?.Invoke(
        Path.Join(
          locationPathField.value, 
          $"{treeNameField.value}.{Constants.Extension}")
      );

      style.visibility = Visibility.Hidden;
    }

    public void Show(Action<string> successCallback)
    {
      ClearAllOnSuccess();
      // if (onSuccess != null)
      //   foreach (var d in onSuccess.GetInvocationList())
      //     onSuccess -= (Action<string, string>)d;

      onSuccess += successCallback;
      style.visibility = Visibility.Visible;
    }

    public void Hide()
    {
      ClearAllOnSuccess();
      style.visibility = Visibility.Hidden;
    }

    private void ClearAllOnSuccess()
    {
      if (onSuccess != null)
        foreach (var d in onSuccess.GetInvocationList())
          Delegate.RemoveAll(onSuccess, d);
    }

    // private static void CreateNewTree(string dirPath, string assetName)
    // {
    //   // var path = System.IO.Path.Combine(dirPath, $"{assetName}.json");
    //   // var tree =  new BehTree();
    //   // tree.Register(new RootNode());
    //
    //   // AssetDatabase.CreateAsset(tree, path);
    //   //
    //   // Selection.activeObject = tree;
    //   // EditorGUIUtility.PingObject(tree);
    //
    //   // return tree;
    // }
  }
}