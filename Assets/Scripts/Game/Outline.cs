using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour
{
    private Material previousMaterial;
    public Material highlightMaterial;

    private void OnEnable()
    {
        previousMaterial = GetComponent<MeshRenderer>().material;
        GetComponent<MeshRenderer>().material = highlightMaterial;
    }

    private void OnDisable()
    {
        GetComponent<MeshRenderer>().material = previousMaterial;
    }
}
