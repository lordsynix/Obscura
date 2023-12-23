using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SearchAlgorithm
{
    private static Vector3Int upperBorder = new Vector3Int(9, 0, 9);    
    private static Vector3Int lowerBorder = new Vector3Int(-10, 0, -10);

    private static List<Vector3Int> tileOffsets = new() { new(-1, 0, 0), new(1, 0, 0), new(0, 0, -1), new(0, 0, 1),
                                                          new(-1, 0, -1), new(-1, 0, 1), new(1, 0, -1), new(1, 0, 1)};

    private static int directPathWeight = 10;
    private static int diagonalPathWeight = 14;
    private static int wallWeigth = 180;
    private static int towerWeigth = 360;
    private static int gateWeight = 150;

    public static Vector3Int GetTarget(Vector3Int startPosition, GridData gridData)
    {
        // Storage for searched positions
        List<Vector3Int> searchList = new();
        Vector3Int target = new(int.MaxValue, 0, 0);
        bool foundTarget = false;

        searchList.Add(startPosition);

        // Target positions like an iglu
        var targets = gridData.GetTargetPositions();
        if (targets.Count == 0) return target;

        int index = 0;

        // Search
        while (!foundTarget)
        {
            if (index > 400) break;

            foreach (var offset in tileOffsets)
            {
                var pos = searchList[index] + offset;

                // Position is outside the borders
                if (pos.x > upperBorder.x || pos.z > upperBorder.z) continue;
                if (pos.x < lowerBorder.x || pos.z < lowerBorder.z) continue;

                // Position already in search list
                if (searchList.Contains(pos)) continue;

                // Found a possible target
                if (targets.Contains(pos))
                {
                    target = pos;
                    foundTarget = true;
                    break;
                }
                searchList.Add(pos);
            }
            index++;
        }
        return target;
    }
    
    public static List<Vector3Int> GetPath(Vector3Int startPosition, GridData gridData)
    {
        Vector3Int targetPosition = GetTarget(startPosition, gridData);

        if (targetPosition.x == int.MaxValue)
        {
            Debug.LogError($"NoTargetException: Make sure a unit has a target");
            return new();
        }

        List<GridNode> openNodes = new();
        List<GridNode> closedNodes = new();
        openNodes.Add(new(startPosition, new(int.MaxValue, 0, 0), 0, GetDistance(startPosition, targetPosition)));
        bool foundPath = false;

        while (!foundPath)
        {
            if (openNodes.Count == 0)
            {
                Debug.LogError("Open list can't be empty : " + closedNodes.Count);
                break;
            }

            // Get index of node in open with the lowest f_cost
            int index = 0;
            for(int i = 0; i < openNodes.Count; i++)
            {
                if (openNodes[i].F_Cost < openNodes[index].F_Cost)
                {
                    index = i;
                }
            }

            GridNode current = openNodes[index];
            openNodes.RemoveAt(index);
            closedNodes.Add(current);

            if (current.Position == targetPosition)
            {
                foundPath = true;
                break;
            }

            foreach (var offset in tileOffsets)
            {
                int pathWeight = tileOffsets.IndexOf(offset) > 3 ? diagonalPathWeight : directPathWeight;
                var neighbourPos = current.Position + offset;

                // Node position is outside the borders
                if (neighbourPos.x > upperBorder.x || neighbourPos.z > upperBorder.z) continue;
                if (neighbourPos.x < lowerBorder.x || neighbourPos.z < lowerBorder.z) continue;

                // Node position is already closed
                if (closedNodes.Any(n => n.Position == neighbourPos)) continue;

                // Position is not traversable
                int additionalWeight = gridData.TraversableWeight(neighbourPos, wallWeigth, towerWeigth, gateWeight);
                
                // Add node to open list
                if (!openNodes.Any(n => n.Position == neighbourPos))
                {
                    int f_cost = current.F_Cost + pathWeight + additionalWeight;
                    int h_cost = GetDistance(neighbourPos, targetPosition);
                    GridNode neighbour = new(neighbourPos, current.Position, f_cost, h_cost);
                    openNodes.Add(neighbour);
                }
                else
                {
                    // Get neighbour node in open list
                    int n_index = -1;
                    foreach (var n in openNodes)
                    {
                        if (n.Position == neighbourPos)
                        {
                            n_index = openNodes.IndexOf(n);
                        }
                    }
                    if (n_index != -1)
                    {
                        // Update f_cost of the neighbour
                        GridNode n = openNodes[n_index];
                        int f_cost = current.F_Cost + pathWeight + additionalWeight;
                        if (f_cost < n.F_Cost)
                        {
                            n.F_Cost = f_cost;
                            n.Parent = current.Position;
                        }
                    } 
                    else
                    {
                        Debug.LogWarning("Index must be non-negative and inside the count of the list");
                    }
                }
            }
        }

        // Get path of the algorithm
        List<Vector3Int> path = new();
        while (true)
        {
            if (path.Count > 400)
            {
                Debug.LogError("Cycles in path");
                break;
            }

            if (path.Count == 0)
            {
                path.Add(closedNodes[closedNodes.Count - 1].Position);
                path.Add(closedNodes[closedNodes.Count - 1].Parent);
            }
            else if (path[path.Count - 1] == startPosition) break;
            else
            {
                foreach (var n in closedNodes)
                {
                    if (n.Position == path[path.Count - 1])
                    {
                        path.Add(n.Parent);
                    }
                }
            }
        }
        path.Reverse();
        return path;
    }

    private static int GetDistance(Vector3Int pos, Vector3Int targetPos)
    {
        return (int)Vector3Int.Distance(pos, targetPos);
    }

    public struct GridNode
    {
        public Vector3Int Position;
        public int F_Cost;
        public int H_Cost;
        public Vector3Int Parent;

        public GridNode(Vector3Int pos, Vector3Int parent, int f_cost = 0, int h_cost = -1)
        {
            Position = pos;
            Parent = parent;
            F_Cost = f_cost;
            H_Cost = h_cost;
        }
    }
}
