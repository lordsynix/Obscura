using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RuntimeSpline : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    private void Start()
    {
        splineContainer.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void GetSplineValues(List<Node> nodes, int resolution, out List<Vector3> verts, out List<Vector3> tangents)
    {
        // Convert nodes into indices
        List<int> splineIndices = new List<int>();
        foreach (Node node in nodes)
        {
            splineIndices.Add(node.index);
        }

        verts = new List<Vector3>();
        tangents = new List<Vector3>();

        float step = 1f / (float)resolution;
        float3 position;
        float3 tangent;

        // Creates x number of vertices based on the resolution of the road
        for (int j = 0; j < splineIndices.Count; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                float t = step * i;
                SampleSplinePoint(splineIndices[j], t, out position, out tangent);
                verts.Add(position);
                tangents.Add(tangent);
            }

            SampleSplinePoint(splineIndices[j], 1f, out position, out tangent);
            verts.Add(position);
            tangents.Add(tangent);
        }
    }

    private void SampleSplinePoint(int splineIndex, float t, out float3 position, out float3 tangent)
    {
        // Returns the position and direction of a specified point on the spline
        splineContainer.Evaluate(splineIndex, t, out position, out tangent, out float3 normal);
    }
}
