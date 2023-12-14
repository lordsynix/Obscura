using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class RoadManager : MonoBehaviour
{
    [SerializeField] private float size = 5;
    private Dictionary<int, BoxCollider> splineColliderDict = new Dictionary<int, BoxCollider>();

    public void CreateCollider(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int splineIndex)
    {
        if (!splineColliderDict.ContainsKey(splineIndex))
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = p3 - p1 / 2;
            collider.size = new Vector3(size, size, size);
            splineColliderDict.Add(splineIndex, collider);
        }
        else
        {
            if (splineColliderDict[splineIndex] == null)
            {
                splineColliderDict.Remove(splineIndex);
                BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
                newCollider.center = p3 - p1 / 2;
                newCollider.size = new Vector3(size, size, size);
                splineColliderDict.Add(splineIndex, newCollider);
            }
            BoxCollider collider = splineColliderDict[splineIndex];
            collider.center = p3 - p1 / 2;
            collider.size = new Vector3(size, size, size);
        }                               
    }
}
