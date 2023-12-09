using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[ExecuteInEditMode()]
public class SplineRoad : MonoBehaviour
{
    private List<Vector3> m_vertsP1;
    private List<Vector3> m_vertsP2;
    public int resolution = 5;
    public float handleSize = 0.25f;
    private SplineSampler m_sampler;
    [SerializeField]
    private MeshFilter m_meshFilter;

    private void Start()
    {
        m_sampler = GetComponent<SplineSampler>();
    }

    private void Update()
    {
        GetVerts();
    }

    private void GetVerts()
    {
        m_vertsP1 = new List<Vector3>();
        m_vertsP2 = new List<Vector3>();

        float step = 1f / (float)resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = step * i;
            m_sampler.SampleSplineWidth(t, out Vector3 p1, out Vector3 p2);
            m_vertsP1.Add(p1);
            m_vertsP2.Add(p2);
        }
    }

    public void BuildMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        int offset = 0;

        int length = m_vertsP2.Count;

        // Iterate verts and build a face
        for (int i=1; i <= length; i++)
        {
            Vector3 p1 = m_vertsP1[i - 1];
            Vector3 p2 = m_vertsP2[i - 1];
            Vector3 p3;
            Vector3 p4;

            if (i == length)
            {
                p3 = m_vertsP1[0];
                p4 = m_vertsP2[0];
            }
            else
            {
                p3 = m_vertsP1[i];
                p4 = m_vertsP2[i];
            }

            offset = 4 * (i - 1);
            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;

            verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
            tris.AddRange(new List<int> {  t1, t2, t3, t4, t5, t6 });
        }

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m_meshFilter.mesh = m;
    }

    private void OnDrawGizmos()
    {
        Handles.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < m_vertsP1.Count; i++)
        {
            Handles.SphereHandleCap(0, m_vertsP1[i], Quaternion.identity, handleSize, EventType.Repaint);
            Handles.SphereHandleCap(0, m_vertsP2[i], Quaternion.identity, handleSize, EventType.Repaint);
            Handles.DrawLine(m_vertsP2[i], m_vertsP1[i]);
        }
    }

    
}
