using System;
using System.Text.RegularExpressions;
using BTree.Editor.Views.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTree.Editor.Views.Inspector
{
  public class InspectorView : VisualElement
  {
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
    {
    }

    private NodeView nodeView;

    internal void UpdateSelection(NodeView newNodeView)
    {
      if (nodeView == newNodeView) return;
      nodeView = newNodeView;

      Clear();

      if (nodeView == null) return;

      Add(new Label($"{nodeView.name} ({nodeView.Node.guid})")
      {
        style =
        {
          paddingLeft = 3,
          paddingRight = 3,
          paddingTop = 5,
          paddingBottom = 20,
        }
      });

      for (var i = 0; i < nodeView.PropertyCount; i++)
      {
        var fieldView = InitField(i, nodeView);
        Add(fieldView);
      }
    }

    private static VisualElement InitField(int index, NodeView nodeView)
    {
      var name = PropNameToTitle(nodeView.GetPropertyName(index));
      var type = nodeView.GetPropertyType(index);
      var value = nodeView.GetPropertyValue(index);
      switch (type)
      {
        case { } when type == typeof(LayerMask):
        {
          var field = new LayerMaskField()
          {
            label = name,
            value = (LayerMask)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, (LayerMask)field.value));
          return field;
        }
        case { } when type == typeof(double):
        {
          var field = new DoubleField()
          {
            label = name,
            value = (double)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(float):
        {
          var field = new FloatField
          {
            label = name,
            value = (float)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(int):
        {
          var field = new IntegerField
          {
            label = name,
            value = (int)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(string):
        {
          var field = new TextField
          {
            label = name,
            value = (string)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Vector2):
        {
          var field = new Vector2Field
          {
            label = name,
            value = (Vector2)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Vector3):
        {
          var field = new Vector3Field
          {
            label = name,
            value = (Vector3)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Vector4):
        {
          var field = new Vector4Field
          {
            label = name,
            value = (Vector4)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Vector2Int):
        {
          var field = new Vector2IntField
          {
            label = name,
            value = (Vector2Int)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Vector3Int):
        {
          var field = new Vector3IntField
          {
            label = name,
            value = (Vector3Int)value
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Bounds):
        {
          var field = new BoundsField()
          {
            label = name,
            value = (Bounds)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(BoundsInt):
        {
          var field = new BoundsIntField()
          {
            label = name,
            value = (BoundsInt)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Enum) || type.IsSubclassOf(typeof(Enum)):
        {
          var field = new EnumField(name, (Enum)value);
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object)):
        {
          var field = new ObjectField()
          {
            label = name,
            value = (UnityEngine.Object)value,
            objectType = type
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Gradient):
        {
          var field = new GradientField()
          {
            label = name,
            value = (Gradient)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(long):
        {
          var field = new LongField()
          {
            label = name,
            value = (long)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(Color):
        {
          var field = new ColorField()
          {
            label = name,
            value = (Color)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        case { } when type == typeof(AnimationCurve):
        {
          var field = new CurveField()
          {
            label = name,
            value = (AnimationCurve)value,
          };
          field.binding = new Binding(() => nodeView.SetPropertyValue(index, field.value));
          return field;
        }
        default:
          return null;
      }
    }

    private static string PropNameToTitle(string input)
    {
      if (input == null) throw new ArgumentNullException(nameof(input));
      if (input == "") throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));

      if (input.StartsWith("m_")) input = input[2..];

      input = input.Replace("_", " ").Trim();
      input = Regex.Replace(input, "([A-Z])", " $1").Trim();
      return input[0].ToString().ToUpper() + input[1..];
    }
  }

  internal class Binding : IBinding
  {
    private readonly Action action;

    public Binding(Action action)
    {
      this.action = action;
    }

    public void PreUpdate()
    {
    }

    public void Update() => action?.Invoke();

    public void Release()
    {
    }
  }
}