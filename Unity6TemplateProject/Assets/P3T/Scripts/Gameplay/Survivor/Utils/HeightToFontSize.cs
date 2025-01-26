using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace P3T.Scripts.Gameplay.Survivor
{
	[RequireComponent(typeof(TMP_Text))]
	public class HeightToFontSize : UIBehaviour
	{
		[Tooltip("0 or lower for no limit")]
		[SerializeField] private float MinFontSize = 0;
		[Tooltip("0 or lower for no limit")]
		[SerializeField] private float MaxFontSize = 0;
    
		private TMP_Text _text;
		private float _lastProcessedHeight;
    
		protected override void OnEnable()
		{
			base.OnEnable();
			if (_text == null) _text = GetComponent<TMP_Text>();
			_text.enableAutoSizing = false;
			OnRectTransformDimensionsChange();
		}

		protected override void OnDisable()
		{
			_lastProcessedHeight = -1;
			base.OnDisable();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
        
			if (_text == null) _text = GetComponent<TMP_Text>();

			var height = _text.rectTransform.rect.height;
			if (Math.Abs(height - _lastProcessedHeight) < .1f) return;

			//apply size limits
			if (MinFontSize > 0 && MinFontSize > height) height = MinFontSize;
			if (MaxFontSize > 0 && MaxFontSize < height) height = MaxFontSize;

			_text.fontSize = height;
		}
	}
}