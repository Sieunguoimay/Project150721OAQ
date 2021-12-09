using System;
using UnityEngine;

namespace Movement
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
        [SerializeField, Range(0f, 10f)] private float jumpHeight = 5f;
        private Vector3 _velocity;
        private Vector3 _desiredVelocity;
        private Rigidbody _rigidbody;
        private bool _desiredJump;
        private bool _onGround;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector2 playerInput = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
            {
                playerInput.x = -1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                playerInput.y = -1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                playerInput.x = 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                playerInput.y = 1f;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _desiredJump = true;
            }

            playerInput = Vector2.ClampMagnitude(playerInput, 1f);
            _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }

        private void FixedUpdate()
        {
            var maxSpeedChange = maxAcceleration * Time.deltaTime;
            _velocity = _rigidbody.velocity;
            if (_velocity.x < _desiredVelocity.x)
            {
                _velocity.x = Mathf.Min(_velocity.x + maxSpeedChange, _desiredVelocity.x);
            }
            else if (_velocity.x > _desiredVelocity.x)
            {
                _velocity.x = Mathf.Max(_velocity.x - maxSpeedChange, _desiredVelocity.x);
            }

            if (_velocity.z < _desiredVelocity.z)
            {
                _velocity.z = Mathf.Min(_velocity.z + maxSpeedChange, _desiredVelocity.z);
            }
            else if (_velocity.z > _desiredVelocity.z)
            {
                _velocity.z = Mathf.Max(_velocity.z - maxSpeedChange, _desiredVelocity.z);
            }

            if (_desiredJump)
            {
                _desiredJump = false;
                if (_onGround)
                {
                    Jump();
                }
            }

            _rigidbody.velocity = _velocity;
            _onGround = false;
        }

        private void Jump()
        {
            _velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        }

        protected void OnCollisionEnter(Collision other)
        {
            _onGround = true;
        }

        protected void OnCollisionStay(Collision other)
        {
            _onGround = true;
        }
    }
}