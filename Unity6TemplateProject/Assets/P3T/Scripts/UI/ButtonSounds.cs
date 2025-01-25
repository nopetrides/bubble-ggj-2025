using P3T.Scripts.Managers;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSounds : MonoBehaviour
{
	[SerializeField] private AudioClip ClickSound;
	[SerializeField] private AudioClip HoverSound;

	// Start is called before the first frame update
	private void Start()
	{
		var button = GetComponent<Button>();
		if (button)
		{
			button.onClick.AddListener(PlayClickSound);
		}
		else
		{
			var betterButton = GetComponent<BetterButton>();
			if (betterButton) betterButton.onClick.AddListener(PlayClickSound);
		}

		var toggle = GetComponent<Toggle>();
		if (toggle)
		{
			toggle.onValueChanged.AddListener(PlayClickSound);
		}
		else
		{
			var betterToggle = GetComponent<BetterToggle>();
			if (betterToggle) betterToggle.onValueChanged.AddListener(PlayClickSound);
		}

		var slider = GetComponent<Slider>();
		if (slider) slider.onValueChanged.AddListener(PlayClickSound);
	}


	private void PlayClickSound(bool state)
	{
		var toggle = GetComponent<Toggle>(); // inefficient
		if (!toggle) return;
		if (!state && toggle.group != null && toggle.group.AnyTogglesOn()) return;

		AudioMgr.Instance.PlaySound(ClickSound);
	}

	private void PlayClickSound(float _)
	{
		AudioMgr.Instance.PlaySound(ClickSound, _);
	}

	private void PlayClickSound()
	{
		AudioMgr.Instance.PlaySound(ClickSound);
	}

    /// <summary>
    ///     Should be called from OnPointerEnter or OnSelected
    /// </summary>
    public void PlayHoverSound()
	{
		if (HoverSound != null)
			AudioMgr.Instance.PlaySound(HoverSound);
		else
			AudioMgr.Instance.PlaySound(AudioMgr.SoundTypes.ButtonHover);
	}
}