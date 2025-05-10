using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDawn.CozyBuilder.Editor
{
    [EditorToolbarElement(id, typeof(SceneView))]
    class CreatePoint : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "CozyToolbar/CreatePoint";

        public EditorWindow containerWindow { get; set; }

        public CreatePoint()
        {
            icon = EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_CreatePoint@64.png") as Texture2D;
            tooltip = "Create Point";
            clicked += OnClick;

        }

        void OnClick()
        {
            var newGameObject = new GameObject("Point");

            newGameObject.AddComponent<CozyPoint>();

            Undo.RegisterCreatedObjectUndo(newGameObject, "Create Point");

            if (Selection.activeTransform != null)
            {
                newGameObject.transform.position = Selection.activeTransform.position;

                if (Selection.activeTransform.GetComponent<CozyPoint>() != null && Selection.activeTransform.parent != null)
                    Undo.SetTransformParent(newGameObject.transform, Selection.activeTransform.parent, "Set Parent");
                else
                    Undo.SetTransformParent(newGameObject.transform, Selection.activeTransform, "Set Parent");

            }

            Selection.activeTransform = newGameObject.transform;

            //if (containerWindow is SceneView view)
            //    view.FrameSelected();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class CreateSpline : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "CozyToolbar/CreateSpline";

        public EditorWindow containerWindow { get; set; }

        public CreateSpline()
        {
            icon = EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_CreateSpline@64.png") as Texture2D;
            tooltip = "Create Spline";
            clicked += OnClick;
        }

        void OnClick()
        {
            var newGameObject = new GameObject("Spline");

            // Add the CozyPoint component to the GameObject
            var spline = newGameObject.AddComponent<CozySpline>();
            foreach (var selection in Selection.gameObjects)
            {
                if (selection.TryGetComponent(out CozyPoint point))
                    spline.Points.Add(point);
            }

            // Register the creation of the object for undo (so that it's undoable)
            Undo.RegisterCreatedObjectUndo(newGameObject, "Create Spline");

            if (Selection.activeTransform != null && Selection.activeTransform.parent != null)
                Undo.SetTransformParent(newGameObject.transform, Selection.activeTransform.parent, "Set Parent");

            Selection.activeTransform = newGameObject.transform;

            //if (containerWindow is SceneView view)
            //    view.FrameSelected();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class CreatePlane : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "CozyToolbar/CreatePlane";

        public EditorWindow containerWindow { get; set; }

        public CreatePlane()
        {
            icon = EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_CreatePlane@64.png") as Texture2D;
            tooltip = "Create Plane";
            clicked += OnClick;
        }

        void OnClick()
        {
            if (Selection.gameObjects.Length != 2)
                return;

            if (!Selection.gameObjects[0].TryGetComponent(out CozySpline splineA) || !Selection.gameObjects[1].TryGetComponent(out CozySpline splineB))
                return;

            var newGameObject = new GameObject("Plane");

            // Add the CozyPoint component to the GameObject
            var plane = newGameObject.AddComponent<CozyPlane>();
            plane.SplineA = splineA;
            plane.SplineB = splineB;

            // Register the creation of the object for undo (so that it's undoable)
            Undo.RegisterCreatedObjectUndo(newGameObject, "Create Plane");

            if (Selection.activeTransform != null && Selection.activeTransform.parent != null)
                Undo.SetTransformParent(newGameObject.transform, Selection.activeTransform.parent, "Set Parent");

            Selection.activeTransform = newGameObject.transform;

            //if (containerWindow is SceneView view)
            //    view.FrameSelected();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class SelectPoints : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "CozyToolbar/Select Points";

        public EditorWindow containerWindow { get; set; }

        public SelectPoints()
        {
            icon = EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_SplineSelectPoints@64.png") as Texture2D;
            tooltip = "Select Points";
            clicked += OnClick;
        }

        void OnClick()
        {
            List<GameObject> pointsToSelect = new();
            foreach (var selection in Selection.gameObjects)
            {
                if (selection.TryGetComponent(out CozySpline spline))
                {
                    if (!spline.IsValid)
                        continue;
                    foreach (var point in spline.Points)
                        pointsToSelect.Add(point.gameObject);
                }
                if (selection.TryGetComponent(out CozyPlane plane))
                {
                    if (!plane.IsValid)
                        continue;
                    foreach (var point in plane.SplineA.Points)
                        pointsToSelect.Add(point.gameObject);
                    foreach (var point in plane.SplineB.Points)
                        pointsToSelect.Add(point.gameObject);
                }
            }

            Selection.objects = pointsToSelect.ToArray();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class Invert : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "CozyToolbar/Invert";

        public EditorWindow containerWindow { get; set; }

        public Invert()
        {
            icon = EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_Invert@64.png") as Texture2D;
            tooltip = "Invert";
            clicked += OnClick;
        }

        void OnClick()
        {
            foreach (var selection in Selection.gameObjects)
            {
                if (selection.TryGetComponent(out CozyPlane plane))
                {
                    if (!plane.IsValid)
                        continue;
                    CozySpline temp = plane.SplineA;
                    plane.SplineA = plane.SplineB;
                    plane.SplineB = temp;
                }

                if (selection.TryGetComponent(out CozySpline spline))
                {
                    if (!spline.IsValid)
                        continue;
                    spline.Points.Reverse();
                }
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class Split : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "CozyToolbar/Extend Spline";

        public EditorWindow containerWindow { get; set; }

        public Split()
        {
            icon = EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_SplineExtend@64.png") as Texture2D;
            tooltip = "Extend Spline";
            clicked += OnClick;
        }

        void OnClick()
        {
            var newSelection = new List<GameObject>();
            foreach (var selection in Selection.gameObjects)
            {
                if (selection.TryGetComponent(out CozySpline spline))
                {
                    if (!spline.IsValid)
                        continue;

                    var newGameObject = new GameObject("Point");

                    var newPoint = newGameObject.AddComponent<CozyPoint>();

                    Undo.RegisterCreatedObjectUndo(newGameObject, "Create Point");

                    if (Selection.activeTransform != null)
                    {
                        float3 direction = math.normalizesafe(spline.SampleSpline(1.0f) - spline.SampleSpline(0.9f));
                        newPoint.Position = spline.SampleSpline(1.0f) + direction * 0.5f;

                        if (selection.transform.parent != null)
                            Undo.SetTransformParent(newGameObject.transform, selection.transform.parent, "Set Parent");
                        else
                            Undo.SetTransformParent(newGameObject.transform, selection.transform, "Set Parent");

                    }

                    Undo.RecordObject(spline, "Add point");

                    spline.Points.Add(newPoint);

                    newSelection.Add(newGameObject);
                }
            }
            Selection.objects = newSelection.ToArray();
        }
    }

    class EditorToolbarImage : VisualElement
    {
        public EditorToolbarImage(Texture2D icon)
        {
            // Create an Image element and assign the texture
            var image = new Image
            {
                image = icon,
                style =
                {
                    width = 16,   // Set desired width
                    height = 16,  // Set desired height
                    marginLeft = 4,
                    marginRight = 4,
                }
            };

            // Add the image to the toolbar element
            Add(image);
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class SplineTools : OverlayToolbar
    {
        public const string id = "CozyToolbar/Spline Tools";

        VisualElement Image;
        SelectPoints SelectPoints = new();
        Invert Invert = new();
        Split Extend = new();
        bool Active = true;

        public SplineTools()
        {
            Image = new EditorToolbarImage(EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_CozySpline@64.png") as Texture2D);
            Add(Image);

            Add(SelectPoints);
            Add(Invert);
            Add(Extend);

            Selection.selectionChanged += () =>
            {
                UpdateVisibility();
            };

            SetupChildrenAsButtonStrip();

            UpdateVisibility();
        }

        public void UpdateVisibility()
        {
            bool selected = Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<CozySpline>();
            if (selected && !Active)
            {
                Active = true;
                Add(Image);
                Add(SelectPoints);
                Add(Invert);
                Add(Extend);
            }
            else if (!selected && Active)
            {
                Active = false;
                Remove(Image);
                Remove(SelectPoints);
                Remove(Invert);
                Remove(Extend);
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class PlaneTools : OverlayToolbar
    {
        public const string id = "CozyToolbar/Plane Tools";

        VisualElement Image;
        SelectPoints SelectPoints = new();
        Invert Invert = new();
        bool Active = true;

        public PlaneTools()
        {
            Image = new EditorToolbarImage(EditorGUIUtility.Load($"Packages/com.projectdawn.cozy-builder/Icons/d_CozyPlane@64.png") as Texture2D);
            Add(Image);

            Add(SelectPoints);
            Add(Invert);

            Selection.selectionChanged += () =>
            {
                UpdateVisibility();
            };

            SetupChildrenAsButtonStrip();

            UpdateVisibility();
        }

        public void UpdateVisibility()
        {
            bool selected = Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<CozyPlane>();
            if (selected && !Active)
            {
                Active = true;
                Add(Image);
                Add(SelectPoints);
                Add(Invert);
            }
            else if (!selected && Active)
            {
                Active = false;
                Remove(Image);
                Remove(SelectPoints);
                Remove(Invert);
            }
        }
    }

    [Overlay(typeof(SceneView), "Cozy Tools")]
    [Icon("Assets/PlacementToolsIcon.png")]
    public class CozyOverlay : ToolbarOverlay
    {
        CozyOverlay() : base(
            CreatePoint.id,
            CreateSpline.id,
            CreatePlane.id,
            SplineTools.id,
            PlaneTools.id
            )
        {
        }
    }
}
