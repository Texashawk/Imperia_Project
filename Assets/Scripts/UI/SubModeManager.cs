using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubModeManager : MonoBehaviour { // only used to switch the sub mode; a 'route station' script only

    private GameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();
    private Button intelButton;

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // tie the game camera script to the data
        intelButton = GameObject.Find("Intel Button").GetComponent<Button>();
    }

    public void SwitchSubModeIntel()
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Intel)
            gGameDataRef.uiSubMode = GameData.eSubMode.Intel;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeWar()
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.War)
            gGameDataRef.uiSubMode = GameData.eSubMode.War;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeEmperor()
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Emperor)
            gGameDataRef.uiSubMode = GameData.eSubMode.Emperor;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeFinance()
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Finance)
            gGameDataRef.uiSubMode = GameData.eSubMode.Finance;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeDiplomacy()
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Diplomacy)
            gGameDataRef.uiSubMode = GameData.eSubMode.Diplomacy;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeScience()
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Science)
            gGameDataRef.uiSubMode = GameData.eSubMode.Science;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeNone()
    {     
        gGameDataRef.uiSubMode = GameData.eSubMode.None;  
    }
}
