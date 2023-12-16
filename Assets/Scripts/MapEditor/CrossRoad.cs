using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CrossRoad
{
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector2> uvs;
    public List<int> involvedRoads;

    public CrossRoad(List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<int> involvedRoads)
    {
        this.verts = verts;
        this.tris = tris;
        this.uvs = uvs;
        this.involvedRoads = involvedRoads;
    }
}
