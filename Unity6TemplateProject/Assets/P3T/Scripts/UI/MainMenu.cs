using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The main menu when starting the game
/// The simple entry point after the game loads and return point if exiting gameplay
/// </summary>
public class MainMenu : MenuBase
{
    [SerializeField] private Button StartButton;
    
    public override GameMenus MenuType()
    {
        return GameMenus.MainMenu;
    }

    private void OnEnable()
    {
        StartButton.Select();
    }

    public void ButtonStart()
    {
        SceneMgr.Instance.LoadScene(GameScenes.Gameplay, GameMenus.InGameUI);
    }

    public void ButtonSettings()
    {
        UIMgr.Instance.ShowMenu(GameMenus.SettingsMenu);
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }
}
