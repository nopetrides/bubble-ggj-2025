using UnityEngine;
using UnityEngine.InputSystem;

namespace P3T.Scripts.Gameplay
{
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private float Speed = 1f;
		[SerializeField] private Rigidbody Rb;
		[SerializeField] private Collider PrimaryCollider;

		private Vector2 _moveInput = Vector2.zero;
		private Vector2 _lookInput = Vector2.zero;

		public void OnDeviceLost()
		{
			
		}

		public void OnDeviceRegained()
		{
			
		}

		public void OnControlsChanged()
		{
			
		}

		public void OnMove(InputValue value)
		{
			_moveInput = value.Get<Vector2>();
		}

		public void OnLook(InputValue value)
		{
			_lookInput = value.Get<Vector2>();
		}

		public void OnAttack()
		{
			
		}
		
		public void OnInteract()
		{
			
		}

		public void OnJump()
		{
			
		}


		public void FixedUpdate()
		{
			// todo make based on facing direction
			Rb.AddForce(new Vector3(_moveInput.x,1,_moveInput.y) * (Time.fixedDeltaTime * Speed), ForceMode.VelocityChange);
			_moveInput = Vector2.zero;
			//Rb.MoveRotation();
		}
	}
}
