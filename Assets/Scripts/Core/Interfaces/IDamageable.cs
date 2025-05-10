using System;
namespace Core.Interfaces
{
	public interface IDamageable : IPushable
	{
		int MaxHealth { get; }

		int CurrentHealth { get; }

		void ChangeHealthBy(int changeAmount);

		void ResetHealth();
	}
}

