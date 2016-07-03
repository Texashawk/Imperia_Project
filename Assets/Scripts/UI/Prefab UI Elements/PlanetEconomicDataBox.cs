using UnityEngine;
using UnityEngine.UI;
using HelperFunctions;
using StellarObjects;
using TMPro;
using Tooltips;

public class PlanetEconomicDataBox : MonoBehaviour {

    PlanetData pData; // reference for planet data
    GameData gameDataRef; // to determine mode
    GraphicAssets graphicsDataRef; // for character pictures and other global pictures
    public GameObject PlanetName;
    public GameObject PlanetType;
    public GameObject PlanetPopulation;
    public GameObject PlanetGPP;
    public GameObject ViceroyPortrait;
    public GameObject DevelopmentBar;
    public GameObject ADL;
    public GameObject ViceroyName;
    public GameObject ViceroyRevenue;
    public GameObject ViceroyADM;
    public GameObject EnergyImportNeed;
    public GameObject BasicImportNeed;
    public GameObject HeavyImportNeed;
    public GameObject RareImportNeed;
    public GameObject FoodImportNeed;

    private TextMeshProUGUI planetName;
    private TextMeshProUGUI planetType;
    private TextMeshProUGUI planetPopulation;
    private TextMeshProUGUI planetGPP;
    private Image viceroyPortrait;
    private Image developmentBar;
    private TextMeshProUGUI aDevelopmentLevel;
    private TextMeshProUGUI viceroyName;
    private TextMeshProUGUI viceroyRevenue;
    private TextMeshProUGUI viceroyADM;
    private TextMeshProUGUI energyImportNeed;
    private TextMeshProUGUI basicImportNeed;
    private TextMeshProUGUI heavyImportNeed;
    private TextMeshProUGUI rareImportNeed;
    private TextMeshProUGUI foodImportNeed;

    private bool boxIsInitialized = false;


    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();

        planetName = PlanetName.GetComponent<TextMeshProUGUI>();
        planetType = PlanetType.GetComponent<TextMeshProUGUI>();
        planetPopulation = PlanetPopulation.GetComponent<TextMeshProUGUI>();
        planetGPP = PlanetGPP.GetComponent<TextMeshProUGUI>();
        viceroyPortrait = ViceroyPortrait.GetComponent<Image>();
        developmentBar = DevelopmentBar.GetComponent<Image>();
        aDevelopmentLevel = ADL.GetComponent<TextMeshProUGUI>();
        viceroyName = ViceroyName.GetComponent<TextMeshProUGUI>();
        viceroyRevenue = ViceroyRevenue.GetComponent<TextMeshProUGUI>();
        viceroyADM = ViceroyADM.GetComponent<TextMeshProUGUI>();
        energyImportNeed = EnergyImportNeed.GetComponent<TextMeshProUGUI>();
        basicImportNeed = BasicImportNeed.GetComponent<TextMeshProUGUI>();
        heavyImportNeed = HeavyImportNeed.GetComponent<TextMeshProUGUI>();
        rareImportNeed = RareImportNeed.GetComponent<TextMeshProUGUI>();
        foodImportNeed = FoodImportNeed.GetComponent<TextMeshProUGUI>(); 

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
        string vID = pData.Viceroy.PictureID; // viceroy info
        planetName.text = pData.Name;
        planetType.text = pData.Type.ToString();
        planetPopulation.text = pData.TotalPopulation.ToString("N0") + "M";
        planetGPP.text = pData.GrossPlanetaryProduct.ToString("N0") + "Bn";
        viceroyPortrait.sprite = graphicsDataRef.CharacterList.Find(p => p.name == vID);
        developmentBar.fillAmount = (pData.AverageDevelopmentLevel / 100f);
        aDevelopmentLevel.text = pData.AverageDevelopmentLevel.ToString("N0");
        viceroyName.text = pData.Viceroy.Name;
        viceroyRevenue.text = (pData.GrossPlanetaryProduct * .25f).ToString("N0") + "bn";
        viceroyADM.text = pData.TotalAdmin.ToString("N0");
        energyImportNeed.text = pData.EnergyImportance.ToString("N0");
        basicImportNeed.text = pData.BasicImportance.ToString("N0");
        heavyImportNeed.text = pData.HeavyImportance.ToString("N0");
        rareImportNeed.text = pData.RareImportance.ToString("N0");
        foodImportNeed.text = pData.FoodImportance.ToString("N0");
    }
}
