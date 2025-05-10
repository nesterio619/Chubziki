using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace RSM
{
    public class ExampleCharacter : MonoBehaviour, IStateBehaviour
    {
        [SerializeField] private Vector2 _velocity;
        [SerializeField] private float _gravityStrength;
        [SerializeField] private float _speed;
        [SerializeField] private float _jumpStrength;
        [SerializeField] private LayerMask _terrainMask;

        private const int _MAX_JUMPS = 1;
        private int _remainingJumps;

        private int _horizontalInput;
        private bool _jumpHeld;

        private void Update()
        {
            GetInputs();
        }

        private void GetInputs()
        {
            if (Input.GetKeyDown(KeyCode.Space)) JumpTrigger.Trigger();
            if (Input.GetKeyUp(KeyCode.Space)) JumpTrigger.Cancel();
            _jumpHeld = Input.GetKey(KeyCode.Space);
            _horizontalInput = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) _horizontalInput -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) _horizontalInput += 1;
        }

        private void Gravity()
            => _velocity.y -= _gravityStrength;
        private void AirControl()
        {
            _velocity.x += _horizontalInput * _speed * 0.005f;
            _velocity.x = Mathf.Clamp(_velocity.x, -_speed, _speed);
        }
        private void GroundControl()
        {
            _velocity.x += _horizontalInput * _speed * 0.05f;
            _velocity.x = Mathf.Clamp(_velocity.x, -_speed, _speed);
        }

        private void ApplyVelocity()
            => transform.Translate(_velocity * Time.deltaTime);

        private void CeilingCollision()
        {
            if (Physics2D.Raycast(transform.position + new Vector3(0, 0.2f, 0), Vector2.up, 0.05f, _terrainMask))
            {
                if (_velocity.y > 0) _velocity.y = -0.01f;
            }
        }

        #region StateBehaviourMethods
        [State] private void EnterIdle()
        {
            _remainingJumps = _MAX_JUMPS;
            _velocity.y = 0;
        }
        [State] private void Idle()
        {
            _velocity.y = 0;
            _velocity.x *= 0.98f;
            ApplyVelocity();
        }
        [State] private void EnterRun()
        {
            _remainingJumps = _MAX_JUMPS;
            _velocity.y = 0;
        }
        [State] private void Run()
        {
            GroundControl();
            _velocity.y = 0;
            ApplyVelocity();
        }
        [State] private void Fall()
        {
            CeilingCollision();
            AirControl();
            Gravity();
            ApplyVelocity();
        }
        [State] private void EnterJump()
        {
            _remainingJumps--;
            _velocity.x = _horizontalInput * _speed;
            _velocity.y = _jumpStrength;
        }
        [State] private void Jump()
        {
            CeilingCollision();
            AirControl();
            ApplyVelocity();
        }
        [State] private void EnterDie()
        {
            _velocity = Vector2.zero;
        }
        #endregion
        
        #region Triggers
        
        // ReSharper disable once InconsistentNaming
        
        [Trigger] public bool JumpTrigger;
        
        [Trigger] public bool DieTrigger;
        #endregion

        #region Conditions
        [Condition] public bool IsGrounded => Physics2D.Raycast(transform.position + new Vector3(0, -0.2f, 0), Vector2.down, 0.05f, _terrainMask);
        [Condition] public bool IsFalling => _velocity.y < 0;
        [Condition] public bool JumpHeld => _jumpHeld;
        [Condition] public bool CanJump => _remainingJumps > 0 ;
        [Condition] public bool MoveHeld => _horizontalInput != 0;

        #endregion


    }
    
}