using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    /// Manages pools of points related objects
    /// </summary>
    public class SurvivorPointsDisplayManager : MonoBehaviour
    {
        /// <summary>
        /// The view where the points score is shown to the user, generically
        /// </summary>
        private Transform _view;
        /// <summary>
        /// A reference to a prefab with <see cref="DefaultPointsVfx"/> to be used when no other template is given
        /// </summary>
        [SerializeField] private DefaultPointsVfx DefaultVfx;
        /// <summary>
        /// A reference to a prefab with <see cref="LabeledPointsVfx"/> to be used when a game wants a points object with an addition label.
        /// </summary>
        [FormerlySerializedAs("_labeledVfx")] 
        [SerializeField] private LabeledPointsVfx DefaultLabeledVfx;
        /// <summary>
        /// Name of the prefab for the default labeled vfx
        /// </summary>
        public string LabeledVfxKey => DefaultLabeledVfx.name;

        /// <summary>
        /// Set of all the object pools used for points
        /// Keyed with the prefab name so we can separate out different prefabs using the same script
        /// </summary>
        private readonly Dictionary<string, IObjectPool<DefaultPointsVfx>> _pointsObjectPools = new();

        private bool _poolIsPrewarmed;

        /// <summary>
        /// Set the view from the game controller
        /// </summary>
        /// <param name="view"></param>
        public void Setup(Transform view)
        {
            _view = view;
        
            if (_poolIsPrewarmed) return;
            AddTemplate(DefaultVfx, 3);
            AddTemplate(DefaultLabeledVfx, 1);
            _poolIsPrewarmed = true;
        }

        /// <summary>
        /// Add a new prefab to the vfx object pool
        /// </summary>
        /// <param name="vfxObject">source prefab to use as a template</param>
        /// <param name="initialSize">number of items to pre-spawn in the pool</param>
        public void AddTemplate(DefaultPointsVfx vfxObject, int initialSize)
        {
            if (_pointsObjectPools.ContainsKey(vfxObject.name))
            {
                return;
            }
        
            var pool = new ObjectPool<DefaultPointsVfx>
            (
                () =>
                {
                    var pooledObject = Instantiate(vfxObject)
                        .SetManager(this)
                        .SetParent(_view)
                        .SetStartPositionLocalSpace(new Vector2(0, 149));
                    pooledObject.transform.localScale = Vector2.one;
                    pooledObject.name = vfxObject.name;
                    return pooledObject;
                },
                visualContent => { visualContent.gameObject.SetActive(true); }, // maybe we don't auto enable?
                visualContent =>
                {
                    visualContent.gameObject.SetActive(false);
                    // clean the object back up so anyone who messed with it will have it reset for next time
                    visualContent
                        .SetManager(this)
                        .SetParent(_view)
                        .SetStartPositionLocalSpace(new Vector2(0, 149))
                        .SetAnimationDuration(vfxObject.MaxDuration);
                    visualContent.transform.localScale = Vector2.one;
                    visualContent.name = vfxObject.name;
                },
                visualContent => Destroy(visualContent.gameObject)
            );
        
            _pointsObjectPools[vfxObject.name] = pool;

            var prespawnedObjects = new DefaultPointsVfx[initialSize];
            for (int i = 0; i < initialSize; i++)
            {
                prespawnedObjects[i] = pool.Get();
            }
            foreach(var vfx in prespawnedObjects) pool.Release(vfx);
        }

        /// <summary>
        /// Returns the first available object of that template type
        /// </summary>
        /// <returns></returns>
        public DefaultPointsVfx GetPooledObject(DefaultPointsVfx template = null)
        {
            // default template if none given
            if (template == null) return _pointsObjectPools[DefaultVfx.name].Get();
        
            // if there are not object available in this pool, expand the pool
            if (_pointsObjectPools.ContainsKey(template.name) == false) AddTemplate(template, 0);
        
            // takes the object out of the list of usable objects of that pool
            var pointsObject = _pointsObjectPools[template.name].Get();
        
            return pointsObject;
        }

        /// <summary>
        /// Get a pooled object by its name.
        /// Requires that the object was added to the object pools first
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public DefaultPointsVfx GetPooledObject(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) return _pointsObjectPools[DefaultVfx.name].Get();
        
            // if there are not object available in this pool, expand the pool
            if (_pointsObjectPools.ContainsKey(prefabName) == false)
            {
                UnityEngine.Debug.LogError("No points object added to pool with name " + prefabName);
                return null;
            }
        
            // takes the object out of the list of usable objects of that pool
            var pointsObject = _pointsObjectPools[prefabName].Get();
        
            return pointsObject;
        }

        /// <summary>
        /// Release the object back to the pool
        /// </summary>
        /// <param name="prefab"></param>
        public void VfxAnimationComplete(DefaultPointsVfx prefab)
        {
            if (_pointsObjectPools.ContainsKey(prefab.name) == false) return;
        
            _pointsObjectPools[prefab.name].Release(prefab);
        }
    }
}
