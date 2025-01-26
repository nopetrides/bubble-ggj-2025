using P3T.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace P3T.Scripts.UI
{
    /// <summary>
    ///     The main menu when starting the game
    ///     The simple entry point after the game loads and return point if exiting gameplay
    /// </summary>
    public class MainMenu : MenuBase
	{
		[SerializeField] private Button StartButton;

		private void OnEnable()
		{
			StartButton.Select();
		}

		public override GameMenus MenuType()
		{
			return GameMenus.MainMenu;
		}

		public void ButtonStart()
		{
			SceneMgr.Instance.LoadScene(GameScenes.MainGame, GameMenus.None);
		}

		public void ButtonSettings()
		{
			UiMgr.Instance.ShowMenu(GameMenus.SettingsMenu);
		}

		public void ButtonQuit()
		{
			Application.Quit();
		}
	}
}