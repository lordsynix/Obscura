using System;
using System.Collections;
using System.Collections.Generic;
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

    public int GetWeight(Node src, Node dst)
    {
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

    public Dictionary<Node, int> ShortestPath(Node src, Node dst)
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

        Dictionary<Node, int> shortestPath = new Dictionary<Node, int>();

        Queue<Tuple<Node, int>> minHeap = new Queue<Tuple<Node, int>>();
        minHeap.Enqueue(new Tuple<Node, int>(src, 0));

        while (minHeap.Count > 0)
        {
            Tuple<Node, int> node1 = minHeap.Dequeue();
            if (shortestPath.ContainsKey(node1.Item1))
            {
                continue;
            }
            // n1.item1 = Node, n2.item2 = weight
            shortestPath[node1.Item1] = node1.Item2;

            foreach (Tuple<Node, int> node in adjacencyList[node1.Item1])
            {
                if (!shortestPath.ContainsKey(node.Item1))
                {
                    minHeap.Enqueue(new Tuple<Node, int>(node.Item1, node1.Item2 + node.Item2));
                }
            }
        }

        foreach (var node in adjacencyList)
        {
            if (!shortestPath.ContainsKey(node.Key))
            {
                shortestPath[node.Key] = - 1;
            }
        }

        return shortestPath;
    }
}

public class Node 
{ 
    public int time;
    public int index;
    public ulong ownerIndex;

    public Node(int time, int index)
    {
        this.time = time;
        this.index = index;
        ownerIndex = ulong.MaxValue;
    }
}
