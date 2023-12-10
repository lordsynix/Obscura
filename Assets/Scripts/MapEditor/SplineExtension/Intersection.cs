using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace UnityEditor.Splines
{
    public class Intersection
    {
        public List<JunctionInfo> junctions;
        public List<float> curves = new List<float>();

        public void AddJunction(int splineIndex, int knotIndex, Spline spline, BezierKnot knot)
        {
            if (junctions == null)
            {
                junctions = new List<JunctionInfo>();
                curves = new List<float>();
            }

            junctions.Add(new JunctionInfo(splineIndex, knotIndex, spline, knot));
            curves.Add(0.3f);
        }

        public IEnumerable<JunctionInfo> GetJunctions()
        {
            return junctions;
        }
    }
}
