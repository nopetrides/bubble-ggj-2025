/// <summary>
/// The "don't destroy on load" parent that contains all other global managers
/// </summary>
public class GlobalsMgr : Singleton<GlobalsMgr>
{
    public override void Awake()
    {
        base.Awake();
        if (Instance == this)
            DontDestroyOnLoad(gameObject);
    }
}
