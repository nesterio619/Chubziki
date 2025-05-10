using System;
using MelenitasDev.SoundsGood;
using UnityEngine;

namespace Components.Car.CarLogic
{
    public class CarSound: IDisposable
    {
        private Sound _motorSound;
        private Sound _screechSound;
        private Sound _moveSound;

        private bool _isDrifting;

        private WheelLogic _wheelLogic = null;

        private float _xVelocityForEffect = 5;

        private float _xSpeedForEffect = 12;

        public void Initialize(WheelLogic wheelLogic, GameObject car)
        {
            _wheelLogic = wheelLogic;

            _screechSound = new Sound(SFX.brake);
            _motorSound = new Sound(SFX.motorEngine);
            _moveSound = new Sound(SFX.moveSound);

            new Sound(SFX.engineStart).Play();

            _screechSound
                .SetLoop(true)
                .SetFollowTarget(car.transform)
                .SetVolume(0.3f);
            _motorSound
                .SetLoop(true)
                .SetFollowTarget(car.transform)
                .SetVolume(0.3f);

            _moveSound
                .SetLoop(true)
                .SetFollowTarget(car.transform)
                .SetVolume(0.3f);

            _motorSound.Play();

            wheelLogic.OnMotor += MotorCheck;

            wheelLogic.OnDrift += SetDriftMode;

            wheelLogic.OnTraction += DfirftTrail;

        }

        void SetDriftMode(bool isDrifting)
        {
            _isDrifting = isDrifting;
        }

        public void Dispose()
        {
            _wheelLogic.OnMotor -= MotorCheck;

            _wheelLogic.OnDrift -= SetDriftMode;

            _wheelLogic.OnTraction -= DfirftTrail;
            
            _motorSound.Stop();
            _moveSound.Stop();
            
            _screechSound = null;
        }

        public void DfirftTrail(bool isTractionLocked, float xLocalVelocity, float wheelSpeed)
        {
            if ((isTractionLocked || Mathf.Abs(xLocalVelocity) > _xVelocityForEffect) && Mathf.Abs(wheelSpeed) > _xSpeedForEffect || _isDrifting)
            {
                if (!_screechSound.Using)
                    _screechSound
                    .Play();
            }
            else
            {
                if (_screechSound.Using)
                    _screechSound
                    .Stop();
            }

        }


        public void MotorCheck(float motorTorque)
        {
            if (!_moveSound.Using && motorTorque > 0)
            {
                _moveSound.Play();
                _motorSound.Stop();
            }
            else if (!_motorSound.Using && motorTorque == 0)
            {
                _motorSound.Play();
                _moveSound.Stop();
            }
        }

        [ContextMenu("OffCarSoun")]
        public void OffCarSound()
        {
            _motorSound.Stop();
            _moveSound.Stop();

        }


    }
}