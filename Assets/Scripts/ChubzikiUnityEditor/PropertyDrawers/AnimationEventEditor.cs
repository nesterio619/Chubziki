#if UNITY_EDITOR
using Components.Animation;
using System.Collections.Generic;
using UnityEditor;

namespace ChubzikiUnityEditor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(CurveAnimatorController.AnimationEvent))]
    public class AnimationEventEditor : BasePropertyDrawer
    {
        protected override IEnumerable<string> PropertyNames => new[] { "AnimationPercent", "OnAnimation" };
    }
}
#endif