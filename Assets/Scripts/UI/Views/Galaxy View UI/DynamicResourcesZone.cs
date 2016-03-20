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
        emperorAP = GameObject.Find("Action Points").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicResourcesZone();
    }

    void UpdateDynamicResourcesZone()
    {
        empireTreasuryRevenues.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Revenues);//.ToString("N2") + " M";
        empireTreasuryExpenses.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Expenses); //.ToString("N2") + " M";
        emperorAP.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").ActionPoints.ToString("N0"); // get human leader   
    }
}