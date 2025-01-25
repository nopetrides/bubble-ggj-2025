using System;
using UnityEngine;

/// <summary>
/// A singleton for communicating with the player object when it exists
/// </summary>
public class PlayerMgr : Singleton<PlayerMgr>
{
    /*
    public override void Awake() {
        base.Awake();
    }*/
    
    /// <summary>
    /// This script should be attached to the player object
    /// Meant for single-player games where accessing the player object quickly is convenient
    /// </summary>
    public GameObject PlayerObject => gameObject;

    public void Move()
    {
        throw new NotImplementedException("Player does not have a controller");
    }
    
    /// <summary>
    /// Handles the player using the pause input action
    /// TODO move to player input handler separate from player controller
    /// </summary>
    public void PauseInput()
    {
        // Run pause from game manager
        GameMgr.Instance.PauseGameToggle();
    }
}