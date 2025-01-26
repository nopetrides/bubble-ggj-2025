using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class LabeledPointsVfx : DefaultPointsVfx
	{
		[SerializeField] private float RelativeLabelFontSize = .67f;
    
		private string _labelText;
    
		public LabeledPointsVfx SetLabel(string labelText)
		{
			_labelText = labelText;
			return this;
		}

		public override DefaultPointsVfx SetPointsValue(int points)
		{
			Points = points;
			UpdateText();
			return this;
		}

		private void UpdateText()
		{
			var prefix = "";
			if (string.IsNullOrEmpty(_labelText) == false)
			{
				var vOffset = (1 - RelativeLabelFontSize) / 2;
				prefix = $"<size={RelativeLabelFontSize}em><voffset={vOffset}em>{_labelText}</voffset></size> ";
			}
			ScoreEarnedText.text = $"{prefix}{Points:D}";
		}
	}
}