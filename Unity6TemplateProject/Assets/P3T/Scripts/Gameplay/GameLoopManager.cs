using System;
using UnityEngine;
using System.ComponentModel;
using JetBrains.Annotations;

/// <summary>
/// This manages the main game loop,
/// which should only start when the scene is fully ready
/// This manager is not a singleton and does not survive the game loop
/// This interacts with the <see cref="GameMgr"/> for any persistant data or states
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    /// <summary>
    /// Count up (time spent) or count down (time limit)
    /// </summary>
    [SerializeField] private bool IsCountdownTimer;

    /// <summary>
    /// Max timer, could be set by difficulty or other outside sources
    /// </summary>
    [Description("If using countdown timer")]
    [SerializeField] private float MaxTimer = 120f;

    /// <summary>
    /// Timer for use with the <see cref="IsCountdownTimer"/>
    /// </summary>
    [UsedImplicitly] // Accessible in case UI wants to show value
    public float GameTimer { get; private set; }

    /// <summary>
    /// todo Something should tell the game to start
    /// </summary>
    public void StartGame()
    {
        // Should the game loop control its own UI, or should that be the GameMgr?
        UIMgr.Instance.ShowMenu(GameMenus.InGameUI);
        
        GameMgr.Instance.StartGame();

        if (IsCountdownTimer)
        {
            GameTimer = MaxTimer;
        }
    }

    private void Update()
    {
        if (GameMgr.Instance.IsGameRunning)
        {
            if (!IsCountdownTimer)
            {
                GameTimer += Time.timeScale * Time.deltaTime; // Count up for now, may change later.
                throw new NotImplementedException("Nothing currently ends the game loop!");
            }
            else
            {
                GameTimer -= Time.timeScale * Time.deltaTime;

                if(GameTimer <= 0)
                {
                    GameOver();
                }
            }
        }
    }

    /// <summary>
    /// End the game loop
    /// Either if time expires or some other reason
    /// </summary>
    private void GameOver()
    {
        GameMgr.Instance.GameOver();
    }

}
