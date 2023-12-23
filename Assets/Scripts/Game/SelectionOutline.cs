using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionOutline : MonoBehaviour
{
    private Transform highlight;
    private RaycastHit hit;
    private GameObject[] selection = new GameObject[2];
    private List<GameObject> pathSelection = new List<GameObject>();
    [SerializeField] private Material yellowHighlightMaterial;
    [SerializeField] private Material redHighlightMaterial;

    public bool interaction = false;
    public bool build = false;

    private void Update()
    {
        if (highlight != null)
        {
            if (!highlight.gameObject.GetComponent<Outline>().stay)
            {
                highlight.gameObject.GetComponent<Outline>().enabled = false;
            }
            highlight = null;
        }
        if (selection[0] != null)
        {
            selection[0].GetComponent<Outline>().enabled = true;
        }
        if (selection[1] != null)
        {
            selection[1].GetComponent<Outline>().enabled = true;
        }

        if (interaction == false) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit) && selection[1] == null)
        {
            highlight = hit.transform.parent;

            if (highlight.CompareTag("Road"))
            {
                HighlightRoad();
            }
            else
            {
                highlight = null;
            }
        }
        // Selection
        if (Input.GetMouseButtonDown(0))
        {            
            if (highlight)
            {
                if (highlight.CompareTag("Road"))
                {
                    SelectRoad();
                }
            }
            else
            {
                if (selection[0] == null) return;
                selection[0].GetComponent<Outline>().enabled = false;
                selection[0] = null;

                if (selection[1] == null) return;
                selection[1].GetComponent<Outline>().highlightMaterial = yellowHighlightMaterial;
                selection[1].GetComponent<Outline>().enabled = false;                
                selection[1] = null;
            }
        }
    }

    private void HighlightRoad()
    {
        if (highlight.gameObject == selection[0]) return;

        if (highlight.gameObject.GetComponent<Outline>() != null)
        {
            if (selection[0] != null)
            {
                highlight.gameObject.GetComponent<Outline>().enabled = true;
                highlight.gameObject.GetComponent<Outline>().highlightMaterial = redHighlightMaterial;
            }
            else
            {
                highlight.gameObject.GetComponent<Outline>().enabled = true;
                highlight.gameObject.GetComponent<Outline>().highlightMaterial = yellowHighlightMaterial;
            }
        }
        else
        {
            if (selection[0] != null)
            {
                Outline outline = highlight.gameObject.AddComponent<Outline>();
                outline.highlightMaterial = redHighlightMaterial;
                outline.enabled = true;
            }
            else
            {
                Outline outline = highlight.gameObject.AddComponent<Outline>();
                outline.highlightMaterial = yellowHighlightMaterial;
                outline.enabled = true;
            }

        }
    }

    private void SelectRoad()
    {
        if (build)
        {
            build = false;
            interaction = false;
            GameManager.Instance.BuildCastleServerRpc(int.Parse(highlight.gameObject.name), NetworkManager.Singleton.LocalClientId);
            return;
        }
        if (GameUI.Instance.pathSelect)
        {
            pathSelection.Add(highlight.gameObject);
            highlight.GetComponent<Outline>().highlightMaterial = yellowHighlightMaterial;
            highlight.GetComponent<Outline>().enabled = true;
            highlight.GetComponent<Outline>().stay = true;

            GameUI.Instance.ValidatePath(pathSelection);
        }
        else
        {
            if (selection[0] == null)
            {
                selection[0] = highlight.gameObject;
                selection[0].GetComponent<Outline>().enabled = true;
                selection[0].GetComponent<Outline>().highlightMaterial = yellowHighlightMaterial;
            }
            else
            {
                if (highlight.gameObject != selection[0] && selection[1] == null)
                {
                    selection[1] = highlight.gameObject;
                    selection[1].GetComponent<Outline>().enabled = true;
                    selection[1].GetComponent<Outline>().highlightMaterial = redHighlightMaterial;
                }
            }
        }
    }

    public void ClearSelection()
    {
        foreach (GameObject road in pathSelection)
        {            
            road.GetComponent<Outline>().highlightMaterial = yellowHighlightMaterial;
            road.GetComponent<Outline>().enabled = false;
            road.GetComponent<Outline>().stay = false;
        }
        pathSelection.Clear();
    }

    public void HighlightPath()
    {
        foreach (GameObject road in pathSelection)
        {
            if (road.GetComponent<Outline>())
            {
                road.GetComponent<Outline>().enabled = false;
                road.GetComponent<Outline>().highlightMaterial = redHighlightMaterial;
                road.GetComponent<Outline>().enabled = true;
                road.GetComponent<Outline>().stay = true;
            }
            else
            {
                Outline outline = road.AddComponent<Outline>();
                outline.enabled = false;
                outline.highlightMaterial = redHighlightMaterial;
                outline.enabled = true;
                outline.stay = true;
            }
        }
    }

    public void HighlightPath(List<Node> nodes)
    {
        ClearSelection();
        foreach (Node node in nodes)
        {
            pathSelection.Add(GetComponent<RoadManager>().roadObjects[node]);
            Debug.Log(GetComponent<RoadManager>().roadObjects[node].name);
        }
        HighlightPath();
    }
}
