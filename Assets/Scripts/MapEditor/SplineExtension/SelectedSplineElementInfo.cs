using UnityEngine;

namespace UnityEditor.Splines
{
    public struct SelectedSplineElementInfo
    {
        public Object target;
        public int targetIndex;
        public int knotIndex;
        public SelectedSplineElementInfo(Object Object, int Index, int knot)
        {
            target = Object;
            targetIndex = Index;
            knotIndex = knot;
        }
    }
}
