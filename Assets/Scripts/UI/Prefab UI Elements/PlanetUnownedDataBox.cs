using UnityEngine;
using UnityEngine.UI;
using HelperFunctions;
using StellarObjects;
using TMPro;
using Tooltips;

public class PlanetUnownedDataBox : MonoBehaviour {

    PlanetData pData; // reference for planet data
    GameData gameDataRef; // to determine mode
    GraphicAssets graphicsDataRef; // for character pictures and other global pictures
    public GameObject PlanetName;
    public GameObject PlanetType;
    public GameObject PlanetBioLevel;
    public GameObject PlanetEnergyLevel;
    public GameObject PlanetBasicLevel;
    public GameObject PlanetHeavyLevel;
    public GameObject PlanetRareLevel;
    public GameObject Trait1;
    public GameObject Trait2;

    private TextMeshProUGUI planetName;
    private TextMeshProUGUI planetType;
    private TextMeshProUGUI planetBioLevel;
    private TextMeshProUGUI planetEnergyLevel;
    private TextMeshProUGUI planetBasicLevel;
    private TextMeshProUGUI planetHeavyLevel;
    private TextMeshProUGUI planetRareLevel;
    private TextMeshProUGUI planetTrait1;
    private TextMeshProUGUI planetTrait2;

    private bool boxIsInitialized = false;

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();

        planetName = PlanetName.GetComponent<TextMeshProUGUI>();
        planetType = PlanetType.GetComponent<TextMeshProUGUI>();
        planetBioLevel = PlanetBioLevel.GetComponent<TextMeshProUGUI>();
        planetEnergyLevel = PlanetEnergyLevel.GetComponent<TextMeshProUGUI>();
        planetBasicLevel = PlanetBasicLevel.GetComponent<TextMeshProUGUI>();
        planetHeavyLevel = PlanetHeavyLevel.GetComponent<TextMeshProUGUI>();
        planetRareLevel = PlanetRareLevel.GetComponent<TextMeshProUGUI>();
        planetTrait1 = Trait1.GetComponent<TextMeshProUGUI>();
        planetTrait2 = Trait2.GetComponent<TextMeshProUGUI>();
        
    }

    void Start()
    {
        if (pData != null && !boxIsInitialized)
        {
            UpdatePlanetBox();
            boxIsInitialized = true;
        }
    }

    public void PopulateDataBox(string pID)
    {
        pData = DataRetrivalFunctions.GetPlanet(pID);
    }

    public void Update()
    {
        if (pData != null && !boxIsInitialized)
        {
            UpdatePlanetBox();
            boxIsInitialized = true;
        }
    }

    private void UpdatePlanetBox()
    {
        planetName.text = pData.Name;
        planetType.text = pData.Type.ToString();
        planetBioLevel.text = pData.AdjustedBio.ToString("N0");
        planetEnergyLevel.text = pData.Energy.ToString("N0");
        planetBasicLevel.text = pData.BasicMaterials.ToString("N0");
        planetHeavyLevel.text = pData.HeavyMaterials.ToString("N0");
        planetRareLevel.text = pData.RareMaterials.ToString("N0");
        if (pData.PlanetTraits.Count > 0)
            planetTrait1.text = pData.PlanetTraits[0].Name.ToLower();
        else
            planetTrait1.text = "NO TRAIT";
        if (pData.PlanetTraits.Count > 1)
            planetTrait2.text = pData.PlanetTraits[1].Name.ToLower();
        else
            planetTrait2.text = "NO TRAIT";
    }

}
