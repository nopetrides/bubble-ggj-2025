using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorLifeCounter : MonoBehaviour
    {
        [SerializeField] private float AnimationTime = 0.2f;
        [SerializeField] private GameObject LifeObjTemplate;
        [SerializeField] private Color LostColor, LifeGainedColor;
        [SerializeField] private List<GameObject> Lives = new ();
        [SerializeField] private Transform LivesParent;

        [SerializeField] private CanvasGroup LastLifeWarningOverlay;

        private int _maxLivesPossible = 3;
        private int _livesRemaining;
        private int _currentLifeIndex;
        
        public int LivesRemaining
        {
            get => _livesRemaining;
            set
            {
                var previousLives = _livesRemaining;
                _livesRemaining = Mathf.Clamp(value, 0, _maxLivesPossible);
                if (_livesRemaining < previousLives && _livesRemaining >= 0)
                {
                    ProcessLifeLost();
                }
                else if (_livesRemaining > previousLives && _livesRemaining <= _maxLivesPossible)
                {
                    ProcessLifeGained();
                }
            }
        }

        public void Initialize(int maxLivesPossible)
        {
            _maxLivesPossible = maxLivesPossible;
            _livesRemaining = _maxLivesPossible;
            foreach (Transform child in LivesParent)
            {
                child.gameObject.SetActive(false);
            }
            for(var i = 0; i < _livesRemaining; ++i)
            {
                var lifeObj = Instantiate(LifeObjTemplate, LivesParent);
                Lives.Add(lifeObj);
                lifeObj.SetActive(true);
            }
            _currentLifeIndex = Lives.Count - 1;
        }

        private void ProcessLifeLost()
        {
            var lifeLost = Lives[_currentLifeIndex--];
            StartCoroutine(LifeLostAnimation(lifeLost));
        }
        
        private void ProcessLifeGained()
        {
            var lifeGained = Lives[++_currentLifeIndex];
            StartCoroutine(LifeGainedAnimation(lifeGained));
        }

        private IEnumerator LifeLostAnimation(GameObject lifeObj)
        {
            var graphic = lifeObj.GetComponent<Graphic>();
            if (_livesRemaining == 1)
            {
                LastLifeWarningOverlay.DOFade(1f, AnimationTime);
            }
            if (graphic != null)
            {
                var colorTween = graphic.DOColor(LostColor, AnimationTime).SetEase(Ease.InBack);
                yield return colorTween.WaitForCompletion();
            }
            else
            {
                lifeObj.transform.DOScale(Vector3.zero, AnimationTime).SetEase(Ease.InBack);
            }

        }
        
        private IEnumerator LifeGainedAnimation(GameObject lifeObj)
        {
            var graphic = lifeObj.GetComponent<Graphic>();
            if (graphic != null)
            {
                var colorTween = graphic.DOColor(LifeGainedColor, AnimationTime).SetEase(Ease.OutBack);
                yield return colorTween.WaitForCompletion();
            }
            else
            {
                lifeObj.transform.DOScale(Vector3.one, AnimationTime).SetEase(Ease.OutBack);
            }
            
            LastLifeWarningOverlay.DOFade(0f, AnimationTime);
        }
    }
}
