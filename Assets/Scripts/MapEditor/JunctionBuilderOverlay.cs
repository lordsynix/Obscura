using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using UnityEditor.Splines;
using UnityEngine;

[Overlay(typeof(SceneView), "Junction Builder", true)]
public class JunctionBuilderOverlay : Overlay
{
    Label SelectionInfoLabel;
    Button generateButton;
    VisualElement root;
    VisualElement sliderArea;

    public override VisualElement CreatePanelContent()
    {
        root = new VisualElement();
        sliderArea = new VisualElement();
        var titleLabel = new Label(text: "Junction Builder");
        generateButton = new Button();

        generateButton.text = "Generate Junction";
        generateButton.clicked += OnBuildJunction;

        SelectionInfoLabel = new Label(text: "");

        root.Add(titleLabel);
        root.Add(SelectionInfoLabel);
        root.Add(generateButton);

        SplineSelection.changed += OnSelectionChanged;
        return root;
    }

    private void OnBuildJunction()
    {
        List<SelectedSplineElementInfo> selection = SplineToolUtility.GetSelection();

        Intersection intersection = new Intersection();
        foreach (SelectedSplineElementInfo item in selection)
        {
            // Get the spline container
            SplineContainer container = (SplineContainer)item.target;
            Spline spline = container.Splines[item.targetIndex];
            intersection.AddJunction(item.targetIndex, item.knotIndex, spline, spline.Knots.ToList()[item.knotIndex]);
        }

        Selection.activeGameObject.GetComponent<SplineRoad>().AddJunction(intersection);
    }

    private void OnSelectionChanged()
    {
        UpdateSelectionInfo();
    }

    private void ClearSelectionInfo()
    {
        SelectionInfoLabel.text = string.Empty;
    }

    private void UpdateSelectionInfo()
    {
        generateButton.visible = true;
        ClearSelectionInfo();
        List<SelectedSplineElementInfo> infos = SplineToolUtility.GetSelection();

        for (int i = 0; i < root.childCount; i++)
        {
            if (root[i].GetType() == typeof(Slider))
            {
                root.Remove(root[i]);
            }
        }

        foreach (SelectedSplineElementInfo element in infos)
        {           
            SelectionInfoLabel.text += $"Spline {element.targetIndex}, Knot {element.knotIndex} \n";

            

            foreach (Intersection intersection in Selection.activeGameObject.GetComponent<SplineRoad>().intersections)
            {
                foreach (JunctionInfo junction in intersection.GetJunctions())
                {
                    if (junction.knotIndex == element.knotIndex)
                    {
                        if (junction.splineIndex == element.targetIndex)
                        {
                            ShowIntersection(intersection);
                        }
                    }                   
                }
            }
        }
    }

    private void ShowIntersection(Intersection intersection)
    {
        SelectionInfoLabel.text = "Selected Intersection";
        generateButton.visible = false;

        for (int i = 0; i < root.childCount; i++)
        {
            if (root[i].GetType() == typeof(Slider))
            {
                root.Remove(root[i]);
            }
        }        

        for (int i = 0; i < intersection.curves.Count; i++)
        {
            int value = i;
            Slider slider = new Slider($"Curve {i}", 0, 1, SliderDirection.Horizontal);
            slider.labelElement.style.minWidth = 60;
            slider.labelElement.style.maxWidth = 80;
            slider.value = intersection.curves[i];
            slider.RegisterValueChangedCallback((x) =>
            {
                intersection.curves[value] = x.newValue;
                Selection.activeGameObject.GetComponent<SplineRoad>().BuildMesh();
            });
            root.Add(slider);
        }
        
    }
}
