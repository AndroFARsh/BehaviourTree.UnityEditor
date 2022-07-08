using System;
using BTree.Runtime.Actions;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BTree.Editor.Views.Nodes
{
  public class ActionNodeView : NodeView
  {
    public ActionNodeView(ActionNode node, Action preUpdate, Action update) : base(node, preUpdate, update)
    {
    }
    
    protected override string NodeStyleClass => "action";
    
    protected override Port CreateInputPorts() => new NodePort(Direction.Input, Port.Capacity.Single)
    {
      portName = "",
      style = { flexDirection = FlexDirection.Column }
    };
  }
}