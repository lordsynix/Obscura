using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionOutline : MonoBehaviour
{
    private Transform highlight;
    private RaycastHit hit;

    [SerializeField] private Material highlightMaterial;

    private void Update()
    {
        if (highlight != null)
        {
            highlight.gameObject.GetComponent<Outline>().enabled = false;
            highlight = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit))
        {
            highlight = hit.transform.parent;
            if (highlight.CompareTag("Selectable"))
            {
                if (highlight.gameObject.GetComponent<Outline>() != null)
                {
                    highlight.gameObject.GetComponent<Outline>().enabled = true;
                }
                else
                {
                    Debug.Log("Added Outline");
                    Outline outline = highlight.gameObject.AddComponent<Outline>();
                    outline.highlightMaterial = highlightMaterial;
                    outline.enabled = true;
                }
            }
            else
            {
                highlight = null;
            }
        }
    }
}
