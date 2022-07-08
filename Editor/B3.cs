using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTree.Runtime
{
  [Serializable]
  public class B3 : IB3, IEnumerable<Node>
  {
    private string rootGuid;
    private Dictionary<string, Node> nodes = new ();
    private Dictionary<string, Vector2> positions = new ();
    
    public Node root => FindByGuid(rootGuid);
    
    public void Register(Node node, Vector2 position)
    {
      if (node is RootNode)
      {
        if (rootGuid != null) nodes.Remove(rootGuid);
        rootGuid = node.guid;
      }

      nodes.Add(node.guid, node);
      UpdatePosition(node, position);
    }

    public void Unregister(Node node)
    {
      if (!nodes.Remove(node.guid)) return;

      positions.Remove(node.guid);
      foreach (var n in nodes.Values)
        n.RemoveChild(node);
    }


    public Vector2 GetPosition(Node node) => positions.ContainsKey(node.guid) ? positions[node.guid] : Vector2.zero;

    public void UpdatePosition(Node node, Vector2 position)
    {
      if (positions.ContainsKey(node.guid)) 
        positions[node.guid] = position;
      else
        positions.Add(node.guid, position);
    } 
    
    public Node FindByGuid(string guid) => nodes.ContainsKey(guid) ? nodes[guid] : null;
    public IEnumerator<Node> GetEnumerator() => nodes.Values.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  
    public Node.State Evaluate<T>(T context) where T : IContext => root?.Evaluate(context) ?? Node.State.Failure;
  }
}