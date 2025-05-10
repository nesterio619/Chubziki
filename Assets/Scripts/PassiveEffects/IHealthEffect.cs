namespace PassiveEffects
{
    public interface IHealthEffect: IPassiveEffect
    {
        int DefaultHealthChangeRate { get; }
        int DefaultHealthChangeAmount { get; }

        public void ChangeHealthBy(int healthChange);
    }
}
