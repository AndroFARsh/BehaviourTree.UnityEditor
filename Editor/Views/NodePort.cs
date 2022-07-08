using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTree.Editor.Views
{
  public class NodePort : Port
  {
    // GITHUB:UnityCsReference-master\UnityCsReference-master\Modules\GraphViewEditor\Elements\Port.cs
    private class DefaultEdgeConnectorListener : IEdgeConnectorListener
    {
      private readonly GraphViewChange graphViewChange;
      private readonly List<Edge> edgesToCreate;
      private readonly List<GraphElement> edgesToDelete;

      public DefaultEdgeConnectorListener()
      {
        edgesToCreate = new List<Edge>();
        edgesToDelete = new List<GraphElement>();

        graphViewChange.edgesToCreate = edgesToCreate;
      }

      public void OnDropOutsidePort(Edge edge, Vector2 position)
      {
      }

      public void OnDrop(GraphView graphView, Edge edge)
      {
        edgesToCreate.Clear();
        edgesToCreate.Add(edge);

        // We can't just add these edges to delete to the m_GraphViewChange
        // because we want the proper deletion code in GraphView to also
        // be called. Of course, that code (in DeleteElements) also
        // sends a GraphViewChange.
        edgesToDelete.Clear();
        if (edge.input.capacity == Capacity.Single)
          foreach (var edgeToDelete in edge.input.connections)
            if (edgeToDelete != edge)
              edgesToDelete.Add(edgeToDelete);
        if (edge.output.capacity == Capacity.Single)
          foreach (var edgeToDelete in edge.output.connections)
            if (edgeToDelete != edge)
              edgesToDelete.Add(edgeToDelete);
        if (edgesToDelete.Count > 0)
          graphView.DeleteElements(edgesToDelete);

        var newEdgesToCreate = edgesToCreate;
        if (graphView.graphViewChanged != null)
        {
          newEdgesToCreate = graphView.graphViewChanged(graphViewChange).edgesToCreate;
        }

        foreach (var e in newEdgesToCreate)
        {
          graphView.AddElement(e);
          edge.input.Connect(e);
          edge.output.Connect(e);
        }
      }
    }

    public NodePort(Direction direction, Capacity capacity)
      : base(Orientation.Vertical, direction, capacity, typeof(bool))
    {
      var connectorListener = new DefaultEdgeConnectorListener();
      m_EdgeConnector = new EdgeConnector<Edge>(connectorListener);
      this.AddManipulator(m_EdgeConnector);
      style.width = 100;
    }

    public override bool ContainsPoint(Vector2 localPoint)
    {
      var rect = new Rect(0, 0, layout.width, layout.height);
      return rect.Contains(localPoint);
    }
  }
}