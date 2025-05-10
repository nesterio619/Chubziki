using UnityEngine;

namespace Core.Interfaces
{
	public interface IPushable
	{
		public abstract Rigidbody GetRigidbody();

		public const ForceMode CurrentForceMode = ForceMode.Impulse;

		public static void PerformPush(IPushable pushable, Vector3 pushDirection, float pushForce, ForceMode forceMode = CurrentForceMode)
		{
			pushable.ApplySinglePushForce(pushDirection * pushForce, forceMode);
		}
		void ApplySinglePushForce(Vector3 pushForce, ForceMode forceMode);
		Vector3 GetVelocity();
	}
}
