using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Grid grid;
    [SerializeField] private ObjectsDatabaseSO database;
    [SerializeField] private GameObject gridVisualization;

    private GridData gridData;

    [SerializeField]
    private PreviewSystem preview;

    [SerializeField] private ObjectPlacer objectPlacer;

    private Vector3Int lastDetectedPosition = Vector3Int.zero;

    IBuildingState buildingState;

    List<Vector3Int> tileOffsets = new() { new(-1, 0, 0), new(1, 0, 0), new(0, 0, -1), new(0, 0, 1) };

    [SerializeField]
    private SoundFeedback soundFeedback;

    private void Start()
    {
        StopPlacement();
        gridData = new();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        soundFeedback.PlaySound(SoundType.Click);
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, gridData, objectPlacer, soundFeedback, this);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving() 
    {
        StopPlacement();
        soundFeedback.PlaySound(SoundType.Click);
        gridVisualization.SetActive(true);
        buildingState = new RemovingState(grid, preview, gridData, objectPlacer, soundFeedback, this);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        buildingState.OnAction(gridPosition);
    }

    private void StopPlacement()
    {
        if (buildingState == null)
            return;
        gridVisualization.SetActive(false);
        buildingState.EndState();
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    public void GetTarget()
    {
        Debug.Log("Target: " + SearchAlgorithm.GetTarget(new(0, 0, 0), gridData));
    }

    private void Update()
    {
        if (buildingState == null)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (lastDetectedPosition != gridPosition)
        {
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
        }
    }

    public void UpdateWall(Vector3Int gridPosition)
    {
        GameObject prefab = database.objectsData[0].Prefab;

        Vector3 position = Vector3.zero;
        Vector3 rotation = new(-90, 0, 0);

        // Get sourounding walls
        var surWalls = gridData.GetSurroundingWalls(gridPosition);
        int count = surWalls.Count;
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
        objectPlacer.UpdateWall(gridData.GetRepresentationIndex(gridPosition), prefab, gridPosition, position, rotation);
    }
}
