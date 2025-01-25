using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

/// <summary>
/// Base class for menus
/// Allows for transitioning as menu is revealed or hidden
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Canvas))]
public class MenuBase : MonoBehaviour
{
    public virtual GameMenus MenuType() => GameMenus.None;
    
    private Canvas _canvas;
    
    public int SortOrder
    {
        get => _canvas.sortingOrder;
        set => _canvas.sortingOrder = value;
    }
    
    private CanvasGroup _canvasGroup;
    
    public void OnInstantiate()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.overrideSorting = true;
        _canvasGroup = GetComponent<CanvasGroup>();
        HideFader();
    }

    TweenerCore<float, float, FloatOptions> _activeTween;
    private bool IsTweening => _activeTween is {active: true};
    
    private void RevealFader()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.gameObject.SetActive(true);
    }

    private void HideFader()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.gameObject.SetActive(false);
    }

    public void PerformFullFadeIn(float duration, Action onFadeInComplete = null)
    {
        if (IsTweening)
            _activeTween.Kill();
        else
            RevealFader();

        if (_canvasGroup.isActiveAndEnabled && Mathf.Approximately(_canvasGroup.alpha, 1f))
            onFadeInComplete?.Invoke();
        else
            _activeTween = _canvasGroup.DOFade(1f, duration).OnComplete(() => onFadeInComplete?.Invoke());
    }

    public void PerformHalfFadeIn(float duration, Action onFadeInComplete = null)
    {
        if (IsTweening)
            _activeTween.Kill();
        else
            RevealFader();

        if (_canvasGroup.isActiveAndEnabled && Mathf.Approximately(_canvasGroup.alpha, 0.5f))
            onFadeInComplete?.Invoke();
        else
            _activeTween = _canvasGroup.DOFade(0.5f, duration).SetUpdate(true)
                .OnComplete(() => onFadeInComplete?.Invoke());
    }

    public void PerformFullFadeOut(float duration, Action onFadeOutComplete = null)
    {
        if (IsTweening)
            _activeTween.Kill();

        if (!_canvasGroup.isActiveAndEnabled && Mathf.Approximately(_canvasGroup.alpha, 0f))
            onFadeOutComplete?.Invoke();
        else
        {
            _activeTween = _canvasGroup.DOFade(0f, duration).SetUpdate(true).OnComplete(
                () =>
                {
                    HideFader();
                    onFadeOutComplete?.Invoke();
                });
        }
    }
}
