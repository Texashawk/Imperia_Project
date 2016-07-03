using UnityEngine;
using UnityEngine.UI;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using TMPro;

public class PlanetBuildPlanWindow : MonoBehaviour {

    // references
    GameData gDataRef;
    UIManager uiManagerRef;
    GalaxyView gScreenRef;
    PlanetData pData;

    // public game objects
    public GameObject BasicBPCounter;   
    public GameObject HeavyBPCounter;
    public GameObject RareBPCounter;
    public GameObject FarmAllocation;
    public GameObject MineAllocation;
    public GameObject FactoryAllocation;
    public GameObject HighTechAllocation;
    public GameObject AdminAllocation;
    public GameObject AcademyAllocation;
    public GameObject GroundMilitaryAllocation;
    public GameObject ShipyardAllocation;
    public GameObject InfrastructureAllocation;
    public GameObject FarmProgress;
    public GameObject MineProgress;

    // private components
    TextMeshProUGUI basicBPCounter;
    TextMeshProUGUI heavyBPCounter;
    TextMeshProUGUI rareBPCounter;
    TextMeshProUGUI farmAllocation;
    TextMeshProUGUI mineAllocation;
    TextMeshProUGUI factoryAllocation;
    TextMeshProUGUI highTechAllocation;
    TextMeshProUGUI adminAllocation;
    TextMeshProUGUI academyAllocation;
    TextMeshProUGUI groundMilitaryAllocation;
    TextMeshProUGUI shipyardAllocation;
    TextMeshProUGUI infrastructureAllocation;
    Image farmProgress;
    Image mineProgress;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        basicBPCounter = BasicBPCounter.GetComponent<TextMeshProUGUI>();
        heavyBPCounter = HeavyBPCounter.GetComponent<TextMeshProUGUI>();
        rareBPCounter = RareBPCounter.GetComponent<TextMeshProUGUI>();
        farmAllocation = FarmAllocation.GetComponent<TextMeshProUGUI>();
        mineAllocation = MineAllocation.GetComponent<TextMeshProUGUI>();
        factoryAllocation = FactoryAllocation.GetComponent<TextMeshProUGUI>();
        highTechAllocation = HighTechAllocation.GetComponent<TextMeshProUGUI>();
        adminAllocation = AdminAllocation.GetComponent<TextMeshProUGUI>();
        academyAllocation = AcademyAllocation.GetComponent<TextMeshProUGUI>();
        groundMilitaryAllocation = GroundMilitaryAllocation.GetComponent<TextMeshProUGUI>();
        shipyardAllocation = ShipyardAllocation.GetComponent<TextMeshProUGUI>();
        infrastructureAllocation = InfrastructureAllocation.GetComponent<TextMeshProUGUI>();
        farmProgress = FarmProgress.GetComponent<Image>();
        mineProgress = MineProgress.GetComponent<Image>();
    }
	
	void Update ()
    {
        // check for updated planet data

        if (uiManagerRef.selectedPlanet != null)
        {
            if (pData == null)
            {
                pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                UpdateBuildPlanInfo();
            }

            else if (pData != null)
            {
                if (pData != gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData)
                {
                    pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                    UpdateBuildPlanInfo();
                }
            }
        }      
    }

    void UpdateBuildPlanInfo()
    {
        // build points
        basicBPCounter.text = pData.BasicBPsGeneratedMonthly.ToString("N0");
        heavyBPCounter.text = pData.HeavyBPsGeneratedMonthly.ToString("N0");
        rareBPCounter.text = pData.RareBPsGeneratedMonthly.ToString("N0");

        // allocations
        farmAllocation.text = pData.BuildPlan.FarmsAllocation.ToString("P0");
        mineAllocation.text = pData.BuildPlan.MineAllocation.ToString("P0");
        factoryAllocation.text = pData.BuildPlan.FactoryAllocation.ToString("P0");
        highTechAllocation.text = pData.BuildPlan.HighTechAllocation.ToString("P0");
        adminAllocation.text = pData.BuildPlan.AdminAllocation.ToString("P0");
        academyAllocation.text = pData.BuildPlan.AcademyAllocation.ToString("P0");
        groundMilitaryAllocation.text = pData.BuildPlan.GroundMilitaryAllocation.ToString("P0");
        shipyardAllocation.text = pData.BuildPlan.ShipyardAllocation.ToString("P0");
        infrastructureAllocation.text = pData.BuildPlan.InfraAllocation.ToString("P0");

        // bar progress images (test!)
        farmProgress.fillAmount = pData.PercentToNewFarmLevel;
        mineProgress.fillAmount = pData.PercentToNewMineLevel;
    }
}
