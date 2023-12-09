using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineRoad))]
class RoadToolWindow : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var linkedObject = target as SplineRoad;
        if (GUILayout.Button("Build Mesh"))
        {
            linkedObject.BuildMesh();
        }
    }
}