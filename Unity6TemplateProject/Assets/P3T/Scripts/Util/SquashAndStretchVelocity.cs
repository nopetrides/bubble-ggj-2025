using System;
using UnityEngine;

namespace P3T.Scripts.Util
{
    public class SquashAndStretchVelocity : MonoBehaviour
    {
        [SerializeField] private Rigidbody Rb;
    
        [SerializeField] private float StretchFactor = 0.5f;
        [SerializeField] private float SmoothingSpeed = 5f;

        private Vector3 _originalScale;

        private void Start()
        {
            _originalScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            if (Rb == null)
                return;
            
            // Calculate the velocity direction and magnitude
            Vector3 velocity = Rb.linearVelocity;
            float speed = velocity.magnitude;

            if (speed > 0.01f) // Avoid adjusting when stationary
            {
                Vector3 localVelocity = transform.InverseTransformDirection(velocity.normalized);

                // Calculate stretch for the x and z axes
                float stretchZ = _originalScale.x + Mathf.Abs(localVelocity.z) * StretchFactor * speed;
                float stretchX = _originalScale.z * (_originalScale.z / stretchZ); // Inverse to maintain volume

                // Apply the calculated stretch
                Vector3 stretchScale = new Vector3(
                    stretchX,
                    _originalScale.y, // Keep the y scale constant
                    stretchZ
                );

                // Smoothly transition to the new scale
                transform.localScale = Vector3.Lerp(transform.localScale, stretchScale, Time.fixedDeltaTime * SmoothingSpeed);
            }
            else
            {
                // Smoothly return to the original scale when stationary
                transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.fixedDeltaTime * SmoothingSpeed);
            }
        }
    }
}
