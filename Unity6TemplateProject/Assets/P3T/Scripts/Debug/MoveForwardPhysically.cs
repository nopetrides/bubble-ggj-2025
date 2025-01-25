using UnityEngine;
using UnityEngine.Animations;

namespace P3T.Scripts.Debug
{
    public class MoveForwardPhysically : MonoBehaviour
    {
        [SerializeField] private float Speed = 1f;
        [SerializeField] private Rigidbody Rb;
        [SerializeField] private LookAtConstraint LookAt;

        private GameObject _player;

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (PlayerMgr.Instance == null)
                return;
            if (_player == null)
            {
                _player = PlayerMgr.Instance.PlayerObject;
                LookAt.AddSource(new ConstraintSource { sourceTransform = _player.transform, weight = 1});
            }

            Rb.AddRelativeForce(Vector3.forward * (Speed * Time.fixedDeltaTime), ForceMode.VelocityChange);
        }
    }
}
