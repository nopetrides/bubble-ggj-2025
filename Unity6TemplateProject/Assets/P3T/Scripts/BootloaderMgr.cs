using P3T.Scripts.Managers;
using UnityEngine;

namespace P3T.Scripts
{
    /// <summary>
    ///     First scene the game auto loads into, and only load into this once.
    ///     Manages the very beginning of the game lifecycle, one time setup only stuff
    /// </summary>
    public class BootloaderMgr : MonoBehaviour
	{
		private void Start()
		{
			UiMgr.Instance.ShowSplash(OnSplashAnimationComplete);
		}

		private void OnSplashAnimationComplete()
		{
			SceneMgr.Instance.LoadScene(GameScenes.MainMenu, GameMenus.MainMenu);
		}
	}
}