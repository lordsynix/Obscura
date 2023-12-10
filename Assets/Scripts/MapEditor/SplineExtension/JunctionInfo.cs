using UnityEngine.Splines;

namespace UnityEditor.Splines
{
    public struct JunctionInfo
    {
        public int splineIndex;
        public int knotIndex;
        public Spline spline;
        public BezierKnot knot;

        public JunctionInfo(int splineIndex, int knotIndex, Spline spline, BezierKnot knot)
        {
            this.splineIndex = splineIndex;
            this.knotIndex = knotIndex;
            this.spline = spline;
            this.knot = knot;
        }
    }
}
