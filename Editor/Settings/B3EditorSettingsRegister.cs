using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BTree.Editor.Settings
{
  static class B3EditorSettingsRegister
  {
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
      // First parameter is the path in the Settings window.
      // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
      var provider = new SettingsProvider("Project/B3EditorSettingsRegister", SettingsScope.Project)
      {
        label = "BTree Settings",
        // activateHandler is called when the user clicks on the Settings item in the Settings window.
        activateHandler = (searchContext, rootElement) =>
        {
          var settings = B3EditorSettings.GetSerializedSettings();

          // rootElement is a VisualElement. If you add any children to it, the OnGUI function
          // isn't called because the SettingsProvider uses the UIElements drawing framework.
          var title = new Label()
          {
            text = "BehTree Settings"
          };
          title.AddToClassList("title");
          rootElement.Add(title);

          var properties = new VisualElement()
          {
            style =
            {
              flexDirection = FlexDirection.Column
            }
          };
          properties.AddToClassList("property-list");
          rootElement.Add(properties);

          properties.Add(new InspectorElement(settings));

          rootElement.Bind(settings);
        },
      };

      return provider;
    }
  }
}