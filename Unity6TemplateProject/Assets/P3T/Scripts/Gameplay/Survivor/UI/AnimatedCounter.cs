using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    /// Class for creating a animating counting graphic with particles.
    /// Exposes overridable values for use with inherited variations
    /// </summary>
    public class AnimatedCounter : MonoBehaviour
    {
        protected const float TravelTime = 0.5f;

        [SerializeField] protected Graphic Particle;
        [SerializeField] protected TMP_Text Counter;
        [SerializeField] protected int NumberOfAnimatedSprites = 10;
    
        protected int StartValue;
        protected int CurrentValue;
        public int EndValue { get; private set; }
        private Coroutine _animationRoutine;
    
        protected Transform AnimationRoot;
        protected Action Callback;
    
        private List<Graphic> _animatedSprites;
        protected bool IsAnimating;
        protected readonly List<Tween> ActiveTweens = new();

        public int CounterValue
        {
            get => CurrentValue;
            set
            {
                CurrentValue = value;
                UpdateCounter(CurrentValue);
            }
        }
    
        protected virtual void Awake()
        {
            AnimationRoot = GetComponentInChildren<Canvas>()?.transform;
        }

        /// <summary>
        /// Public method to set text without animation
        /// </summary>
        /// <param name="countToSet"></param>
        public void SetCountTextImmediate(int countToSet)
        {
            EndValue = countToSet;
            CounterValue = countToSet;
        }

        protected virtual void UpdateCounter(int newValue)
        {
            Counter.text = NumUtil.FormatScoreForDisplay(newValue);
        }

        public void Animate(int delta, Vector3? origin = null, Action completed = null)
        {
            Callback = completed;
            Animate(CurrentValue, delta, origin);
        }

        protected void Animate(int startValue, int delta, Vector3? origin, bool includeCounter = true)
        {
            IsAnimating = true;
        
            if (gameObject.activeInHierarchy == false || delta == 0 || (AnimationRoot == null && includeCounter == false))
            {
                AnimateComplete();
                return;
            }

            var counterDuration = includeCounter ? 1.2f : 0f;

            if (AnimationRoot == null)
            {
                if(_animationRoutine != null)
                    StopCoroutine(_animationRoutine);

                _animationRoutine = StartCoroutine(AnimationRoutine(delta, counterDuration));
                return;
            }
		
            origin ??= Vector3.zero;
		
            StartValue = startValue;
		
            if (_animatedSprites == null)
            {
                _animatedSprites = new List<Graphic>();
                for (var i = 0; i < NumberOfAnimatedSprites; i++)
                {
                    var sp = Instantiate(Particle, AnimationRoot);
                    sp.gameObject.SetActive(false);
                    _animatedSprites.Add(sp);
                }
            }
		
            // reset alpha to 0
            var delay = 0f;
            var delayInc = TravelTime / Math.Min(delta, 4);
            var c = 1;

            if (includeCounter)
            {
                _animationRoutine = StartCoroutine(AnimationRooted(delta, counterDuration));
            }

            foreach (var sp in _animatedSprites)
            {
                if (c++ > delta) break;

                var spColor = sp.color;
                spColor.a = 0f;
                sp.color = spColor;
                sp.gameObject.SetActive(true);

                var t = sp.transform;
			
                t.position = origin.Value;

                ActiveTweens.Add(DOTween.To(() => sp.color.a, value =>
                {
                    spColor = sp.color;
                    spColor.a = value;
                    sp.color = spColor;
                }, 1f, TravelTime / 2.0f).SetDelay(delay));
			
                var pos1 = sp.transform.localPosition + 
                           new Vector3((Random.value - 0.5f), (Random.value - 0.5f)) * 200f;
			
                ActiveTweens.Add(t.DOLocalMove(pos1, TravelTime)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        ActiveTweens.Add(t.DOMove(AnimationRoot.position, TravelTime)
                            .SetEase(Ease.InSine));
                    }));
			
                t.localScale = Vector3.one * 3f;
                ActiveTweens.Add(t.DOScale(1f, TravelTime)
                    .SetEase(Ease.InCubic)
                    .SetDelay(delay + TravelTime));

                ActiveTweens.Add(DOTween.To(() => sp.color.a, value =>
                    {
                        spColor = sp.color;
                        spColor.a = value;
                        sp.color = spColor;
                    }, 0f, 0.1f).SetDelay(delay + TravelTime * 2)
                    .OnComplete(() => { sp.gameObject.SetActive(false); }));

                delay += delayInc * Random.value;
            }
        
            StartCoroutine(AnimationCompleteAfterDelay(Mathf.Max(delay, counterDuration) + TravelTime * 2 + delayInc));
        }

        private IEnumerator AnimationRoutine(int delta, float counterDuration)
        {
            yield return AnimateCounter(delta, counterDuration);
            AnimateComplete();
        }

        private IEnumerator AnimationRooted(int delta, float counterDuration)
        {
            yield return new WaitForSeconds(TravelTime * 2);
            yield return AnimateCounter(delta, counterDuration);
            AnimateComplete();
        }

        private IEnumerator AnimationCompleteAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AnimateComplete();
        }

        protected void AnimateComplete()
        {
            Callback?.Invoke();
            Callback = null;

            foreach (var tween in ActiveTweens.Where(tween => tween.active))
            {
                tween.Kill();
            }
            ActiveTweens.Clear();
        
            IsAnimating = false;
        }
	
        protected virtual IEnumerator AnimateCounter(int delta, float duration)
        {
            StartValue = CurrentValue;
            EndValue += delta;
       
            // Debug.Log($"Animating counter from {_startValue} to {toValue}");
            for (var t = 0.0f; t < duration && IsAnimating; t += Time.deltaTime * 1.0f)
            {
                var tmpVal = Mathf.Lerp(StartValue, EndValue, Mathf.Min(t/duration, 1f));
            
                CounterValue = StartValue > EndValue ? Mathf.FloorToInt(tmpVal) : Mathf.CeilToInt(tmpVal);
            
                UpdateCounter(CounterValue);
                yield return null;
            }
		
            CounterValue = EndValue;
        }

        public virtual void Complete()
        {
            if(_animationRoutine != null)
                StopCoroutine(_animationRoutine);
            CounterValue = EndValue;
            AnimateComplete();
            if (_animatedSprites == null) return;
            foreach (var sprite in _animatedSprites)
            {
                sprite.DOKill();
                sprite.gameObject.SetActive(false);
            }
        }
    }
}