using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings menu
/// Should include sliders and toggles for player preferences
/// Such as audio settings or accessibility settings
/// </summary>
public class Settings : MenuBase
{
    [SerializeField] private Button BackButton;

    private void OnEnable()
    {
        BackButton.Select();
    }

    public override GameMenus MenuType()
    {
        return GameMenus.SettingsMenu;
    }

    public void Close()
    {
        UIMgr.Instance.HideMenu(GameMenus.SettingsMenu);
    }
}
