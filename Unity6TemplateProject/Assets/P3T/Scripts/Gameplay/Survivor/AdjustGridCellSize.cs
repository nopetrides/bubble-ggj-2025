using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    public abstract class AdjustGridCellSize : MonoBehaviour
    {
        public enum Axis { X, Y };

        [SerializeField] private Axis Expand;

        private RectTransform _transform;

        protected abstract bool HasGridReference { get; }
        protected abstract int GridConstraintCount { get; }
        protected abstract Vector2 GridSpacing { get; set; }
        protected  abstract  Vector2 GridCellSize { get; set; }
        protected abstract RectOffset GridPadding { get; set; }

        private bool _hasCachedStartValues;
        private int _gridConstraintCount;
        private Vector2 _gridSpacing;
        private Vector2 _gridCellSize;
        private RectOffset _gridPadding;
    
        protected virtual void Awake()
        {
            _transform = (RectTransform)transform;
            UpdateCellSize();
        }
 
        // Start is called before the first frame update
        private void Start()
        {
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }

        private void CacheStartValues()
        {
            GetGridReference();
            if (HasGridReference == false) return;
        
            _transform = (RectTransform)transform;
        
            _gridConstraintCount = GridConstraintCount;
            _gridSpacing = GridSpacing;
            _gridCellSize = GridCellSize;
            _gridPadding = new RectOffset(GridPadding.left, GridPadding.right, GridPadding.top, GridPadding.bottom);
            _hasCachedStartValues = true;
        }

        protected abstract void GetGridReference();
    
        [ContextMenu("Refresh")]
        private void UpdateCellSize()
        {
            if(_hasCachedStartValues == false || Application.isPlaying == false) CacheStartValues();
        
            if (HasGridReference == false || enabled == false) return;

            var count = _gridConstraintCount;
            float multiplier;
        
            if (Expand == Axis.X)
            {
                var totalDesiredSize = _gridPadding.left 
                                       + _gridPadding.right 
                                       + _gridSpacing.x * Mathf.Max(0, count - 1) 
                                       + _gridCellSize.x * count;
            
                multiplier = _transform.rect.width / totalDesiredSize;
            }
            else
            {
                var totalDesiredSize = _gridPadding.top 
                                       + _gridPadding.bottom 
                                       + _gridSpacing.y * Mathf.Max(0, count - 1) 
                                       + _gridCellSize.y * count;
            
                multiplier = _transform.rect.height / totalDesiredSize;
            }
        
            //apply multiplier to all reference/starting values
            GridSpacing = _gridSpacing * multiplier;
            GridPadding.bottom = (Mathf.FloorToInt(_gridPadding.bottom * multiplier));
            GridPadding.top = (Mathf.FloorToInt(_gridPadding.top * multiplier));
            GridPadding.left = (Mathf.FloorToInt(_gridPadding.left * multiplier));
            GridPadding.right = (Mathf.FloorToInt(_gridPadding.right * multiplier));
            GridCellSize = _gridCellSize * multiplier;
        }
    }
}