using Actors.AI;
using Components.ProjectileSystem.AttackPattern;
using Core.Utilities;
using Regions;
using UnityEngine;

namespace Actors.EditorActors
{
    [ExecuteInEditMode]
    public class TowerEditorActor : EditorActor
    {
        [SerializeField, HideInInspector] private RangedAttackPattern _attackPattern;
        [SerializeField, HideInInspector] private Vector3 _shootPoint;

        [SerializeField] private bool showGizmos = true;

#if UNITY_EDITOR
        public override void Initialize(Location location)
        {
            base.Initialize(location);

            var tower = GetComponent<TowerActor>();

            AssetUtils.TryLoadAsset(tower.attackPoolPattern_PrefabPoolInfo.ObjectPath, out GameObject patternPrefab);
            _attackPattern = patternPrefab.GetComponent<RangedAttackPattern>();
            _shootPoint = tower.FirePoint.localPosition;

        }
#endif

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = Color.yellow;

            if (_attackPattern != null)
                Gizmos.DrawWireSphere(transform.position + _shootPoint, _attackPattern.FiringRadius);
        }
    }
}