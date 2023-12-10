using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEditor.PlayerSettings;

[ExecuteInEditMode()]
public class SplineRoad : MonoBehaviour
{
    private List<Vector3> innerVerts;
    private List<Vector3> outerVerts;

    public int resolution = 5;

    public float handleSize = 0.25f;
    [SerializeField]
    private MeshFilter m_meshFilter;

    [SerializeField]
    private SplineContainer m_splineContainer;
    [SerializeField]
    private float m_width = 0.1f;
    private int numSplines = 1;
    [SerializeField]
    float m_curveStep = 1;

    float3 position;
    float3 tangent;
    float3 normal;

    public List<Intersection> intersections = new List<Intersection>();
    private List<Vector3> curveVerts;

    private void Start()
    {
        m_splineContainer = GetComponent<SplineContainer>();
        m_meshFilter = GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {        
        Spline.Changed += OnSplineChanged;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline arg1, int arg2, SplineModification arg3)
    {
        BuildMesh();
    }

    private void Update()
    {
        numSplines = m_splineContainer.Splines.Count;
        GetVerts();
    }

    /// <summary>
    /// Returns 2 points p1 and p2 to the sides of a specified point on a spline. The distance between the two points is the width of the road.
    /// </summary>
    /// <param name="splineIndex"></param>
    /// <param name="t"></param>
    /// <param name="width"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    private void SampleSplineWidth(int splineIndex, float t, float width, out Vector3 p1, out Vector3 p2)
    {
        // Returns the position and direction of a specified point on the spline
        m_splineContainer.Evaluate(splineIndex, t, out position, out tangent, out normal);

        // Creates two points on the sides of the spline
        float3 right = Vector3.Cross(tangent, normal).normalized;
        p1 = position + (right * width);
        p2 = position + (-right * width);
    }

    /// <summary>
    /// Creates vertices for a road mesh
    /// </summary>
    private void GetVerts()
    {
        innerVerts = new List<Vector3>();
        outerVerts = new List<Vector3>();

        float step = 1f / (float)resolution;
        Vector3 p1;
        Vector3 p2;
        for (int j = 0; j < numSplines; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                float t = step * i;
                SampleSplineWidth(j, t, m_width, out p1, out p2);
                innerVerts.Add(p1);
                outerVerts.Add(p2);
            }

            SampleSplineWidth(j, 1f, m_width, out p1, out p2);
            innerVerts.Add(p1);
            outerVerts.Add(p2);
        }
    }

    public void BuildMesh()
    {
        curveVerts = new List<Vector3>();
        Mesh m = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        int offset = 0;

        int length = outerVerts.Count;

        // Iterates through every spline to build face
        for (int currentSplineIndex = 0; currentSplineIndex < numSplines; currentSplineIndex++)
        {
            int splineOffset = resolution * currentSplineIndex;
            splineOffset += currentSplineIndex;
            // Iterate verts and build a face
            for (int currentSplinePoint = 1; currentSplinePoint < resolution + 1; currentSplinePoint++)
            {
                int vertOffset = splineOffset + currentSplinePoint;
                Vector3 p1 = innerVerts[vertOffset - 1];
                Vector3 p2 = outerVerts[vertOffset - 1];
                Vector3 p3 = innerVerts[vertOffset];
                Vector3 p4 = outerVerts[vertOffset];

                offset = 4 * resolution * currentSplineIndex;
                offset += 4 * (currentSplinePoint - 1);

                int t1 = offset + 0;
                int t2 = offset + 2;
                int t3 = offset + 3;

                int t4 = offset + 3;
                int t5 = offset + 1;
                int t6 = offset + 0;

                verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
                tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
            }
        }

        // If there are intersections between splines, they will be built here
        offset = verts.Count;

        if (intersections.Count > 0)
        {
            //Get intersection verts
            for (int i = 0; i < intersections.Count; i++)
            {
                Intersection intersection = intersections[i];
                int count = 0;

                List<JunctionEdge> junctionEdges = new List<JunctionEdge>();

                Vector3 center = new Vector3();
                foreach (JunctionInfo junction in intersection.GetJunctions())
                {
                    int splineIndex = junction.splineIndex;
                    float t = junction.knotIndex == 0 ? 0f : 1f;
                    SampleSplineWidth(splineIndex, t, m_width, out Vector3 p1, out Vector3 p2);

                    if (junction.knotIndex == 0)
                    {
                        junctionEdges.Add(new JunctionEdge(p1, p2));
                    }
                    else
                    {
                        junctionEdges.Add(new JunctionEdge(p2, p1));
                    }
                    center += p1;
                    center += p2;
                    count++;
                }

                // Get the center of all the points
                center = center / (junctionEdges.Count * 2);

                // Sort the points based on their direction from the center
                junctionEdges.Sort((x, y) => SortPoints(center, x.Center, y.Center));
                junctionEdges.Reverse();
                
                List<Vector3> curvePoints = new List<Vector3>();
                // Add additional points
                Vector3 mid;
                Vector3 c;
                Vector3 b;
                Vector3 a;
                BezierCurve curve;
                for (int j = 1; j <= junctionEdges.Count; j++)
                {
                    a = junctionEdges[j - 1].left;
                    curvePoints.Add(a);
                    b = (j < junctionEdges.Count) ? junctionEdges[j].right : junctionEdges[0].right;
                    mid = Vector3.Lerp(a, b, 0.5f);
                    c = Vector3.Lerp(mid, center, intersection.curves[j-1]);

                    curve = new BezierCurve(a, c, b);
                    for (float t = 0f; t < 1f; t += m_curveStep)
                    {
                        Vector3 pos = CurveUtility.EvaluatePosition(curve, t);
                        curvePoints.Add(pos);
                        curveVerts.Add(pos);
                    }

                    curvePoints.Add(b);
                    curveVerts.Add(b);
                }

                curvePoints.Reverse();

                int pointsOffset = verts.Count;

                for (int j = 1; j <= curvePoints.Count; j++)
                {
                    verts.Add(center);
                    verts.Add(curvePoints[j - 1]);
                    if (j == curvePoints.Count)
                    {
                        verts.Add(curvePoints[0]);
                    }
                    else
                    {
                        verts.Add(curvePoints[j]);
                    }

                    tris.Add(pointsOffset + ((j - 1) * 3) + 0);
                    tris.Add(pointsOffset + ((j - 1) * 3) + 1);
                    tris.Add(pointsOffset + ((j - 1) * 3) + 2);
                }
            }
        }

        // Mesh gets created
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m_meshFilter.mesh = m;
    }

    private int SortPoints(Vector3 center, Vector3 xCenter, Vector3 yCenter)
    {
        Vector3 xDir = xCenter - center;
        Vector3 yDir = yCenter - center;

        float angleA = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
        float angleB = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

        if (angleA > angleB)
        {
            return 1;
        }
        if (angleA < angleB)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public void AddJunction(Intersection intersection)
    {
        intersections.Add(intersection);
        BuildMesh();
    }

    private void OnDrawGizmos()
    {
        Handles.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < innerVerts.Count; i++)
        {
            Handles.SphereHandleCap(0, innerVerts[i], Quaternion.identity, handleSize, EventType.Repaint);
            Handles.SphereHandleCap(0, outerVerts[i], Quaternion.identity, handleSize, EventType.Repaint);
            Handles.DrawLine(outerVerts[i], innerVerts[i]);
        }
        for (int i = 0; i < curveVerts.Count; i++)
        {
            Handles.SphereHandleCap(0, curveVerts[i], Quaternion.identity, handleSize, EventType.Repaint);
        }
    }
}
