using DG.Tweening;
using P3T.Scripts.Util;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor.Drivers
{
	public class SurvivorAnimatedAsset : MonoBehaviour
	{
		private static readonly int Velocity = Animator.StringToHash("Velocity");
		private static readonly int Dead = Animator.StringToHash("Dead");

		[SerializeField] private Renderer RendererParent;
		[SerializeField] private Transform VisualsParent; // may not be the same as the renderer

		// Addressable assets have colliders on them but not the rigidbody.
		// We need to wait to get rigidbody to get attached to the collider when the object is instantiated
		[SerializeField] private Collider ObjectCollider;
		[SerializeField] private Animator MovementAnimator;

		[SerializeField] private TrailLocator TrailLocator;
		[SerializeField] private SquashAndStretchVelocity SquashAndStretch;

		public Renderer PrimaryRenderer => RendererParent;
		public Transform Visuals => VisualsParent;
		public Transform TrailParent => TrailLocator.TrailParent;
		private Rigidbody _drivingRigidBody;

		private void Start()
		{
			_drivingRigidBody = ObjectCollider.attachedRigidbody;
			if (MovementAnimator != null) MovementAnimator.SetBool(Dead, false);
			if (SquashAndStretch != null) SquashAndStretch.SetRigidBody(_drivingRigidBody);
			Vector3 originalScale = VisualsParent.localScale;
			VisualsParent.localScale = Vector3.zero;
			VisualsParent.DOScale(originalScale, 1f);
		}

		private void FixedUpdate()
		{
			// Seems the rigidbody needs a frame or two to connect
			if (_drivingRigidBody == null) return;
			// If it has a animator, set the velocity on it so it can animate appropriately
			if (MovementAnimator != null) MovementAnimator.SetFloat(Velocity, _drivingRigidBody.linearVelocity.magnitude);
		}

		public void OnKilled()
		{
			if (MovementAnimator != null) MovementAnimator.SetBool(Dead, true);
		}

		public void OnDisable()
		{
			VisualsParent.DOKill(true);
		}
	}
}