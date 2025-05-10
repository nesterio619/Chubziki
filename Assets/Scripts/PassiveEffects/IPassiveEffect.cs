using System.Collections;

namespace PassiveEffects
{
    public interface IPassiveEffect
    {
        public void ToggleActive(bool stateToSet);

        public IEnumerator ApplyPassiveEffect();
    }
}
