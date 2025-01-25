using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorBonusPointsWidget : MonoBehaviour
	{
		public CanvasGroup CanvasGroup;
    
		[SerializeField] private TMP_Text TitleLabel;
		[SerializeField] private TMP_Text PointsLabel;
		[SerializeField] private Image BorderImage;
    
		[SerializeField] private Color NormalColor;
		[SerializeField] private Color SpecialColor;
		public void Setup(string label, string points, bool specialColor)
		{
			TitleLabel.text = label;
			PointsLabel.text = points;
			
			BorderImage.color = specialColor ? SpecialColor : NormalColor;
		}
	}
}