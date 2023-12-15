using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode()]
public class SplineRoad : MonoBehaviour
{
    private List<Vector3> innerVerts;
    private List<Vector3> outerVerts;
    private List<Vector3> tangents;

    [SerializeField]
    public int resolution = 8;
    [SerializeField]
    public float handleSize = 0.02f;
    [SerializeField]
    private float m_width = 0.19f;
    [SerializeField]
    float m_curveStep = 0.1f;

    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private SplineContainer splineContainer;

    float3 position;
    float3 tangent;
    float3 normal;

    public List<Intersection> intersections = new List<Intersection>();
    private List<Vector3> curveVerts;
    private List<Road> roads;
    private List<CrossRoad> crossRoads;
    [SerializeField] private Transform roadContainer;
    [SerializeField] private Material roadMaterial;
    [SerializeField] private Transform crossRoadContainer;
    [SerializeField] private Material crossRoadMaterial;

    private int numSplines;
    private float editCooldown = 0.1f;
    private float editTime;
    private bool edited = false;

    [SerializeField] private float colliderFrequency = 5;

    private void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        meshFilter = GetComponent<MeshFilter>();
        editTime = Time.time;
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
        if (editTime + editCooldown < Time.time)
        {
            if (edited)
            {
                if (GetComponent<MeshCollider>())
                {
                    DestroyImmediate(GetComponent<MeshCollider>());
                }
                gameObject.AddComponent<MeshCollider>();
                edited = false;
            }
        }
        numSplines = splineContainer.Splines.Count;
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
    private void SampleSplineWidth(int splineIndex, float t, float width, out Vector3 p1, out Vector3 p2, out float3 tangent)
    {
        // Returns the position and direction of a specified point on the spline
        splineContainer.Evaluate(splineIndex, t, out position, out tangent, out normal);

        // Creates two points on the sides of the spline
        float3 right = Vector3.Cross(tangent, normal).normalized;
        p1 = position + (right * width) - (float3)transform.position;
        p2 = position + (-right * width) - (float3)transform.position;
    }

