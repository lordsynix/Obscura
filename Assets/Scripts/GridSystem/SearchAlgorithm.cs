using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SearchAlgorithm
{
    private static Vector3Int upperBorder = new Vector3Int(4, 0, 4);    
    private static Vector3Int lowerBorder = new Vector3Int(-5, 0, -5);

    private static List<Vector3Int> tileOffsets = new() { new(-1, 0, 0), new(1, 0, 0), new(0, 0, -1), new(0, 0, 1) };

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
            if (index > 100) break;

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
}
