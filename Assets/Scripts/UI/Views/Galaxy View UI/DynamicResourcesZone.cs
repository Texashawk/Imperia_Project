using UnityEngine;
using UnityEngine.UI;
using Managers;
using StellarObjects;
using TMPro;

public class DynamicResourcesZone : MonoBehaviour
{
    private GlobalGameData gameDataRef;
    private TextMeshProUGUI empireTreasuryRevenues;
    private TextMeshProUGUI empireTreasuryExpenses;
    //private TextMeshProUGUI focusObjectName;
    private UIManager uiManagerRef;
    private TextMeshProUGUI emperorAP;

    // Use this for initialization
    void Awake()
    {
        // initialize all empire bar stats
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // get global game data (date, location, version, etc)
        uiManagerRef = GameObject.Find("UI Engine").GetComponent<UIManager>(); // get global game data (date, location, version, etc)
        empireTreasuryRevenues = GameObject.Find("EmpireTreasuryRevenues").GetComponent<TextMeshProUGUI>();
        empireTreasuryExpenses = GameObject.Find("EmpireTreasuryExpenses").GetComponent<TextMeshProUGUI>();
        //focusObjectName = GameObject.Find("Focus Object Name").GetComponent<TextMeshProUGUI>();
        emperorAP = GameObject.Find("Action Points").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicResourcesZone();
    }

    void UpdateDynamicResourcesZone()
    {
        // determine the name of the dynamic selection
        //if (uiManagerRef.ViewMode == UIManager.eViewMode.System)
        //    focusObjectName.text = "THE " + uiManagerRef.SelectedStellarObject.GetComponent<Star>().starData.Name.ToUpper() + " SYSTEM"; // the selected star's name    
        //else if (uiManagerRef.ViewMode == UIManager.eViewMode.Planet)
        //    focusObjectName.text = "THE PLANET OF " + uiManagerRef.selectedPlanet.Name.ToUpper(); // the selected star's name
        //else
        //    focusObjectName.text = "THE CELESTIAL EMPIRE"; // your empire's name

        empireTreasuryRevenues.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Revenues);//.ToString("N2") + " M";
        empireTreasuryExpenses.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Expenses); //.ToString("N2") + " M";
        emperorAP.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").ActionPoints.ToString("N0"); // get human leader   
    }
}