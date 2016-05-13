using UnityEngine;
using UnityEngine.UI;
using HelperFunctions;
using Managers;
using StellarObjects;
using TMPro;

public class DynamicResourcesZone : MonoBehaviour
{
    private GameData gameDataRef;
    private TextMeshProUGUI empireTreasuryRevenues;
    private TextMeshProUGUI empireTreasuryExpenses;
    private UIManager uiManagerRef;
    private TextMeshProUGUI emperorAP;
    private TextMeshProUGUI energyStock;
    private TextMeshProUGUI basicStock;
    private TextMeshProUGUI heavyStock;
    private TextMeshProUGUI rareStock;

    // Use this for initialization
    void Awake()
    {
        // initialize all empire bar stats
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
        uiManagerRef = GameObject.Find("GameEngine").GetComponent<UIManager>(); // get global game data (date, location, version, etc)
        empireTreasuryRevenues = GameObject.Find("EmpireTreasuryRevenues").GetComponent<TextMeshProUGUI>();
        empireTreasuryExpenses = GameObject.Find("EmpireTreasuryExpenses").GetComponent<TextMeshProUGUI>();
        energyStock = GameObject.Find("Energy Stock").GetComponent<TextMeshProUGUI>();
        basicStock = GameObject.Find("Basic Stock").GetComponent<TextMeshProUGUI>();
        heavyStock = GameObject.Find("Heavy Stock").GetComponent<TextMeshProUGUI>();
        rareStock = GameObject.Find("Rare Stock").GetComponent<TextMeshProUGUI>();
        emperorAP = GameObject.Find("Action Points").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicResourcesZone();
    }

    void UpdateDynamicResourcesZone()
    {
        float energyAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).EnergyStored / 1000f;
        float basicAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).BasicStored / 1000f;
        float heavyAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).HeavyStored / 1000f;
        float rareAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).RareStored / 1000f;

        empireTreasuryRevenues.text = StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Revenues);//.ToString("N2") + " M";
        empireTreasuryExpenses.text = StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Expenses); //.ToString("N2") + " M";
        emperorAP.text = DataRetrivalFunctions.GetCivilization("CIV0").PlayerEmperor.ActionPoints.ToString("N0"); // get human leader
        energyStock.text = energyAmount.ToString("N1") + "K";
        basicStock.text = basicAmount.ToString("N1") + "K";
        heavyStock.text = heavyAmount.ToString("N1") + "K";
        rareStock.text = rareAmount.ToString("N1") + "K";
    }
}