using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	/// <summary>
	///     Handle teleporting object to the opposite side of the screen.
	///     We could do this with weird camera calculations, but since we are already using physics, this seems appropriate.
	/// </summary>
	public class PlayerBounds : MonoBehaviour
	{
		/// <summary>
		///     Used with <see cref="WrapDirection" />
		/// </summary>
		[SerializeField] private Collider2D PlayerMovementColliders;

		/// <summary>
		///     Spawned objects should be fully offscreen, and not touching the edge of the screen.
		///		todo, make based off actual object size
		/// </summary>
		private float GameObjectSize = 100f;

		public float SpawnOffset => GameObjectSize / 2f;

		public Collider2D PlayerMovementBounds => PlayerMovementColliders;
	}

	/// <summary>
	///     The set of colliders for performing wrapping
	/// </summary>
	public enum WrapDirection
	{
		Left = 0,
		Right,
		Top,
		Bottom
	}
}