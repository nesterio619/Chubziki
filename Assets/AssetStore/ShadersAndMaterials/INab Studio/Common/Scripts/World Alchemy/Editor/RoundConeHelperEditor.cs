using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


namespace INab.WorldAlchemy
{

	[CustomEditor(typeof(RoundConeHelper)), CanEditMultipleObjects]
    public class RoundConeHelperEditor : Editor
	{
		private RoundConeHelper roundConeHelper;
		private MaskSettings m_GizmoSettings;

		private Transform startTransform;
		private Transform endTransform;

		private void OnEnable()
		{
			roundConeHelper = (RoundConeHelper)target;
			startTransform = roundConeHelper.mask.startTransform;
			endTransform = roundConeHelper.mask.endTransform;
			m_GizmoSettings = roundConeHelper.mask.maskSettings;
		}
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
		}

		private void OnSceneGUI()
		{
			Handles.color = m_GizmoSettings.Color;
			if (startTransform != null && endTransform != null)
				HandlesUtilities.DrawConeLines(startTransform, endTransform);
		}
	}
}