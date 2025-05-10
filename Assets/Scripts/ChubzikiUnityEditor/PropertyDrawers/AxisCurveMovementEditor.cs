#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Components.Animation;
using UnityEditor;

namespace ChubzikiUnityEditor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(CurveAnimatorController.AxisCurveMovement))]
    public class AxisCurveMovementDrawer : BasePropertyDrawer
    {
        private static readonly string[] CurvePropertyNames = { "XCurve", "YCurve", "ZCurve", "XRotationCurve", "YRotationCurve", "ZRotationCurve" };

        private static readonly string[] FieldPropertyNames = { "Intensity", "Speed" };
        private const string EventPropertyName = "CurveEvent";

        protected override IEnumerable<string> PropertyNames =>
            CurvePropertyNames.Concat(FieldPropertyNames).Concat(new[] { EventPropertyName });
    }
}
#endif