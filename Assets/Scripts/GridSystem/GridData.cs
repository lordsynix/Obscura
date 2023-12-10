using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    List<Vector3Int> souroundingTileOffsets = new() { new(-1, 0, 0), new(1, 0, 0), new(0, 0, -1), new(0, 0, 1) };

    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                throw new Exception($"Dictionary already contains this cell position {pos}");
            }
            placedObjects[pos] = data;
        }
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                return false;
            }
        }
        return true;
    }

    public int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
        {
            return -1;
        }
        if (placedObjects[gridPosition].ID == 0)
        {
            return placedObjects[gridPosition].PlacedObjectIndex;
        }
        return -1;
    }

    public void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach(var pos in placedObjects[gridPosition].OccupiedPositions)
        {
            placedObjects.Remove(pos);
        }
    }

    public List<Vector3Int> GetSouroundingWalls(Vector3Int gridPosition)
    {
        List<Vector3Int> returnVal = new();
        foreach (var offset in souroundingTileOffsets)
        {
            Vector3Int pos = gridPosition + offset;
            if (placedObjects.ContainsKey(pos) && placedObjects[pos].ID == 0)
            {
                returnVal.Add(offset);
            }
        }
        return returnVal;
    }
}

public class PlacementData
{
    public List<Vector3Int> OccupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        OccupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}
