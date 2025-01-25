using UnityEngine;

/// <summary>
/// Basic camera manager.
/// This camera will always exist and is not tied to any player or specific game state
/// </summary>
public class CameraMgr : Singleton<CameraMgr>
{
    /*
    public override void Awake() {
        base.Awake();
    }*/
    
    [Header("Obj Refs")]
    public Camera MainCamera;

}