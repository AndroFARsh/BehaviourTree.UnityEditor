using System;
using System.Collections.Generic;
using System.Linq;
using BTree.Editor.Views.Nodes;
using BTree.Runtime;
using BTree.Runtime.Actions;
using BTree.Runtime.Composites;
using BTree.Runtime.Decorators;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = BTree.Runtime.Node;

namespace BTree.Editor.Views
{
  public class B3GraphView : GraphView
  {
    public new class UxmlFactory : UxmlFactory<B3GraphView, UxmlTraits>
    {
    }

    private readonly List<NodeView> selectedList = new();
    private readonly List<Action<NodeView>> nodeSelectListeners = new();
    public event Action<NodeView> nodeSelectListener
    {
      add => nodeSelectListeners.Add(value);
      remove => nodeSelectListeners.Remove(value);
    }

    private TextAsset asset;
    private B3 tree;

    public B3GraphView()
    {
      Insert(0, new GridBackground());

      this.AddManipulator(new ContentZoomer());
      this.AddManipulator(new ContentDragger());
      this.AddManipulator(new DoubleClickSelection());
      this.AddManipulator(new SelectionDragger());
      this.AddManipulator(new RectangleSelector());

      var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Constants.B3EditorWindowStyle);
      styleSheets.Add(styleSheet);
      
      Undo.undoRedoPerformed += OnUndoRedo;
    }
    
    private void OnUndoRedo()
    {
      PopulateView(asset);
      AssetDatabase.SaveAssets();
    }

    internal void GenerateB3Script()
    {
      if (asset && tree != null)
      {
        B3CreateScript.CreateNewB3Script(asset, tree);
      }
    }

    internal void PopulateView(TextAsset newAsset)
    {
      asset = newAsset;
      tree = asset.ToB3();
      
      graphViewChanged -= OnGraphViewChanged;
      ClearSelection(); 
      DeleteElements(graphElements.ToList());
      
      if (tree != null)
      {
        // Create and register node view
        foreach (var node in tree)
          CreateAndRegisterNodeView(node, tree.GetPosition(node));

        // Create edges
        nodes.ForEach(n => (n as NodeView)?.SetupEdge());
      }
      
      graphViewChanged += OnGraphViewChanged;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) =>
      ports.ToList().Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
      graphViewChange.elementsToRemove?.ForEach(elem =>
      {
        switch (elem)
        {
          case RootNodeView rootNodeView:
            DeleteAsset();
            break;
          case NodeView nodeView:
            DeleteNode(nodeView);
            break;
          case Edge edge:
          {
            DeleteEdge(edge);
            break;
          }
        }
      });

      graphViewChange.edgesToCreate?.ForEach(CreateEdge);

      if ((graphViewChange.movedElements?.Count ?? 0) > 0)
      {
        Undo.RecordObject(asset, "Behaviour Tree (AddChild)");
        
        nodes.ForEach(n =>
        {
          if (n is not NodeView nodeView) return;
          
          var pos = new Vector2
          {
            x = nodeView.style.left.value.value,
            y = nodeView.style.top.value.value
          };
          nodeView.SortChildren();
          tree.UpdatePosition(nodeView.Node, pos);
        });
        
        asset = asset.UpdateB3Asset(tree);
      }

      return graphViewChange;
    }

    private void DeleteAsset()
    {
      var path = AssetDatabase.GetAssetPath(asset);
      if (!AssetDatabase.DeleteAsset(path)) return;
      
      ClearSelection();
      Selection.activeObject = null;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
      this.BuildNewScriptMenu(evt);
      
      if (tree != null)
        this.BuildAddNodeMenu(evt, CreateNode);
    }

    public override void AddToSelection(ISelectable selectable)
    {
      base.AddToSelection(selectable);
      if (selectable is not NodeView selectedNode) return;
      
      if (!selectedList.Contains(selectedNode))
        selectedList.Add(selectedNode);
      
      nodeSelectListeners.ForEach(action => action.Invoke(selectedList.FirstOrDefault()));
    }

    public override void RemoveFromSelection(ISelectable selectable)
    {
      base.RemoveFromSelection(selectable);
      if (selectable is not NodeView selectedNode) return;
      
      selectedList.Remove(selectedNode);
      nodeSelectListeners.ForEach(action => action.Invoke(selectedList.FirstOrDefault()));
    }

    public override void ClearSelection()
    {
      base.ClearSelection();
      
      selectedList.Clear();
      nodeSelectListeners.ForEach(action => action.Invoke(null));
    }

    public void UpdateNodeStates() => nodes.ForEach(n => (n as NodeView)?.UpdateState());

    private void CreateEdge(Edge edge)
    {
      if (edge.output.node is not NodeView parentView || edge.input.node is not NodeView childView) return;
      
      Undo.RecordObject(asset, "Behaviour Tree (AddChild)");
      parentView.Node.AddChild(childView.Node);
      asset = asset.UpdateB3Asset(tree);
    }
    
    private void DeleteEdge(Edge edge)
    {
      if (edge.output.node is not NodeView parentView || edge.input.node is not NodeView childView) return;
      
      Undo.RecordObject(asset, "Behaviour Tree (RemoveChild)");
      parentView.Node.RemoveChild(childView.Node);
      asset = asset.UpdateB3Asset(tree);
    }
    
    private void DeleteNode(NodeView nodeView)
    {
      Undo.RecordObject(asset, "Behaviour Tree (DeleteNode)");
      tree.Unregister(nodeView.Node);
      asset = asset.UpdateB3Asset(tree);
    }

    private void CreateNode(Type type, Vector2 position)
    {
      var node = Tools.CreateNode(type);
      Undo.RecordObject(asset, "Behaviour Tree (CreateNode)");
      tree.Register(node, position);
      CreateAndRegisterNodeView(node, position);
      asset = asset.UpdateB3Asset(tree);
    }

    private void CreateAndRegisterNodeView(Node node, Vector2 pos)
    {
      var nodeView = CreateNodeView(node);
      nodeView.style.left = pos.x;
      nodeView.style.top = pos.y;
      AddElement(nodeView);
    }

    private NodeView CreateNodeView(Node node)
    {
      return node switch
      {
        ActionNode actionNode => new ActionNodeView(actionNode, Before, After),
        RootNode rootNode => new RootNodeView(rootNode, Before, After),
        DecoratorNode decoratorNode => new DecoratorNodeView(decoratorNode, Before, After),
        CompositeNode compositeNode => new CompositeNodeView(compositeNode, Before, After),
        _ => throw new Exception($"Unknown Node type {node.GetType()}={node}")
      };
      
      void Before() => Undo.RecordObject(asset, "Behaviour Tree (UpdateNode)");
      void After() => asset = asset.UpdateB3Asset(tree);
    }
  }
}