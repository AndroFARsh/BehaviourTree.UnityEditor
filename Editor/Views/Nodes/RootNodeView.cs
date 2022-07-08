using System;
using BTree.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BTree.Editor.Views.Nodes
{
  public class RootNodeView : NodeView
  {
    private readonly RootNode node;

    public RootNodeView(RootNode n, Action preUpdate, Action update) : base(n, preUpdate, update)
    {
      node = n;
    }

    protected override string NodeStyleClass => "root";

    protected override Port CreateOutputPorts() => new NodePort(Direction.Output, Port.Capacity.Single)
    {
      portName = "",
      style = { flexDirection = FlexDirection.ColumnReverse }
    };

    public override void SetupEdge()
    {
      var child = node.Child;
      if (child != null)
      {
        var childView = GraphView.GetNodeByGuid(child.guid) as NodeView;

        var edge = ConnectTo(childView);
        if (edge != null) GraphView.AddElement(edge);
      }
    }
  }
}