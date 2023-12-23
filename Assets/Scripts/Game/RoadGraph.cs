using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadGraph
{
    private Dictionary<Node, List<Tuple<Node, int>>> adjacencyList;

    public RoadGraph()
    {
        adjacencyList = new Dictionary<Node, List<Tuple<Node, int>>>();
    }

    public void AddNode(Node node)
    {
        adjacencyList.Add(node, new List<Tuple<Node, int>>());
    }

    public void RemoveNode(Node node)
    {
        adjacencyList.Remove(node);
    }

    public Dictionary<Node, List<Tuple<Node, int>>> GetGraph()
    {
        return adjacencyList;
    }

    public Node GetNode(int index)
    {
        foreach (Node node in adjacencyList.Keys)
        {
            if (node.index == index)
            {
                return node;
            }
        }
        Debug.Log($"Didn't find node of index {index}!");
        return null;
    }

    public void AddEdge(Node src, Node dst, int weight)
    {
        if (adjacencyList[src] == null)
        {
            Debug.Log("Specified source is invalid!");
            return;
        }
        if (adjacencyList[dst] == null)
        {
            Debug.Log("Specified destination is invalid!");
            return;
        }

        adjacencyList[src].Add(new Tuple<Node, int>(dst, weight));
    }

    public void AddEdge(int srcIndex, int dstIndex, int weight)
    {
        Node src = GetNode(srcIndex);
        Node dst = GetNode(dstIndex);

        if (src == null)
        {
            Debug.Log("Specified source index is invalid!");
            return;
        }
        if (dst == null)
        {
            Debug.Log("Specified destination index is invalid!");
            return;
        }

        foreach (Tuple<Node, int> edge in adjacencyList[src])
        {
            if (edge.Item1 == dst)
            {
                Debug.Log("There is already an edge between the specified nodes!");
            }
        }

        adjacencyList[src].Add(new Tuple<Node, int>(dst, weight));
    }

    public int GetWeight(Node src, Node dst)
    {
        if (src == null)
        {
            Debug.Log("Specified source is invalid!");
            return int.MaxValue;
        }
        if (adjacencyList[src] == null)
        {
            Debug.Log("Specified source is invalid!");
            return int.MaxValue;
        }
        foreach (Tuple<Node, int> edge in adjacencyList[src])
        {
            if (edge.Item1 == dst)
            {
                return edge.Item2;
            }
        }
        Debug.Log("Specified destination is invalid!");
        return int.MaxValue;
    }

    public bool HasEdge(Node src, Node dst)
    {
        if (src == null)
        {
            Debug.Log("Specified source index is invalid!");
            return false;
        }

        if (dst == null)
        {
            Debug.Log("Specified destination index is invalid!");
            return false;
        }

        foreach (Tuple<Node, int> edge in adjacencyList[src])
        {
            if (edge.Item1 == dst)
            {
                return true;
            }
        }
        return false;
    }


    public void SetWeight(Node src, Node dst, int weight)
    {
        if (src == null)
        {
            Debug.Log("Specified source index is invalid!");
            return;
        }

        if (dst == null)
        {
            Debug.Log("Specified destination index is invalid!");
            return;
        }

        foreach (Tuple<Node, int> edge in adjacencyList[src])
        {
            if (edge.Item1 == dst)
            {
                adjacencyList[src].Remove(edge);
                AddEdge(src, dst, weight);
                break;
            }
        }
        Debug.Log("There is no edge between the specified nodes!");
        return;
    }

    public List<Node> ShortestPath(Node src, Node dst)
    {
        // Implementation of Dijkstra algorithm
        if (src == null)
        {
            Debug.Log("Specified source index is invalid!");
            return null;
        }
        if (dst == null)
        {
            Debug.Log("Specified destination index is invalid!");
            return null;
        }

        Dictionary<Node, int> dist = new Dictionary<Node, int>();
        Dictionary<Node, Node> pred = new Dictionary<Node, Node>();
        List<Node> notVisited = new List<Node>();

        // Initialize dictionaries
        foreach (var node in adjacencyList)
        {
            if (node.Key == src) 
            {
                dist.Add(node.Key, 0);
            }
            else
            {
                dist.Add(node.Key, int.MaxValue);
            }
            pred.Add(node.Key, null);
            notVisited.Add(node.Key);
        }

        Node currentNode = src;
        while (currentNode != null)
        {
            // Remove visited nodes from list
            notVisited.Remove(currentNode);

            Node lowest = null;
            // Discover all connections
            foreach (var node in adjacencyList[currentNode])
            {
                // Change their distance if smaller than previously
                if (node.Item2 + dist[currentNode] < dist[node.Item1])
                {
                    dist[node.Item1] = node.Item2 + dist[currentNode];

                    // Save their predecessor
                    pred[node.Item1] = currentNode;

                    Debug.Log(node.Item1.index + " : " + dist[node.Item1]);
                }

                // Find not yet visited node with lowest distance
                if (notVisited.Contains(node.Item1))
                {
                    if (lowest == null)
                    {
                        lowest = node.Item1;
                    }
                    if (dist[node.Item1] < dist[lowest])
                    {
                        lowest = node.Item1;
                    }
                }
            }

            currentNode = lowest;
        }

        Stack<Node> path = new Stack<Node> ();

        // Get shortest path to dst node
        Node predNode = dst;
        while (predNode != src && predNode != null)
        {
            path.Push(predNode);
            predNode = pred[predNode];
        }
        path.Push(src);

        return path.ToList();
    }

    public bool HasPath(List<Node> nodes, Node origin, Node target)
    {
        // Sort the path so that origin and target are at start and end
        nodes = SortNodes(nodes, origin, target);

        if (nodes == null) return false;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (i < nodes.Count - 1)
            {
                if (HasEdge(nodes[i], nodes[i + 1]))
                {
                    Debug.Log($"Edge exists from {nodes[i].index} to {nodes[i + 1].index}.");
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    public List<Node> SortNodes(List<Node> nodes, Node orgin, Node target)
    {
        // Bring origin and target to first and last
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == orgin)
            {
                Node temp = nodes[0];
                nodes[0] = nodes[i];
                nodes[i] = temp;
            }
            if (nodes[i] == target)
            {
                Node temp = nodes[nodes.Count - 1];
                nodes[nodes.Count - 1] = nodes[i];
                nodes[i] = temp;
            }
        }

        // Check if node i has and edge with node j
        for (int i = 0; i < nodes.Count - 2; i++)
        {
            int edgeCount = 0;
            for (int j = i + 1; j < nodes.Count - 1; j++)
            {
                if (HasEdge(nodes[i], nodes[j]))
                {
                    edgeCount++;

                    if (edgeCount > 1)
                    {
                        return null;
                    }

                    Node temp = nodes[i + 1];
                    nodes[i + 1] = nodes[j];
                    nodes[j] = temp;
                }
            }
            if (edgeCount == 0)
            {
                return null;
            }
        }
        return nodes;
    }

    public int GetPathTime(List<Node> nodes)
    {
        int time = 0;
        foreach (Node node in nodes)
        {
            time += node.time;
        }
        return time;
    }
}



public class Node 
{ 
    public int time;
    public int index;

    public Outpost outpost;

    public Node(int time, int index)
    {
        this.time = time;
        this.index = index;        
        outpost = null;
    }
}
