using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Road
{
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector2> uvs;
    public List<Vector3> tangents;
    public float length;

    public Road(List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Vector3> tangents, float length)
    {
        this.verts = verts;
        this.tris = tris;
        this.uvs = uvs;
        this.tangents = tangents;
        this.length = length;
    }
}
