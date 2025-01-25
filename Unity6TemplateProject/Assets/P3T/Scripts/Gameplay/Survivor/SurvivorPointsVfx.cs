using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    /// Points Vfx object for use in car racing
    /// Does not animate in the default way
    /// </summary>
    public class SurvivorPointsVfx : DefaultPointsVfx
    {
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _bonusColor;
        [SerializeField] private Image _outline;
        [SerializeField] private float _animationDistance = 100;
    
        private Vector2 _worldPosition;
        private Camera _gameCamera;
    
        /// <summary>
        /// Specify if this is for bonus points. Affects coloring.
        /// </summary>
        /// <param name="isBonus"></param>
        /// <returns>the CarRacingPointsVfx instance</returns>
        public SurvivorPointsVfx SetIsBonus(bool isBonus)
        {
            SetColor(isBonus ? _bonusColor : _defaultColor);
            return this;
        }

        /// <summary>
        /// Use during setup. Sets the world position that the bubble should be anchored to
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="gameCamera"></param>
        /// <returns>the CarRacingPointsVfx instance</returns>
        public SurvivorPointsVfx SetWorldPosition(Vector3 worldPosition, Camera gameCamera)
        {
            _worldPosition = worldPosition;
            _gameCamera = gameCamera;
            return this;
        }
    
        public override DefaultPointsVfx SetColor(Color color)
        {
            base.SetColor(color);

            _outline.color = color;
        
            return this;
        }

        /// <summary>
        /// See <see cref="DefaultPointsVfx.Animate"></see>
        /// </summary>
        public override Sequence Animate()
        {
            CanvasGroup.transform.localPosition = Vector3.one;
            CanvasGroup.transform.localScale = Vector3.zero;
            CanvasGroup.alpha = 1;
        
            var sequence = DOTween.Sequence();
            sequence.Append(CanvasGroup.DOFade(1, .25f * AnimationDuration));
            sequence.Join(CanvasGroup.transform.DOScale(1, .25f * AnimationDuration));
            sequence.Join(CanvasGroup.transform.DOLocalMoveY(_animationDistance, .5f * AnimationDuration));
            sequence.AppendInterval(.25f * AnimationDuration);
            sequence.Append(CanvasGroup.DOFade(0, .25f * AnimationDuration));
            sequence.OnComplete(VfxComplete);

            return sequence;
        }
    
        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_gameCamera == null) return;
            transform.position =  _gameCamera.WorldToScreenPoint(_worldPosition);
        }
    }
}
