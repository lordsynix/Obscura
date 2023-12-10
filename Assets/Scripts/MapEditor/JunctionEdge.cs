using UnityEngine;

public struct JunctionEdge
{
    public Vector3 left;
    public Vector3 right;

    public Vector3 Center => (left + right) / 2;

    public JunctionEdge(Vector3 p1, Vector3 p2)
    {
        this.left = p1;
        this.right = p2;
    }
}