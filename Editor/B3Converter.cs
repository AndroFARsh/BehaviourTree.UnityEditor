using System;
using System.Collections.Generic;
using System.Linq;
using BTree.Runtime.Composites;
using BTree.Runtime.Decorators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BTree.Runtime
{
  public class B3Converter : JsonConverter<B3>
  {
    private const string NodesPropName = "nodes";
    private const string PosXPropName = "posX";
    private const string PosYPropName = "posY";
    private const string ConnectionsPropName = "connections";
    private const string GuidPropName = "guid";
    private const string NamePropName = "name";
    
    private const string ChildrenPropName = "children";
    private const string TypePropName = "$type";

    public override void WriteJson(JsonWriter writer, B3 value, JsonSerializer serializer)
    {
      var t = value != null
        ? WriteBehTree(value, serializer)
        : JToken.FromObject(value, serializer);
      
      t.WriteTo(writer);
    }

    public override B3 ReadJson(JsonReader reader, Type objectType, B3 existingValue, bool hasExistingValue, JsonSerializer serializer) 
    {
      switch (reader.TokenType)
      {
        case JsonToken.Null:
          return existingValue;
        default:
        {
          return ReadBehTree(reader, existingValue);
        }
      }
    }

    private static B3 ReadBehTree(JsonReader reader, B3 existingValue)
    {
      var tree = existingValue ?? new B3();
      var obj = JObject.Load(reader);

      ReadNodes(obj, tree);
      ReadNodeConnections(obj, tree);

      if (tree.root == null)
        tree.Register(new RootNode(), Vector2.zero);
      return tree;
    }

    private static void ReadNodeConnections(JObject obj, B3 tree)
    {
      if (obj[ConnectionsPropName] == null) return;
      
      foreach (var jConnection in obj[ConnectionsPropName])
      {
        var jConnectionObject = (JObject)jConnection;
        var jParentGuid = jConnectionObject[GuidPropName];
        if (jParentGuid == null) continue;
        
        var parentGuid = jParentGuid.ToString();
        var parent = tree.FindByGuid(parentGuid);
        if (parent == null) continue;
        
        var jChildren = jConnectionObject[ChildrenPropName];
        if (jChildren == null) continue;
        
        var jPosX = jConnectionObject[PosXPropName];
        if (jPosX == null) continue;
        var jPosY = jConnectionObject[PosYPropName];
        if (jPosY == null) continue;

        var pos = new Vector2(jPosX.Value<float>(), jPosY.Value<float>());
        tree.UpdatePosition(parent, pos);
        
        foreach (var jChild in jChildren)
        {
          var childGuid = jChild.ToString();
          var child = tree.FindByGuid(childGuid);
          parent.AddChild(child);
        }
      }
    }

    private static void ReadNodes(JObject obj, B3 tree)
    {
      if (obj[NodesPropName] == null) return;
      
      foreach (var jNode in obj[NodesPropName])
      {
        var jType = jNode[TypePropName];
        if (jType == null) continue;
        
        var type = Type.GetType(jType.ToString());
        if (type == null) continue;
        
        var node = jNode.ToObject(type) as Node;
        tree.Register(node, Vector2.zero);
      }
    }

    private static JObject WriteBehTree(B3 tree, JsonSerializer jsonSerializer) => new()
    {
      { NodesPropName, WriteNodes(tree, jsonSerializer) },
      { ConnectionsPropName, WriteNodeConnections(tree, jsonSerializer) },
    };
  
    private static JArray WriteNodeConnections(B3 tree, JsonSerializer jsonSerializer) => new (tree
      .Select(node => new JObject
      {
        { GuidPropName, node.guid },
        { NamePropName, node.name },
        { PosXPropName, JToken.FromObject(tree.GetPosition(node).x, jsonSerializer) },
        { PosYPropName,  JToken.FromObject(tree.GetPosition(node).y, jsonSerializer) },
        { ChildrenPropName, WriteChildrenGuid(node) }
      }));
    
    private static JArray WriteNodes(IEnumerable<Node> nodes, JsonSerializer jsonSerializer) => new(nodes.Select(node => 
      {
        var o = (JObject)JToken.FromObject(node, jsonSerializer);
        o.Remove("Children");
        o.Remove("children");
        o.Remove("Child");
        o.Remove("child");
        o.Remove("Name");
        o.Remove("name");
        return o;
      }));

    private static JArray WriteChildrenGuid(Node node) => new(
      node switch
      {
        RootNode rootNode => rootNode.Child != null
          ? new[] { rootNode.Child.guid }
          : Array.Empty<string>(),
        DecoratorNode decoratorNode => decoratorNode.Child != null
          ? new[] { decoratorNode.Child.guid }
          : Array.Empty<string>(),
        CompositeNode compositeNode => compositeNode.Children.Select(child => child.guid).ToArray(),
        _ => Array.Empty<string>()
      }
    );
  }
}