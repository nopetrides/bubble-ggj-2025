using UnityEngine;
using UnityEngine.Pool;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorBulletManager : MonoBehaviour
    {
        public enum BulletModifiers
        {
            None = 0,
            Pierce = 1
        }

        [SerializeField] private Transform BulletParent;

        [SerializeField] private SurvivorBullet DefaultArcadeSurvivorBulletPrefab;

        [SerializeField] private float BulletImpulse = 1000f;

        private SurvivorBullet _arcadeSurvivorBulletToSpawn;
        private SurvivorController _controller;
        private ObjectPool<SurvivorBullet> _pool;
        private Collider2D _gameBounds;

        private void ReadyPool(GameObject firingCharacter)
        {
            _arcadeSurvivorBulletToSpawn = firingCharacter.GetComponentInChildren<SurvivorBullet>(true) ?? DefaultArcadeSurvivorBulletPrefab;
            _pool = new ObjectPool<SurvivorBullet>
            (
                () =>
                {
                    var pooledObject = Instantiate(_arcadeSurvivorBulletToSpawn)
                        .SetManager(this)
                        .SetParent(BulletParent)
                        .SetImpulse(BulletImpulse)
                        .SetBoundsCollider(_gameBounds);

                    pooledObject.transform.localScale = Vector3.one;
                    pooledObject.name = _arcadeSurvivorBulletToSpawn.name + _pool.CountAll;

                    return pooledObject;
                },
                pooledObject =>
                {
                    pooledObject.gameObject.SetActive(true);
                    // Has to be called when object is set active, resets on disable

                    pooledObject.SetImpulse(BulletImpulse);
                },
                pooledObject =>
                {
                    pooledObject.gameObject.SetActive(false);
                    // clean the object back up so anyone who messed with it will have it reset for next time
                    pooledObject.transform.localScale = Vector3.one;
                },
                pooledObject => Destroy(pooledObject.gameObject)
            );
        }

        public void Setup(SurvivorController gameController, GameObject hero, Collider2D gameBounds)
        {
            _controller = gameController;
            _gameBounds = gameBounds;
            ReadyPool(hero);
        }

        public void FireBullet(Vector3 originPosition, Vector3 forwardVector,
            BulletModifiers modifiers, Rigidbody target = null)
        {
            var nextBullet = _pool.Get();
            var bulletTransform = nextBullet.transform;
            var pos = originPosition;
            bulletTransform.position = pos;
            bulletTransform.forward = forwardVector;

            nextBullet.SetModifiers(modifiers)
                .SetTarget(target);
            nextBullet.Fire();
        }

        public void Release(SurvivorBullet arcadeSurvivorBullet)
        {
            _pool.Release(arcadeSurvivorBullet);
        }

        public bool LookupHazard(Rigidbody rb)
        {
            return _controller.DoesRigidbodyBelongToHazard(rb);
        }

        public void DamageHazard(Rigidbody rb, Collider damagingCollider)
        {
            _controller.DamageHazard(rb, damagingCollider);
        }
    }
}