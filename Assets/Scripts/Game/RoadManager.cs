using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    private RoadGraph roadGraph;
    public Road[] roads;
    public CrossRoad[] crossRoads;
    private Dictionary<Node, GameObject> roadObjects = new Dictionary<Node, GameObject>();
    [SerializeField] private Transform roadContainer;

    private void Start()
    {
        CreateRoadGraph();
    }

    public void CreateRoadGraph()
    {
        roadGraph = new RoadGraph();
        for (int i = 0; i < roads.Length; i++)
        {
            Road road = roads[i];
            int time;
            if (road.length <= 200)
            {
                time = 1;
            }
            else if (road.length > 200 &&  road.length <= 400)
            {
                time = 2;
            }
            else
            {
                time = 3;
            }
            Node node = new Node(time, i);
            roadGraph.AddNode(node);

            // Map nodes to road gameobjects
            GameObject roadObject = roadContainer.GetChild(i).gameObject;
            roadObjects.Add(node, roadObject);
            Debug.Log(node.index + " " + roadObject.gameObject.name);
        }

        CreateEdges();
    }

    private void CreateEdges()
    {
        foreach (CrossRoad crossRoad in crossRoads)
        {
            int srcIndex = crossRoad.involvedRoads[0];
            for (int i = 1; i <  crossRoad.involvedRoads.Count; ++i)
            {
                roadGraph.AddEdge(srcIndex, crossRoad.involvedRoads[i], roadGraph.GetNode(srcIndex).time);
                roadGraph.AddEdge(crossRoad.involvedRoads[i], srcIndex, roadGraph.GetNode(crossRoad.involvedRoads[i]).time);
            }
        }

        foreach (var node in roadGraph.GetGraph())
        {
            foreach (Tuple<Node, int> tuple in node.Value)
            {
                Debug.Log($"Node {node.Key.index} has an edge with node {tuple.Item1.index} with a weight of {node.Key.time}");
            }
        }
        roadGraph.ShortestPath(roadGraph.GetNode(0), roadGraph.GetNode(roadGraph.GetGraph().Count - 1));

    }
}
