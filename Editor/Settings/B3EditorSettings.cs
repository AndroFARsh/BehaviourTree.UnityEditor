using System.IO;
using UnityEditor;
using UnityEngine;

namespace BTree.Editor.Settings
{
  // Create a new type of Settings Asset.
  class B3EditorSettings : ScriptableObject
  {
    public TextAsset scriptTemplateActionNode;
    public TextAsset scriptTemplateCompositeNode;
    public TextAsset scriptTemplateDecoratorNode;
    public TextAsset scriptTemplateB3;
    
    public string newNodeBasePath = "Assets/";


    private void OnValidate()
    {
      if (!scriptTemplateActionNode) scriptTemplateActionNode = AssetDatabase.LoadAssetAtPath<TextAsset>(Constants.ActionNodeScriptTemplate);
      if (!scriptTemplateCompositeNode) scriptTemplateCompositeNode = AssetDatabase.LoadAssetAtPath<TextAsset>(Constants.CompositeNodeScriptTemplate);
      if (!scriptTemplateDecoratorNode) scriptTemplateDecoratorNode = AssetDatabase.LoadAssetAtPath<TextAsset>(Constants.DecoratorNodeScriptTemplate);
      if (!scriptTemplateB3) scriptTemplateB3 = AssetDatabase.LoadAssetAtPath<TextAsset>(Constants.B3ScriptTemplate);
    }

    private static B3EditorSettings FindSettings()
    {
      var guids = AssetDatabase.FindAssets("t:B3EditorSettings");
      if (guids.Length > 1)
      {
        Debug.LogWarning($"Found multiple settings files, using the first.");
      }
  
      switch (guids.Length)
      {
        case 0:
          return null;
        default:
          var path = AssetDatabase.GUIDToAssetPath(guids[0]);
          return AssetDatabase.LoadAssetAtPath<B3EditorSettings>(path);
      }
    }
  
    private static B3EditorSettings CreateSettings()
    {
      var settings = CreateInstance<B3EditorSettings>();
      var path = Path.Combine("Assets", "B3EditorSettings.asset");
      AssetDatabase.CreateAsset(settings, path);
      AssetDatabase.SaveAssets();
      return settings;
    }
    
    internal static B3EditorSettings GetOrCreateSettings()
    {
      var settings = FindSettings();
      return settings == null ? CreateSettings() : settings;
    }
    
    internal static SerializedObject GetSerializedSettings() => new (GetOrCreateSettings());
  }
}