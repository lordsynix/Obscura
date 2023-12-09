using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

[ExecuteInEditMode()]
public class SplineSampler : MonoBehaviour
{
    [SerializeField]
    private SplineContainer m_splineContainer;

    [SerializeField]
    private int m_splineIndex;
    [SerializeField]
    private float m_width;

    float3 position;
    float3 tangent;
    float3 normal;

    public void SampleSplineWidth(float t, out Vector3 p1, out Vector3 p2)
    {
        m_splineContainer.Evaluate(m_splineIndex, t, out position, out tangent, out normal);

        float3 right = Vector3.Cross(tangent, normal).normalized;
        p1 = position + (right * m_width);
        p2 = position + (-right * m_width);
    }
}