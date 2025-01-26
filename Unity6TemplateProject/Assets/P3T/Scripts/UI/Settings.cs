using P3T.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace P3T.Scripts.UI
{
	public class Settings : MenuBase
	{
		[SerializeField] private Button BackButton;

		[Header("Audio")] 
		[SerializeField] private Slider SliderMasterVolume;
		[SerializeField] private Slider SliderMusicVolume;
		[SerializeField] private Slider SliderSoundsVolume;

		public override GameMenus MenuType()
		{
			return GameMenus.SettingsMenu;
		}

		public void Close()
		{
			UiMgr.Instance.HideMenu(GameMenus.SettingsMenu);
		}

		private void OnEnable()
		{
			BackButton.Select();
			UpdateAudioDisplay();
		}

#region Audio

		public void SliderValueChanged()
		{
			AudioMgr.Instance.GlobalVolume = SliderMasterVolume.value;
			AudioMgr.Instance.MusicVolume = SliderMusicVolume.value;
			AudioMgr.Instance.SfxVolume = SliderSoundsVolume.value;
            
			SaveUtil.Save();

			UpdateAudioDisplay();
		}

		private void UpdateAudioDisplay()
		{
			SliderMasterVolume.SetValueWithoutNotify(SaveUtil.SavedValues.GlobalVolume);
			SliderMusicVolume.SetValueWithoutNotify(SaveUtil.SavedValues.MusicVolume);
			SliderSoundsVolume.SetValueWithoutNotify(SaveUtil.SavedValues.SfxVolume);
		}
#endregion
	}
}