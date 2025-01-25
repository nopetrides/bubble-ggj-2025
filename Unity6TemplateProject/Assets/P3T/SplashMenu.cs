using System;
using UnityEngine;

public class SplashMenu : MenuBase
{
    [SerializeField] private Animator Animator;
    
    private Action _onAnimationComplete;
    
    public void OnShow(Action onAnimationComplete)
    {
        _onAnimationComplete = onAnimationComplete;
        Animator.Play(0);
    }

    /// <summary>
    /// Called by the animation event on the splash animation controller
    /// </summary>
    public void AnimationComplete()
    {
        _onAnimationComplete?.Invoke();
    }
}
