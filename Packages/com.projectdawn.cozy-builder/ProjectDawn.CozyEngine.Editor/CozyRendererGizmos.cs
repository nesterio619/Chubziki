using UnityEditor;
using UnityEngine;

namespace ProjectDawn.CozyBuilder.Editor
{
    public class CozyRendererGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmoForMyScript(CozyPoint scr, GizmoType gizmoType)
        {
            Gizmos.color = (gizmoType & (GizmoType.Selected | GizmoType.InSelectionHierarchy)) != 0 ? CozyGizmos.SelectedColor : CozyGizmos.DefaultColor;
            if (gizmoType == GizmoType.Pickable)
                Gizmos.color = Color.black;
            scr.DrawPoint(Gizmos.color);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawPlane(CozyPlane scr, GizmoType gizmoType)
        {
            Gizmos.color = (gizmoType & (GizmoType.Selected | GizmoType.InSelectionHierarchy)) != 0 ? CozyGizmos.SelectedColor : CozyGizmos.DefaultColor;
            if (gizmoType == GizmoType.Pickable)
                Gizmos.color = Color.black;
            scr.DrwaPlane(Gizmos.color);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawSpline(CozySpline scr, GizmoType gizmoType)
        {
            Gizmos.color = (gizmoType & (GizmoType.Selected | GizmoType.InSelectionHierarchy)) != 0 ? CozyGizmos.SelectedColor : CozyGizmos.DefaultColor;
            if (gizmoType == GizmoType.Pickable)
                Gizmos.color = Color.black;
            scr.DrawSpline(Gizmos.color);
        }
    }
}