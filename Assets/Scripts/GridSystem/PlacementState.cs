using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          ObjectsDatabaseSO database,
                          GridData floorData,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.gridData = floorData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

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

        Vector3 position = grid.CellToWorld(gridPosition);
        Vector3 rotation = Vector3.zero;

        if (selectedObjectIndex == 0)
        {
            // Get sourounding walls
            var souroundingWalls = gridData.GetSouroundingWalls(gridPosition);
            int count = souroundingWalls.Count;
            Debug.Log($"{count} sourouding walls detected!");

            if (count == 1 && souroundingWalls[0].z != gridPosition.z)
            {
                // Vertical Wall
                position.z += 1;
                rotation.y = 90;

                // Update other wall
                int representationIndex = gridData.GetRepresentationIndex(souroundingWalls[0]);
                objectPlacer.UpdateWallRotation(representationIndex, new(0, 90, 0));
            }
        }
        int index = objectPlacer.PlaceObject(prefab, position, rotation);

        gridData.AddObjectAt(gridPosition,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            index);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
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
