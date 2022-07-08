using BTree.Editor.Views.Nodes;
using UnityEditor;
using UnityEngine.UIElements;

namespace BTree.Editor.Views
{
  public class DoubleClickSelection : MouseManipulator
  {
    private const double DoubleClickDuration = 0.3;
    
    private double time;

    public DoubleClickSelection()
    {
      time = EditorApplication.timeSinceStartup;
    }

    protected override void RegisterCallbacksOnTarget() => 
      target?.RegisterCallback<MouseDownEvent>(OnMouseDown);
    
    protected override void UnregisterCallbacksFromTarget() =>
      target?.UnregisterCallback<MouseDownEvent>(OnMouseDown);

    private void OnMouseDown(MouseDownEvent evt)
    {
      if (target is not B3GraphView)
        return;

      var duration = EditorApplication.timeSinceStartup - time;
      if (duration < DoubleClickDuration)
      {
        SelectChildren(evt);
      }

      time = EditorApplication.timeSinceStartup;
    }

    private void SelectChildren(MouseDownEvent evt)
    {
      if (target is not B3GraphView || !CanStopManipulation(evt))
        return;

      if (evt.target is not NodeView clickedElement)
      {
        var ve = evt.target as VisualElement;
        clickedElement = ve?.GetFirstAncestorOfType<NodeView>();
        if (clickedElement == null)
          return;
      }
      
      // Add children to selection so the root element can be moved
      clickedElement.DeepSelection();
    }
  }
}