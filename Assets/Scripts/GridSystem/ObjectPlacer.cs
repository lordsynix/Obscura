using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> placedGameObjects = new();

    public int PlaceObject(GameObject prefab, Vector3 worldPos, Vector3 position, Vector3 rotation)
    {
        GameObject newObject = Instantiate(prefab, worldPos, Quaternion.identity);

        if (position != Vector3.zero)
        {
            newObject.transform.GetChild(0).position = position;
            newObject.transform.GetChild(0).position += worldPos;
        }
        newObject.transform.GetChild(0).rotation = Quaternion.Euler(rotation);

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

    public void UpdateWall(int gameObjectIndex, GameObject prefab, Vector3Int worldPos, Vector3 position, Vector3 rotation)
    {
        GameObject go = placedGameObjects[gameObjectIndex];
        Destroy(go);

        GameObject newObject = Instantiate(prefab, worldPos, Quaternion.identity);

        if (position != Vector3.zero)
        {
            newObject.transform.GetChild(0).position = position;
            newObject.transform.GetChild(0).position += worldPos;
        }
        newObject.transform.GetChild(0).rotation = Quaternion.Euler(rotation);

        placedGameObjects[gameObjectIndex] = newObject;
    }
}
