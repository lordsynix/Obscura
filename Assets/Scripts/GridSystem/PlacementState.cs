using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    ObjectsDatabaseSO database;
    GridData gridData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
    PlacementSystem placementSystem;

    List<Vector3Int> tileOffsets = new() { new(-1, 0, 0), new(1, 0, 0), new(0, 0, -1), new(0, 0, 1) };

    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          ObjectsDatabaseSO database,
                          GridData floorData,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback,
                          PlacementSystem placementSystem)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.gridData = floorData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;
        this.placementSystem = placementSystem;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);

        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
        }
        else
        {
            throw new System.Exception($"No object with ID {iD}");
        }
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (!placementValidity)
        {
            soundFeedback.PlaySound(SoundType.WrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        GameObject prefab = database.objectsData[selectedObjectIndex].Prefab;

        Vector3 position = Vector3.zero;
        Vector3 rotation = new(-90, 0, 0);

        var surWalls = gridData.GetSurroundingWalls(gridPosition);
        int count = surWalls.Count;

        if (selectedObjectIndex == 0)
        {
            // Get sourounding walls
            if (count == 0)
            {
                // Horizontal allign
                rotation.y = 90;
            }
            if (count == 1)
            {
                // Horizontal allign
                if (surWalls[0].x != 0)
                {
                    rotation.y = 90;
                }
            }
            else if (count == 2)
            {
                if (surWalls[1].z != 0)
                {
                    if (surWalls[0].z != 0)         // vertical
                    {
                        rotation.y = 0;
                    }
                    else if (surWalls[0].x == -1)
                    {
                        if (surWalls[1].z == 1)     // up - left
                        {
                            prefab = database.objectsData[1].Prefab;
                            position = new(0.3333333f, 1, 0.6666667f);
                            rotation.y = -90;
                        }
                        else                         // down - left
                        {
                            prefab = database.objectsData[1].Prefab;
                            position = new(0.3333333f, 1, 0.3333333f);
                            rotation.y = 180;
                        }
                    }
                    else if (surWalls[0].x == 1)
                    {
                        if (surWalls[1].z == 1)     // up - right
                        {
                            prefab = database.objectsData[1].Prefab;
                            position = new(0.6666667f, 1, 0.6666667f);
                            rotation.y = 0;
                        }
                        else                         // down - right
                        {
                            prefab = database.objectsData[1].Prefab;
                            position = new(0.6666667f, 1, 0.3333333f);
                            rotation.y = 90;
                        }
                    }
                }
                else
                {
                    // horizontal
                    rotation.y = 90;
                }
            }
            else if (count == 3)
            {
                if (!surWalls.Contains(tileOffsets[0]))
                {
                    // right allign
                    prefab = database.objectsData[2].Prefab;
                    position = new(0.5833333f, 1, 0.5f);
                }
                else if (!surWalls.Contains(tileOffsets[1]))
                {
                    // left allign
                    prefab = database.objectsData[2].Prefab;
                    position = new(0.4166667f, 1, 0.5f);
                    rotation.y = 180;
                }
                else if (!surWalls.Contains(tileOffsets[2]))
                {
                    // top allign
                    prefab = database.objectsData[2].Prefab;
                    position = new(0.5f, 1, 0.5833333f);
                    rotation.y = -90;
                }
                else if (!surWalls.Contains(tileOffsets[3]))
                {
                    // bottom allign
                    prefab = database.objectsData[2].Prefab;
                    position = new(0.5f, 1, 0.4166667f);
                    rotation.y = 90;
                }
            }
            else if (count == 4)
            {
                prefab = database.objectsData[3].Prefab;
            }
        }
        else if (selectedObjectIndex == 4)  
        {
            if (count == 0)
            {
                rotation.y = -90;
            }
            else if (count == 1)
            {
                if (surWalls[0] == tileOffsets[0])
                {
                    rotation.y = -90;
                }
                else if (surWalls[0] == tileOffsets[0])
                {
                    rotation.y = -90;
                }
            }
        }

        int index = objectPlacer.PlaceObject(prefab, grid.CellToWorld(gridPosition), position, rotation);

        gridData.AddObjectAt(gridPosition,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            index);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);

        foreach (var wall in surWalls)
        {
            placementSystem.UpdateWall(wall + gridPosition);
        }
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        return gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);

    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
    }
}
