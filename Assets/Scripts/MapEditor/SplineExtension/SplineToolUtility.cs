using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Splines
{
    public static class SplineToolUtility
    {
        public static bool HasSelection()
        {
            return SplineSelection.HasActiveSplineSelection();
        }

        public static List<SelectedSplineElementInfo> GetSelection()
        {
            // Get internal struct data
            List<SelectableSplineElement> elements = SplineSelection.selection;

            // Make new public struct data
            List<SelectedSplineElementInfo> infos = new List<SelectedSplineElementInfo>();

            // Convert internal struct to public struct data
            foreach (SelectableSplineElement element in elements)
            {
                infos.Add(new SelectedSplineElementInfo(element.target, element.targetIndex, element.knotIndex));
            }

            // Return the public version
            return infos;
        }               
    }
}
