using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    private RoadGraph roadGraph;
    public Road[] roads;
    public CrossRoad[] crossRoads;
    public Dictionary<Node, GameObject> roadObjects = new Dictionary<Node, GameObject>();
    [SerializeField] private Transform roadContainer;

    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private Transform uiContainer;

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
        }

        CreateEdges();
    }

    private void CreateEdges()
    {
        foreach (CrossRoad crossRoad in crossRoads)
        {
            List<int> involvedRoads = crossRoad.involvedRoads;
            for (int i = 0; i < involvedRoads.Count - 1; i++)
            {
                for (int j = i + 1; j < involvedRoads.Count; j++)
                {
                    roadGraph.AddEdge(involvedRoads[i], crossRoad.involvedRoads[j], roadGraph.GetNode(involvedRoads[i]).time);
                    roadGraph.AddEdge(crossRoad.involvedRoads[j], involvedRoads[i], roadGraph.GetNode(crossRoad.involvedRoads[j]).time);
                }
            }           
        }
    }

    public bool RoadOccupied(int roadIndex)
    {
        if (roadGraph.GetNode(roadIndex).outpost == null)
        {
            return false;
        }
        return true;
    }

    public void BuildCastle(int roadIndex, ulong clientId)
    {
        Node node = roadGraph.GetNode(roadIndex);

        // Position index 2 for center
        // Players start with 10 infantry and 5 artillery
        Outpost outpost = new Outpost(roadIndex, 2, clientId, true, 10, 5, 0, 0);
        node.outpost = outpost;
        node.center = clientId;
        GetComponent<GameUI>().BuildCastleUI(node, clientId);
    }
    
    public Vector3 GetMiddleVertexPosition(Node node)
    {
        Mesh mesh = roadObjects[node].GetComponent<MeshFilter>().mesh;
        Vector3 worldPosition = mesh.vertices[(mesh.vertices.Length - 1) / 2];
        Vector3 uiPosition = Camera.main.WorldToScreenPoint(worldPosition);
        
        return uiPosition;
    }

    public Node GetNode(int roadIndex)
    {
        return roadGraph.GetNode(roadIndex);
    }

    public List<Node> GetShortestPath(Node src, Node dst)
    {
         return roadGraph.ShortestPath(src, dst);
    }
}
