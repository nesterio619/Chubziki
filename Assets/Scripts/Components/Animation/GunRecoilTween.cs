using DG.Tweening;
using System;
using UnityEngine;

namespace Components.Animation
{

    [Serializable]
    public class GunRecoilTween
    {
        [SerializeField] private Transform recoilTransform;
        [Space(10)]
        [SerializeField] RecoilData positionRecoil;
        [Space(7)]
        [SerializeField] RecoilData rotationRecoil;

        private Vector3 _defaultLocalPosition;
        private Vector3 _defaultLocalRotation;
        private bool _initialized;

        private Tween _positionRecoilTween;
        private Tween _rotationRecoilTween;

        public void Initialize()
        {
            if (recoilTransform == null) return;
            if (_initialized) return;

            _initialized = true;
            _defaultLocalPosition = recoilTransform.localPosition;
            _defaultLocalRotation = recoilTransform.localEulerAngles;
        }

        public void PlayRecoil()
        {
            if (recoilTransform == null) return;
            if (!_initialized) Initialize();

            _positionRecoilTween.Complete();
            _rotationRecoilTween.Complete();

            recoilTransform.localPosition = _defaultLocalPosition + positionRecoil.recoilAmount;
            _positionRecoilTween = recoilTransform.DOLocalMove(_defaultLocalPosition, positionRecoil.Duration).SetEase(positionRecoil.Ease);

            recoilTransform.localRotation = Quaternion.Euler(_defaultLocalRotation + rotationRecoil.recoilAmount);
            _rotationRecoilTween = recoilTransform.DOLocalRotate(_defaultLocalRotation, rotationRecoil.Duration).SetEase(rotationRecoil.Ease);
        }

        [Serializable]
        private struct RecoilData
        {
            public Vector3 recoilAmount;
            public float Duration;
            public Ease Ease;
        }
    }
}
