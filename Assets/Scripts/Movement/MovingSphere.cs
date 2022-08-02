using System;
using UnityEngine;

namespace Movement
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
        [SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
        [SerializeField, Range(0f, 10f)] private float jumpHeight = 5f;
        [SerializeField, Range(0, 5)] private int maxAirJumps = 2;
        [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;
        [SerializeField, Range(0f, 100f)] private float maxSnappingSpeed = 8f;
        [SerializeField, Min(0f)] private float probeDistance = 1f;

        private Vector3 _velocity;
        private Vector3 _desiredVelocity;
        private Rigidbody _rigidbody;
        private bool _desiredJump;
        private int _groundContactCount;
        private Vector3 _contactNormal;
        private int _jumpPhase;
        private float _minGroundDotProduct;
        private int _stepsSinceLastGrounded;
        private int _stepsSinceLastJump;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private bool OnGround => _groundContactCount > 0;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            OnValidate();
        }

        private void Update()
        {
            var playerInput = Vector2.zero;
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

            GetComponent<Renderer>().material.SetColor(BaseColor, OnGround ? Color.black : Color.white);
        }

        private void FixedUpdate()
        {
            UpdateState();
            AdjustVelocity();

            if (_desiredJump)
            {
                _desiredJump = false;
                Jump();
            }

            _rigidbody.velocity = _velocity;

            ClearState();
        }


        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        protected void OnCollisionEnter(Collision other)
        {
            EvaluateCollision(other);
        }

        protected void OnCollisionStay(Collision other)
        {
            EvaluateCollision(other);
        }

        private void UpdateState()
        {
            _stepsSinceLastGrounded++;
            _stepsSinceLastJump++;
            _velocity = _rigidbody.velocity;

            if (OnGround || SnapToGround())
            {
                _stepsSinceLastGrounded = 0;
                _jumpPhase = 0;
                _contactNormal.Normalize();
            }
            else
            {
                _contactNormal = Vector3.up;
            }
        }

        private bool SnapToGround()
        {
            if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2)
            {
                return false;
            }

            var speed = _velocity.magnitude;

            if (speed > maxSnappingSpeed)
            {
                return false;
            }

            if (!Physics.Raycast(transform.position, -transform.up, out var hit, probeDistance))
            {
                return false;
            }

            if (hit.normal.y < _minGroundDotProduct)
            {
                return false;
            }

            _groundContactCount = 1;
            _contactNormal = hit.normal;

            var dot = Vector3.Dot(_contactNormal, _velocity);
            if (dot > 0f)
            {
                _velocity = (_velocity - dot * _contactNormal).normalized * speed;
            }

            return true;
        }

        private void AdjustVelocity()
        {
            var acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;

            var xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            var zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            var currentVelocityX = Vector3.Dot(_velocity, xAxis);
            var currentVelocityZ = Vector3.Dot(_velocity, zAxis);

            var newVelocityX = Mathf.MoveTowards(currentVelocityX, _desiredVelocity.x, maxSpeedChange);
            var newVelocityZ = Mathf.MoveTowards(currentVelocityZ, _desiredVelocity.z, maxSpeedChange);

            _velocity += xAxis * (newVelocityX - currentVelocityX) + zAxis * (newVelocityZ - currentVelocityZ);
        }

        private void ClearState()
        {
            _groundContactCount = 0;
            _contactNormal = Vector3.zero;
        }

        private void Jump()
        {
            if (OnGround || _jumpPhase < maxAirJumps)
            {
                _jumpPhase++;
                _stepsSinceLastJump = 0;

                var jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
                var alignedSpeed = Vector3.Dot(_velocity, _contactNormal);
                if (alignedSpeed > 0f)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
                }

                _velocity += _contactNormal * jumpSpeed;
            }
        }

        private void EvaluateCollision(Collision collision)
        {
            for (var i = 0; i < collision.contactCount; i++)
            {
                var normal = collision.GetContact(i).normal;
                if (!(normal.y >= _minGroundDotProduct)) continue;
                _groundContactCount++;
                _contactNormal += normal;
            }
        }

        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            return vector - Vector3.Dot(vector, _contactNormal) * _contactNormal;
        }
    }
}