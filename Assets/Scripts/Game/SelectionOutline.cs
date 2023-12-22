using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class SelectionOutline : MonoBehaviour
{
    private Transform highlight;
    private RaycastHit hit;
    private GameObject[] selection = new GameObject[2];
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