    /// <summary>
    /// Creates vertices on the right and left of the spline
    /// </summary>
    private void GetVerts()
    {
        innerVerts = new List<Vector3>();
        outerVerts = new List<Vector3>();
        tangents = new List<Vector3>();

        float step = 1f / (float)resolution;
        Vector3 p1;
        Vector3 p2;
        float3 tangent;
        
        // Creates x number of vertices based on the resolution of the road
        for (int j = 0; j < numSplines; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                float t = step * i;
                SampleSplineWidth(j, t, m_width, out p1, out p2, out tangent);
                innerVerts.Add(p1);
                outerVerts.Add(p2);
                tangents.Add(tangent);
            }

            SampleSplineWidth(j, 1f, m_width, out p1, out p2, out tangent);
            innerVerts.Add(p1);
            outerVerts.Add(p2);
            tangents.Add(tangent);
        }
    }

    public void BuildMesh()
    {
        curveVerts = new List<Vector3>();
        roads = new List<Road>();
        crossRoads = new List<CrossRoad>();

        Mesh m = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Create roads with repeating uvs to avoid stretching textures
        BuildRoads(verts, tris, uvs);       

        // If there are intersections put them into seperate submesh to apply different material
        List<int> trisB = new List<int>();
        if (intersections.Count > 0)
        {
            BuildIntersections(verts, trisB, uvs);
        }

        m.subMeshCount = 2;

        // Mesh gets created
        m.SetVertices(verts);

        // Mesh triangles get created for roads
        m.SetTriangles(tris, 0);

        // Mesh triangles get created for intersection submesh
        m.SetTriangles(trisB, 1);

        // Mesh uvs get created, which are different for roads and intersection
        m.SetUVs(0, uvs);

        // Mesh is assigned
        meshFilter.mesh = m;

        edited = true;
        editTime = Time.time;       
    }

    private void BuildRoads(List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        // Iterates through every spline to build a face
        for (int currentSplineIndex = 0; currentSplineIndex < numSplines; currentSplineIndex++)
        {
            List<Vector3> roadVerts = new List<Vector3>();
            List<int> roadTris = new List<int>();
            List<Vector2> roadUvs = new List<Vector2>();

            float uvOffset = 0;
            int splineOffset = resolution * currentSplineIndex;
            splineOffset += currentSplineIndex;

            // Assigns vertices, builds faces, and creates uvs for roads          
            for (int currentSplinePoint = 1; currentSplinePoint < resolution + 1; currentSplinePoint++)
            {
                int vertOffset = splineOffset + currentSplinePoint;
                Vector3 p1 = innerVerts[vertOffset - 1];
                Vector3 p2 = outerVerts[vertOffset - 1];
                Vector3 p3 = innerVerts[vertOffset];
                Vector3 p4 = outerVerts[vertOffset];

                int offset = 4 * resolution * currentSplineIndex;
                offset += 4 * (currentSplinePoint - 1);

                int t1 = offset + 0;
                int t2 = offset + 2;
                int t3 = offset + 3;

                int t4 = offset + 3;
                int t5 = offset + 1;
                int t6 = offset + 0;

                // Creates vertices, triangles and uvs for a whole mesh containing roads and intersection to display and edit in edit mode.
                verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
                tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });               

                float distance = Vector3.Distance(p1, p3) / 4f;
                float uvDistance = uvOffset + distance;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1) });

                // Create seperate vertices, triangles and uvs for roads to later seperate them.
                roadVerts.AddRange(new List<Vector3> { p1 + transform.position, p2 + transform.position, p3 + transform.position, p4 + transform.position});

                // We have to subtract the offset from the triangles of the seperated roads since the counting should start at 0 for a seperated mesh and not at the offset.
                roadTris.AddRange(new List<int> { t1 - 4 * resolution * currentSplineIndex, t2 - 4 * resolution * currentSplineIndex, t3 - 4 * resolution * currentSplineIndex, t4 - 4 * resolution * currentSplineIndex, t5 - 4 * resolution * currentSplineIndex, t6 - 4 * resolution * currentSplineIndex });
                
                roadUvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1) });

                uvOffset += distance;               
            }
            float roadLength = splineContainer.Splines[currentSplineIndex].GetLength();
            Road newRoad = new Road(roadVerts, roadTris, roadUvs, roadLength);

            roads.Add(newRoad);
        }
    }

    private void BuildIntersections(List<Vector3> verts, List<int> trisB, List<Vector2> uvs)
    {
        //Get intersection verts
        for (int i = 0; i < intersections.Count; i++)
        {
            Intersection intersection = intersections[i];
            int count = 0;
            List<JunctionEdge> junctionEdges = new List<JunctionEdge>();
            Vector3 center = new Vector3();

            // Cycling through a manually created list with junction vertices.
            foreach (JunctionInfo junction in intersection.GetJunctions())
            {
                int splineIndex = junction.splineIndex;
                float t = junction.knotIndex == 0 ? 0f : 1f;
                SampleSplineWidth(splineIndex, t, m_width, out Vector3 p1, out Vector3 p2, out float3 tangent);

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

            List<Vector3> curvePoints = CreateCurves(junctionEdges, center.normalized, intersection);           

            int pointsOffset = verts.Count;

            CreateIntersectionVerts(curvePoints, verts, trisB, center, pointsOffset, uvs);
        }
    }

    private int SortPoints(Vector3 center, Vector3 xCenter, Vector3 yCenter)
    {
        Vector3 xDir = xCenter - center;
        Vector3 yDir = yCenter - center;

        float angleA = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
        float angleB = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

        if (angleA > angleB)
        {
            return -1;
        }
        if (angleA < angleB)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    private List<Vector3> CreateCurves(List<JunctionEdge> junctionEdges, Vector3 center, Intersection intersection)
    {
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
            Vector3 dir = center - mid;
            mid = mid - dir;
            c = Vector3.Lerp(mid, center, intersection.curves[j - 1]);

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
        return curvePoints;
    }

    private void CreateIntersectionVerts(List<Vector3> curvePoints, List<Vector3> verts, List<int> trisB, Vector3 center, int pointsOffset, List<Vector2> uvs)
    {
        // Creates storage for vertices, triangles, and uvs that can later be seperated from the main mesh.
        CrossRoad crossRoad = new CrossRoad(new List<Vector3>(), new List<int>(), new List<Vector2>());

        for (int j = 1; j <= curvePoints.Count; j++)
        {
            Vector3 pointA = curvePoints[j - 1];
            Vector3 pointB;

            if (j == curvePoints.Count)
            {
                pointB = curvePoints[0];
            }
            else
            {
                pointB = curvePoints[j];
            }

            verts.Add(center);
            verts.Add(pointA);
            verts.Add(pointB);

            trisB.Add(pointsOffset + ((j - 1) * 3) + 0);
            trisB.Add(pointsOffset + ((j - 1) * 3) + 1);
            trisB.Add(pointsOffset + ((j - 1) * 3) + 2);

            uvs.Add(new Vector2(center.z, center.x));
            uvs.Add(new Vector2(pointA.z, pointA.x));
            uvs.Add(new Vector2(pointB.z, pointB.x));

            // Create seperate vertices, triangles, and uvs for intersections to later seperate the mesh.
            crossRoad.verts.Add(center + transform.position);
            crossRoad.verts.Add(pointA + transform.position);
            crossRoad.verts.Add(pointB + transform.position);

            crossRoad.tris.Add(((j - 1) * 3) + 0);
            crossRoad.tris.Add(((j - 1) * 3) + 1);
            crossRoad.tris.Add(((j - 1) * 3) + 2);

            crossRoad.uvs.Add(new Vector2(center.z, center.x));
            crossRoad.uvs.Add(new Vector2(pointA.z, pointA.x));
            crossRoad.uvs.Add(new Vector2(pointB.z, pointB.x));
        }
        crossRoads.Add(crossRoad);
    }

    public void AddJunction(Intersection intersection)
    {
        intersections.Add(intersection);
        BuildMesh();
    }

    public void SeperateRoads()
    {
        // Deletes all already seperated roads.
        ClearRoads();

        // Creates new seperated roads, each having their own mesh and collider.
        for (int i = 0; i < roads.Count; i++)
        {            
            GameObject roadObject = new GameObject(i.ToString());
            roadObject.transform.parent = roadContainer;
            roadObject.AddComponent(typeof(MeshRenderer));
            roadObject.AddComponent(typeof(MeshFilter));

            Mesh roadMesh = new Mesh();
            roadMesh.SetVertices(roads[i].verts);
            roadMesh.SetTriangles(roads[i].tris, 0);
            roadMesh.SetUVs(0, roads[i].uvs);

            roadObject.GetComponent<MeshFilter>().mesh = roadMesh;
            roadObject.GetComponent<MeshRenderer>().material = roadMaterial;

            BuildColliders(roads[i], roadObject);
        }

        for (int i = 0; i < crossRoads.Count; i++)
        {
            GameObject roadObject = new GameObject(i.ToString());
            roadObject.transform.parent = crossRoadContainer;
            roadObject.AddComponent(typeof(MeshRenderer));
            roadObject.AddComponent(typeof(MeshFilter));

            Mesh roadMesh = new Mesh();
            roadMesh.SetVertices(crossRoads[i].verts);
            roadMesh.SetTriangles(crossRoads[i].tris, 0);
            roadMesh.SetUVs(0, crossRoads[i].uvs);

            roadObject.GetComponent<MeshFilter>().mesh = roadMesh;
            roadObject.GetComponent<MeshRenderer>().material = crossRoadMaterial;
        }
    }

    private void ClearRoads()
    {
        int roadCount = roadContainer.childCount;
        for (int i = 0; i < roadCount; i++)
        {
            DestroyImmediate(roadContainer.GetChild(0).gameObject);
            
        }

        int crossRoadCount = crossRoadContainer.childCount;
        for (int i = 0; i < crossRoadCount; i++)
        {
            DestroyImmediate(crossRoadContainer.GetChild(0).gameObject);
        }
    }

    private void BuildColliders(Road road, GameObject roadObject)
    {
        float distance = road.length / colliderFrequency;
        Vector3 start = road.verts[0];
        Vector3 colliderDirection = (road.verts[road.verts.Count - 1] - road.verts[0]).normalized;
        for (int i = 0; i < colliderFrequency; i++)
        {
            Vector3 colliderOffset = start + colliderDirection * distance * i;

            GameObject child = new GameObject("Hull" + i.ToString());
            child.transform.parent = roadObject.transform;
            child.transform.position = colliderOffset;

            BoxCollider collider =  child.AddComponent<BoxCollider>();
            collider.size = new Vector3(distance, distance, distance);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Handles.matrix = transform.localToWorldMatrix;
        /*for (int i = 0; i < innerVerts.Count; i++)
        {
            Handles.SphereHandleCap(0, innerVerts[i], Quaternion.identity, handleSize, EventType.Repaint);
            Handles.SphereHandleCap(0, outerVerts[i], Quaternion.identity, handleSize, EventType.Repaint);
            Handles.DrawLine(outerVerts[i], innerVerts[i]);
        }
        for (int i = 0; i < curveVerts.Count; i++)
        {
            Handles.SphereHandleCap(0, curveVerts[i], Quaternion.identity, handleSize, EventType.Repaint);
        }*/
    }
}
