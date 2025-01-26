using P3T.Scripts.Managers;

/// <summary>
///     Game over screen
///     Allows for quitting or retrying
/// </summary>
public class GameOver : MenuBase
{
	public override GameMenus MenuType()
	{
		return GameMenus.GameOverMenu;
	}

	public void ButtonRetry()
	{
		SceneMgr.Instance.LoadScene(GameScenes.MainGame, GameMenus.InGameUI);
	}

	public void ButtonMainMenu()
	{
		SceneMgr.Instance.LoadScene(GameScenes.MainMenu, GameMenus.MainMenu);
	}
}