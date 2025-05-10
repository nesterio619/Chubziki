using System;

namespace Core.Interfaces
{
	public interface IAttacker
	{
		int AttackDamage { get; }

		void PerformSingleAttack(IDamageable target, Action OnAttackFinished) { }

		void StartAttacking(IDamageable target) { }

		void StopAttacking() { }
	}
}