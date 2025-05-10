using Core;
using Core.ObjectPool;
using Core.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Components.Particles
{
	public class PooledParticle : PooledGameObject
	{
		[SerializeField] protected ParticleSystem myParticleSystem;

		public bool IsPlaying { get; private set; }
		protected bool isInitialized;
		private Coroutine _delayedDispose;

		private void OnParticleSystemStopped()
		{
			if(!isInitialized || myParticleSystem == null)
				return;
			
			if (myParticleSystem.main.loop)
			{
				IsPlaying = false;
				ReturnToPool();
			}
		}

		public static PooledParticle TryToLoadAndPlay(PrefabPoolInfo _PrefabPoolInfo, Transform parent, Vector3 offset)
		{
			var pooledParticle = Initialize(_PrefabPoolInfo);

			if (pooledParticle.transform.parent != parent)
			{
				pooledParticle.transform.SetParent(parent);
				pooledParticle.transform.localPosition = offset;
			}

			return pooledParticle;
		}

		public static PooledParticle TryToLoadAndPlay(PrefabPoolInfo _PrefabPoolInfo, Transform parent)
		{
			var pooledParticle = Initialize(_PrefabPoolInfo);

			if (pooledParticle.transform.parent != parent)
			{
				pooledParticle.transform.SetParent(parent);
				pooledParticle.transform.localPosition = Vector3.zero;
			}
			

			return pooledParticle;
		}

		public static void TryToLoadAndPlay(PrefabPoolInfo _PrefabPoolInfo, Vector3 position)
		{
			var pooledParticle = Initialize(_PrefabPoolInfo);
			pooledParticle.transform.position = position;
		}
		
		public static PooledParticle TryToLoadAndPlayLoop(PrefabPoolInfo _PrefabPoolInfo, Vector3 position)
		{
			var pooledParticle = Initialize(_PrefabPoolInfo);
			var mainParticleSystem = pooledParticle.myParticleSystem.main;
			mainParticleSystem.loop = true;
			pooledParticle.transform.position = position;
			return pooledParticle;
		}
		
		public static PooledParticle TryToLoadAndPlayLoop(PrefabPoolInfo _PrefabPoolInfo, Transform parent)
		{
			var pooledParticle = Initialize(_PrefabPoolInfo);

			if (pooledParticle == null || pooledParticle.myParticleSystem == null)
				return null;
			
			var mainParticleSystem = pooledParticle.myParticleSystem.main;
			mainParticleSystem.loop = true;
			if (pooledParticle.transform.parent != parent)
			{
				pooledParticle.transform.SetParent(parent);
				pooledParticle.transform.localPosition = Vector3.zero;
			}
			return pooledParticle;
		}
		
		protected static PooledParticle Initialize(PrefabPoolInfo _PrefabPoolInfo)
		{
			var pooledParticle = (PooledParticle)ObjectPooler.TakePooledGameObject(_PrefabPoolInfo);

			if (!pooledParticle.IsPlaying)
				pooledParticle.PlayParticle();

			pooledParticle.isInitialized = true;

			return pooledParticle;
		}
		
		protected void PlayParticle()
		{
			if (IsPlaying || myParticleSystem == null) return;

			myParticleSystem.Play();
			IsPlaying = true;
			if (!myParticleSystem.main.loop)
				_delayedDispose = UtilitiesProvider.WaitAndRun(ReturnToPool, false, myParticleSystem.main.duration);
		}
		
		public void StopParticle()
		{
			if (!IsPlaying || !isInitialized || myParticleSystem == null) return;
			myParticleSystem.Stop();
			IsPlaying = false;
		}

		public override void ReturnToPool()
		{
			if(_delayedDispose != null)
			{
				Player.Instance.StopCoroutine(_delayedDispose);
				_delayedDispose = null;
			}

			if (!isInitialized) return;
			if (myParticleSystem != null)
			{
				StopParticle();
			}
			isInitialized = false;
			base.ReturnToPool();
		}
	}
}