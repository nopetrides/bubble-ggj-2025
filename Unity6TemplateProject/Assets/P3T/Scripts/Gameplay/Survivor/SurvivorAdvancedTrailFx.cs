using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorAdvancedTrailFx : MonoBehaviour
	{
		[SerializeField] private TrailRenderer[] TrailRenderers;
		[SerializeField] private ParticleSystem[] ParticleSystems;

		private Rigidbody2D _rigidbody2D;

		private void Start()
		{
			// Get the rigidbody that the fx is attached to
			_rigidbody2D = gameObject.GetComponentInParent<Rigidbody2D>();
			var t = transform as RectTransform;
			if (t != null) SetTrailStartWidth(t.rect.height); // height since the survivor style assets stay horizontal
			else UnityEngine.Debug.LogError("Failed to set trail start width. Does the fx object have a RectTransform?");
		}

		private void OnEnable()
		{
			foreach (var tr in TrailRenderers) tr.emitting = true;
			foreach (var ps in ParticleSystems)
			{
				var emission = ps.emission;
				emission.enabled = true;
			}
		}

		private void OnDisable()
		{
			foreach (var tr in TrailRenderers) tr.emitting = false;
			foreach (var ps in ParticleSystems)
			{
				var emission = ps.emission;
				emission.enabled = false;
			}
		}
        
		// set trail start width
		private void SetTrailStartWidth(float width)
		{
			foreach (var trailRenderer in TrailRenderers) trailRenderer.startWidth = width;
		}
	}
}