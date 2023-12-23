using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackSystem : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputManager inputManager;

    private Action OnClicked;

    [Header("AttackSystem")]
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private Grid grid;
    [SerializeField] private UnitsDatabaseSO database;

    [SerializeField] private GameObject cellHighlightPrefab;
    [SerializeField] private Material cellHighlightMaterial;

    private int selectedUnit = -1;

    private void Start()
    {
        OnClicked += PlaceUnit;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnClicked?.Invoke();
    }
    private bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public void SelectUnit(int ID)
    {
        selectedUnit = ID;
    }

    public void PlaceUnit()
    {
        if (IsPointerOverUI() || selectedUnit == -1)
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int startPos = grid.WorldToCell(mousePosition);
        GameObject prefab = database.unitsData[selectedUnit].Prefab;

        var go = Instantiate(prefab, mousePosition, Quaternion.identity);

        var path = SearchAlgorithm.GetPath(startPos, placementSystem.GetGridData());

        if (path.Count > 0)
        {
            StartCoroutine(VisualizePath(path));
        }
    }

    IEnumerator VisualizePath(List<Vector3Int> path)
    {
        var pos = grid.CellToWorld(path[0]);

        var go = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
        go.GetComponentInChildren<MeshRenderer>().material = cellHighlightMaterial;
        go.SetActive(true);

        for (int i = 0; i < path.Count; i++)
        {
            go.transform.position = path[i];
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(go);
    }

}
