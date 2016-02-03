using UnityEngine;
using UnityEngine.UI;
using UI.Manager;
using StellarObjects;

public class DynamicResourcesZone : MonoBehaviour
{

    private GlobalGameData gameDataRef;
    private Text empireTreasuryRevenues;
    private Text empireTreasuryExpenses;
    private Text focusObjectName;
    private UIManager uiManagerRef;
    private Text emperorAP;

    // Use this for initialization
    void Awake()
    {
        // initialize all empire bar stats
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // get global game data (date, location, version, etc)
        uiManagerRef = GameObject.Find("UI Engine").GetComponent<UIManager>(); // get global game data (date, location, version, etc)
        empireTreasuryRevenues = GameObject.Find("EmpireTreasuryRevenues").GetComponent<Text>();
        empireTreasuryExpenses = GameObject.Find("EmpireTreasuryExpenses").GetComponent<Text>();
        focusObjectName = GameObject.Find("Focus Object Name").GetComponent<Text>();
        emperorAP = GameObject.Find("Action Points").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicResourcesZone();
    }

    void UpdateDynamicResourcesZone()
    {
        // determine the name of the dynamic selection
        if (uiManagerRef.ViewMode == UIManager.eViewMode.System)
            focusObjectName.text = "THE " + uiManagerRef.SelectedStellarObject.GetComponent<Star>().starData.Name.ToUpper() + " SYSTEM"; // the selected star's name    
        else if (uiManagerRef.ViewMode == UIManager.eViewMode.Planet)
            focusObjectName.text = "THE PLANET OF " + uiManagerRef.SelectedStellarObject.GetComponent<Planet>().planetData.Name.ToUpper(); // the selected star's name
        else
            focusObjectName.text = "THE CELESTIAL EMPIRE"; // your empire's name

        empireTreasuryRevenues.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Revenues);//.ToString("N2") + " M";
        empireTreasuryExpenses.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Expenses); //.ToString("N2") + " M";
        emperorAP.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").ActionPoints.ToString("N0"); // get human leader   
    }
}