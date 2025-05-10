using UnityEngine;

namespace INab.WorldAlchemy
{
	[CreateAssetMenu(fileName = "Data", menuName = "World Dissolve/Mask Settings", order = 1)]
	public class MaskSettings : ScriptableObject
	{
		public Color Color = new Color(0.1f, 0.97f, 0.1f, 0.7f);
		public float NormalSize = 1;
		public float PlaneSize = 1;

		public Mesh PlaneMesh;
		public Mesh SphereMesh;
		public Mesh BoxMesh;
		public Mesh SolidAngleMesh;

		public Material PreviewMaterial;
		public Mesh PreviewPlaneMesh;
		public Mesh PreviewSphereMesh;
		public Mesh PreviewBoxMesh;
		public Mesh PreviewSolidAngleMesh;
	}
}