using UnityEngine;
using UnityEngine.UI;
using HelperFunctions;
using Managers;
using StellarObjects;
using TMPro;

public class DynamicResourcesZone : MonoBehaviour
{
    private GameData gameDataRef;  
    private UIManager uiManagerRef;
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
        energyStock = GameObject.Find("Energy Stock").GetComponent<TextMeshProUGUI>();
        basicStock = GameObject.Find("Basic Stock").GetComponent<TextMeshProUGUI>();
        heavyStock = GameObject.Find("Heavy Stock").GetComponent<TextMeshProUGUI>();
        rareStock = GameObject.Find("Rare Stock").GetComponent<TextMeshProUGUI>();
       
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicResourcesZone();
    }

    void UpdateDynamicResourcesZone()
    {
        float energyAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).EnergyStored;
        float basicAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).BasicStored;
        float heavyAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).HeavyStored;
        float rareAmount = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetCivilization("CIV0").CapitalPlanetID).RareStored;
               
        energyStock.text = energyAmount.ToString("N0");
        basicStock.text = basicAmount.ToString("N0");
        heavyStock.text = heavyAmount.ToString("N0");
        rareStock.text = rareAmount.ToString("N0");
    }
}