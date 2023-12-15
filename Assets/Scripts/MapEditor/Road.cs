using System.Collections.Generic;
using UnityEngine;

public struct Road
{
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector2> uvs;

    public Road(List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        this.verts = verts;
        this.tris = tris;
        this.uvs = uvs;
    }
}