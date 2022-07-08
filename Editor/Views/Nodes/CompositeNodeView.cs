using System;
using System.Collections.Generic;
using BTree.Runtime.Composites;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Node = BTree.Runtime.Node;

namespace BTree.Editor.Views.Nodes
{
  public class CompositeNodeView : NodeView
  {
    private readonly CompositeNode node;

    public CompositeNodeView(CompositeNode n, Action preUpdate, Action update) : base(n, preUpdate, update)
    {
      node = n;
    }

    protected override string NodeStyleClass => "composite";

    protected override Port CreateInputPorts() => new NodePort(Direction.Input, Port.Capacity.Single)
    {
      portName = "",
      style = { flexDirection = FlexDirection.Column }
    };

    protected override Port CreateOutputPorts() => new NodePort(Direction.Output, Port.Capacity.Multi)
    {
      portName = "",
      style = { flexDirection = FlexDirection.ColumnReverse }
    };

    public override void SetupEdge()
    {
      foreach (var child in node.Children)
      {
        var childView = GraphView.GetNodeByGuid(child.guid) as NodeView;

        var edge = ConnectTo(childView);
        if (edge != null) GraphView.AddElement(edge);
      }
    }

    public override void SortChildren()
    {
      if (node is not CompositeNode compositeNode || compositeNode.Children.Count <= 1) return;

      var list = new List<Node>(compositeNode.Children);
      list.Sort(SortByHorizontalPosition);

      list.ForEach(n => compositeNode.RemoveChild(n));
      list.ForEach(n => compositeNode.AddChild(n));
    }

    private int SortByHorizontalPosition(Node left, Node right)
    {
      var leftView = (NodeView) GraphView.GetNodeByGuid(left.guid);
      var rightView = GraphView.GetNodeByGuid(right.guid);
      return leftView.style.left.value.value < rightView.style.left.value.value ? -1 : 1;
    } 
  }
}