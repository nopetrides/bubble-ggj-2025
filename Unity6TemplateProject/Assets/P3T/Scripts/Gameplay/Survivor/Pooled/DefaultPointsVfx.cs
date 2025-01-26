using DG.Tweening;
using TMPro;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
   /// <summary>
   /// Points Vfx to be used with a points 'bubble' prefab.
   /// </summary>
   public class DefaultPointsVfx : MonoBehaviour
   {
      [Tooltip("The actual points value to display that their score increased by.")]
      [SerializeField] protected TMP_Text ScoreEarnedText;
        
      [Tooltip("The canvas which we need to reference the alpha transparency value")]
      [SerializeField] protected CanvasGroup CanvasGroup;

      [Tooltip("CanvasGroup assigned to the particle burst. Used for animation")]
      [SerializeField] protected CanvasGroup ParticleCanvasGroup;

      [Tooltip("Particle Burst that plays at the beginning of the animation")]
      [SerializeField] protected ParticlesBurst ParticleBurst;
   
      [Tooltip("How long the total animation animates for. Automatically cleaned up after")]
      [SerializeField] protected float AnimationDuration = 1.83f;
      /// <summary>
      /// Public accessor to fetch the animation time.
      /// True animation time and duration should be checked by <see cref="Animate"/> return value;
      /// </summary>
      public float MaxDuration => AnimationDuration;

      /// <summary>
      /// Amount of points this vfx will display
      /// </summary>
      protected int Points = 0;
      /// <summary>
      /// Position to begin the animation from
      /// </summary>
      protected Vector2 LocalSpaceStartLocation = new Vector2(0, 1200);
      /// <summary>
      /// How much the vfx object should move in Local Space
      /// </summary>
      protected Vector2 MoveDistance = new Vector2(0, 90);
      /// <summary>
      /// The points manager that manages the object pool this belongs to
      /// </summary>
      private SurvivorPointsDisplayManager _pointsManager;
      /// <summary>
      /// Sets <inheritdoc cref="_pointsManager"/>
      /// </summary>
      /// <param name="manager"></param>
      /// <returns></returns>
      public DefaultPointsVfx SetManager(SurvivorPointsDisplayManager manager)
      {
         _pointsManager = manager;
         return this;
      }
      /// <summary>
      /// Sets the parent of the game object so it will be on the right canvas
      /// </summary>
      /// <param name="parentTransform"></param>
      /// <returns></returns>
      /// <remarks>Most implementations will not need this set</remarks>
      public DefaultPointsVfx SetParent(Transform parentTransform)
      {
         transform.SetParent(parentTransform);
         return this;
      }
   
      /// <summary>
      /// Public setter for duration
      /// </summary>
      /// <param name="duration"></param>
      /// <returns>self</returns>
      public DefaultPointsVfx SetAnimationDuration(float duration)
      {
         AnimationDuration = duration;
         return this;
      }
   
      /// <summary>
      /// Set the number of points this object displays
      /// </summary>
      /// <param name="points"></param>
      /// <returns></returns>
      public virtual DefaultPointsVfx SetPointsValue(int points)
      {
         Points = points;
         ScoreEarnedText.text = $"{points:D}";
         return this;
      }

      /// <summary>
      /// Sets the <see cref="LocalSpaceStartLocation"/>
      /// </summary>
      /// <param name="startPos"></param>
      public DefaultPointsVfx SetStartPositionLocalSpace(Vector2 startPos)
      {
         LocalSpaceStartLocation = startPos;
         return this;
      }

      /// <summary>
      /// Sets the <see cref="LocalSpaceStartLocation"/> with the provided world position converted into local space
      /// </summary>
      /// <param name="startPos"></param>
      public DefaultPointsVfx SetStartPositionWorldSpace(Vector3 startPos)
      {
         LocalSpaceStartLocation = transform.parent.InverseTransformPoint(startPos);
         return this;
      }
   
      /// <summary>
      /// Sets the <see cref="EndLocation"/>
      /// </summary>
      /// <param name="endPos"></param>
      /// <returns></returns>
      public DefaultPointsVfx SetMoveDistance(Vector2 endPos)
      {
         MoveDistance = endPos;
         return this;
      }
   
      /// <summary>
      /// Sets the color to use for this object,
      /// text and outline colors on the TMP <see cref="ScoreEarnedText"/>
      /// </summary>
      /// <param name="color"></param>
      /// <returns></returns>
      public virtual DefaultPointsVfx SetColor(Color color)
      {
         if (ScoreEarnedText != null)
         {
            ScoreEarnedText.color = color;
            ScoreEarnedText.outlineColor = color;
         }
         return this;
      }

      /// <summary>
      /// Animate the points vfx object
      /// </summary>
      /// <returns>Animation sequence</returns>
      public virtual Sequence Animate()
      {
         // Reset it to it's starting state
         var transformToMove = transform;
         transformToMove.transform.localPosition = LocalSpaceStartLocation;
      
         var textTransform = ScoreEarnedText.transform;
         textTransform.localScale = Vector2.zero;
         ScoreEarnedText.alpha = .95f;
      
         if (ParticleCanvasGroup != null) ParticleCanvasGroup.alpha = 0;

         var sequence = DOTween.Sequence();
      
         //Particle
         if (ParticleCanvasGroup != null && ParticleBurst != null)
         {
            ParticleBurst.AnimateParticleFX(ParticleBurst.transform);
         
            sequence
               .Append(ParticleCanvasGroup.DOFade(.3f, .13f * AnimationDuration).SetEase(Ease.Linear))
               .Append(ParticleCanvasGroup.DOFade(.7f, .055f * AnimationDuration).SetEase(Ease.Linear))
               .Append(ParticleCanvasGroup.DOFade(.4f, .073f * AnimationDuration).SetEase(Ease.Linear));
         }

         //Vertical Movement
         sequence.Insert(0, transformToMove.DOLocalMove(LocalSpaceStartLocation + MoveDistance, AnimationDuration));
      
         //Scale
         sequence.Insert(0, textTransform.DOScale(Vector3.one * .89f, .27f * AnimationDuration).SetEase(Ease.OutBack));
         sequence.Insert(.8f * AnimationDuration, textTransform.DOScale(Vector3.zero, .18f * AnimationDuration));
      
         //Text Opacity
         sequence.Insert(.8f * AnimationDuration, ScoreEarnedText.DOFade(0, .2f * AnimationDuration));

         sequence.OnComplete(VfxComplete);
      
         // return the running sequence in case someone cares
         return sequence;
      }

      /// <summary>
      /// When the points vfx is completely done and should not be doing anything anymore
      /// </summary>
      protected virtual void VfxComplete()
      {
         _pointsManager.VfxAnimationComplete(this);
      }
   }
}
