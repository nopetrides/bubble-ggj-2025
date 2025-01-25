using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorSummaryWidget : MonoBehaviour
    {
        [SerializeField] private CanvasGroup CanvasGroup;
        [SerializeField] private TMP_Text TotalPointsLabel;
        [SerializeField] private Button ContinueButton;
        [SerializeField] private List<SurvivorBonusPointsWidget> BonusPointsWidgets = new();
        private Action _continueCallback;

        private void OnEnable()
        {
            ContinueButton.onClick.AddListener(() => _continueCallback?.Invoke());
        }

        public void Setup(SurvivorController.PointsTracker pointsTracker, int totalPoints,
            Action continueCallback)
        {
            _continueCallback = continueCallback;

            var brickPointsWidget = BonusPointsWidgets[0];
            brickPointsWidget.transform.SetAsLastSibling();
            brickPointsWidget.Setup("Time", NumUtil.FormatScoreForDisplay(pointsTracker.SurvivalPoints), true);

            if (pointsTracker.HazardPoints != 0)
            {
                var hazardPointsWidget = BonusPointsWidgets[1];
                hazardPointsWidget.gameObject.SetActive(true);
                hazardPointsWidget.transform.SetAsLastSibling();
                hazardPointsWidget.Setup("Enemies", NumUtil.FormatScoreForDisplay(pointsTracker.HazardPoints),
                    false);
            }

            if (pointsTracker.PickupPoints != 0)
            {
                var pickupPointsWidget = BonusPointsWidgets[2];
                pickupPointsWidget.gameObject.SetActive(true);
                pickupPointsWidget.transform.SetAsLastSibling();
                pickupPointsWidget.Setup("Points", NumUtil.FormatScoreForDisplay(pointsTracker.PickupPoints),
                    false);
            }

            if (pointsTracker.PickupPowers != 0)
            {
                var pickupPowerWidget = BonusPointsWidgets[3];
                pickupPowerWidget.gameObject.SetActive(true);
                pickupPowerWidget.transform.SetAsLastSibling();
                pickupPowerWidget.Setup("Powers", NumUtil.FormatScoreForDisplay(pointsTracker.PickupPowers),
                    false);
            }

            StartCoroutine(PlayAnimation(totalPoints));
        }


        private IEnumerator PlayAnimation(int totalPoints)
        {
            //Init
            CanvasGroup.alpha = 0;
            foreach (var widget in BonusPointsWidgets) widget.CanvasGroup.alpha = 0;
            TotalPointsLabel.text = "0";
            ContinueButton.transform.localScale = Vector3.zero;

            // //give a frame for layouts to calculate and what-not
            yield return null;

            //Full Fade in
            CanvasGroup.DOFade(1, .25f);

            var sequence = DOTween.Sequence();
            sequence.SetDelay(.25f);

            for (var i = 0; i < BonusPointsWidgets.Count; i++)
            {
                if (BonusPointsWidgets[i].gameObject.activeInHierarchy == false)
                    continue; // Don't animate things that are not turned on
                var widget = BonusPointsWidgets[i];
                var delay = i * .5f;
                sequence.Insert(delay, widget.CanvasGroup.transform.DOLocalMoveY(50, 1)
                    .From()
                    .SetEase(Ease.OutQuad));
                sequence.Insert(delay, widget.CanvasGroup.DOFade(1, 1));
            }

            var points = 0;
            DOTween.To(() => points, p =>
            {
                points = p;
                TotalPointsLabel.text = NumUtil.FormatScoreForDisplay(points);
            }, totalPoints, sequence.Duration()).SetDelay(sequence.Delay());

            sequence.Append(ContinueButton.transform.DOScale(Vector3.one, .25f)
                .SetEase(Ease.OutBack));
        }

    }
}