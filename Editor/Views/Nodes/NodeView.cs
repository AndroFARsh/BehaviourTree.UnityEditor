using System;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = BTree.Runtime.Node;

namespace BTree.Editor.Views.Nodes
{
  public abstract class NodeView : UnityEditor.Experimental.GraphView.Node
  {
    private const string DescriptionProp = "description";
    private readonly Action preUpdate;
    private readonly Action update;
    private readonly Node node;
    private readonly Port input;
    private readonly Port output;
    private readonly Type type;
    private readonly FieldInfo[] fields;
    private readonly Label descriptionLabel;
    
    private GraphView graphView;

    protected abstract string NodeStyleClass { get; }
    protected GraphView GraphView => graphView ??= GetFirstAncestorOfType<GraphView>();

    public Node Node => node;

    protected NodeView(Node n, Action preUpdt, Action updt) : base(Constants.NodeView)
    {
      preUpdate = preUpdt;
      update = updt;
      node = n;

      type = n.GetType();
      fields = n.GetNodeFields(true);

      name = title = node.GetType().Name
        .Replace("(Clone)", "")
        .Replace("Node", "");
      viewDataKey = node.guid;

      // style.left = node.posX;
      // style.top = node.posY;
      AddToClassList(NodeStyleClass);

      input = CreateInputPorts();
      if (input != null) inputContainer.Add(input);

      output = CreateOutputPorts();
      if (output != null) outputContainer.Add(output);

      descriptionLabel = this.Q<Label>(DescriptionProp);
      UpdateDescription();
    }

    private void UpdateDescription() => descriptionLabel.text = node.description;

    protected Edge ConnectTo(NodeView child)
    {
      if (output != null && child is { input: { } })
      {
        return output.ConnectTo(child.input);
      }

      return null;
    }

    protected virtual Port CreateInputPorts() => null;
    protected virtual Port CreateOutputPorts() => null;
    
    // public override void SetPosition(Rect newPos)
    // {
    //   base.SetPosition(newPos);
    //   preUpdate?.Invoke();
    //   
    //   node.posX = newPos.xMin;
    //   node.posY = newPos.yMin;
    //
    //   update?.Invoke();
    // }

    public virtual void SortChildren()
    {
    }

    public virtual void SetupEdge() {}

    public void UpdateState()
    {
      RemoveFromClassList("running");
      RemoveFromClassList("failure");
      RemoveFromClassList("success");

      if (Application.isPlaying)
      {
        switch (node.state)
        {
          case Node.State.Running:
            AddToClassList("running");
            break;
          case Node.State.Failure:
            AddToClassList("failure");
            break;
          case Node.State.Success:
            AddToClassList("success");
            break;
        }
      }
    }

    public int PropertyCount => fields.Length;

    public FieldInfo GetProperty(int propertyIndex) => fields[propertyIndex];
    
    public string GetPropertyName(int propertyIndex) => fields[propertyIndex].Name;

    public Type GetPropertyType(int propertyIndex) => fields[propertyIndex].FieldType;

    public object GetPropertyValue(int propertyIndex) => fields[propertyIndex].GetValue(node);

    public void SetPropertyValue(int propertyIndex, object newValue)
    {
      var value = fields[propertyIndex].GetValue(node);
      if ((newValue == null && value == null) ||
          (newValue != null && newValue.Equals(value)) ||
          (value != null && value.Equals(newValue)))
        return;

      preUpdate?.Invoke();

      fields[propertyIndex].SetValue(node, newValue);
      UpdateDescription();

      update?.Invoke();
    }
    
    public void DeepSelection()
    {
      GraphView.ClearSelection();
      Node.Traverse(node =>
      {
        var nodeView = GraphView.GetNodeByGuid(node.guid);
        GraphView.AddToSelection(nodeView);
      });
      
    }
  }
}