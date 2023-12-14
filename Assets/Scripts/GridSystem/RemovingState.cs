using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    GridData gridData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
    PlacementSystem placementSystem;

    public RemovingState(Grid grid,
                         PreviewSystem previewSystem,
                         GridData floorData,
                         ObjectPlacer objectPlacer,
                         SoundFeedback soundFeedback,
                         PlacementSystem placementSystem)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.gridData = floorData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;
        this.placementSystem = placementSystem;

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        GridData selectedData = null;
        if (gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one) == false)
        {
            selectedData = gridData;
        }

        if (selectedData == null) 
        {
            soundFeedback.PlaySound(SoundType.WrongPlacement);
        }
        else
        {
            soundFeedback.PlaySound(SoundType.Remove);
            gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
            Debug.Log("Position: " + gridPosition + " Index: " + gameObjectIndex);
            if (gameObjectIndex == -1)
                return;
            selectedData.RemoveObjectAt(gridPosition);
            objectPlacer.RemoveObjectAt(gameObjectIndex);

            var surWalls = gridData.GetSurroundingWalls(gridPosition);

            foreach (var wall in surWalls)
            {
                placementSystem.UpdateWall(wall + gridPosition);
            }

        }
        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !gridData.CanPlaceObjectAt(gridPosition, Vector2Int.one);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }
}
