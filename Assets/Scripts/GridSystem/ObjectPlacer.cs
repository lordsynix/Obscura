using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> placedGameObjects = new();

    public int PlaceObject(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
        placedGameObjects.Add(newObject);

        return placedGameObjects.Count - 1;
    }

    public void RemoveObjectAt(int gameObjectIndex)
    {
        if (placedGameObjects.Count <= gameObjectIndex || placedGameObjects[gameObjectIndex] == null)
            return;
        Destroy(placedGameObjects[gameObjectIndex]);
        placedGameObjects[gameObjectIndex] = null;
    }

    public void UpdateWallRotation(int gameObjectIndex, Vector3 rotation)
    {
        GameObject go = placedGameObjects[gameObjectIndex];
        Debug.Log($"{go.transform.rotation.x} {go.transform.rotation.y} {go.transform.rotation.z}");
        if (go.transform.rotation.y == 0)
        {
            var pos = go.transform.position;
            pos.z += 1;

            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(rotation));
        }
    }
}
