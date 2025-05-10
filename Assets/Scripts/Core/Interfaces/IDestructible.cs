using Core.Interfaces;
using UnityEngine;

public interface IDestructible : IDamageable
{
	float DefaultStunTime { get; }
	bool IsStanding { get; }
	void Fall(Vector3 pushForce);
	void GetUp();
}
