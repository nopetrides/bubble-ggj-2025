using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void RefreshUI() 
    {
        Debug.Log("bool Value"+ SaveUtil.SavedValues.ToggleValue);
        Debug.Log("int value" +SaveUtil.SavedValues.IntValue);
        Debug.Log("string Value" +SaveUtil.SavedValues.StringValue);
    }

    public void Save()
    {
        SaveUtil.SavedValues.ToggleValue = true;
        SaveUtil.SavedValues.IntValue = 10;
        SaveUtil.SavedValues.StringValue = "hey";
        SaveUtil.OnSaveCompleted += RefreshUI;
        SaveUtil.Save();
           
    }

    public void Load()
    {
        SaveUtil.OnLoadCompleted += RefreshUI;
        SaveUtil.Load();
    }
}
