using UnityEngine;
using UnityEngine.UI;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using TMPro;
public class PlanetProductionWindow : MonoBehaviour {

    // references
    GameData gDataRef;
    UIManager uiManagerRef;
    PlanetView planetViewRef;
    GalaxyView gScreenRef;
    PlanetData pData;

    // public game objects
    public GameObject FarmPops;
    public GameObject MinePops;
    public GameObject AdminPops;
    public GameObject MerchantPops;
    public GameObject EngineerPops;
    public GameObject AcademicPops;
    public GameObject FarmerSkillLevel;
    public GameObject MinerSkillLevel;
    public GameObject AdminSkillLevel;
    public GameObject MerchantSkillLevel;
    public GameObject EngineerSkillLevel;
    public GameObject AcademicSkillLevel;
    public GameObject FarmCount;
    public GameObject EnergyFacilityCount;
    public GameObject MineCount;
    public GameObject FactoryCount;
    public GameObject AcademyCount;
    public GameObject AdminFacilityCount;
    public GameObject FoodBalance;
    public GameObject EnergyBalance;
    public GameObject BasicBalance;
    public GameObject HeavyBalance;
    public GameObject RareBalance;
    public GameObject AdminOutput;
    public GameObject FarmStaffing;
    public GameObject EnergyFacilityStaffing;
    public GameObject FactoryStaffing;
    public GameObject MineStaffing;
    public GameObject AcademyStaffing;
    public GameObject AdminStaffing;
    public GameObject FarmerEmployment;
    public GameObject MinerEmployment;
    public GameObject AdminEmployment;
    public GameObject EngineerEmployment;
    public GameObject AcademicEmployment;

    // private components
    TextMeshProUGUI farmPops;
    TextMeshProUGUI minePops;
    TextMeshProUGUI adminPops;
    TextMeshProUGUI merchantPops;
    TextMeshProUGUI engineerPops;
    TextMeshProUGUI academicPops;
    TextMeshProUGUI farmerSkillLevel;
    TextMeshProUGUI minerSkillLevel;
    TextMeshProUGUI adminSkillLevel;
    TextMeshProUGUI merchantSkillLevel;
    TextMeshProUGUI engineerSkillLevel;
    TextMeshProUGUI academicSkillLevel;
    TextMeshProUGUI farmCount;
    TextMeshProUGUI energyFacilityCount;
    TextMeshProUGUI mineCount;
    TextMeshProUGUI factoryCount;
    TextMeshProUGUI academyCount;
    TextMeshProUGUI adminFacilityCount;
    TextMeshProUGUI foodBalance;
    TextMeshProUGUI energyBalance;
    TextMeshProUGUI basicBalance;
    TextMeshProUGUI heavyBalance;
    TextMeshProUGUI rareBalance;
    TextMeshProUGUI adminOutput;

    // bar graphs
    Image farmStaffing;
    Image energyFacilityStaffing;
    Image factoryStaffing;
    Image mineStaffing;
    Image academyStaffing;
    Image adminStaffing;
    Image farmEmployment;
    Image minerEmployment;
    Image adminEmployment;
    Image engineerEmployment;
    Image academicEmployment;
    Button viceroyChat;
    

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        planetViewRef = GameObject.Find("UI Engine").GetComponent<PlanetView>();
        farmPops = FarmPops.GetComponent<TextMeshProUGUI>();
        minePops = MinePops.GetComponent<TextMeshProUGUI>();
        adminPops = AdminPops.GetComponent<TextMeshProUGUI>();
        merchantPops = MerchantPops.GetComponent<TextMeshProUGUI>();
        engineerPops = EngineerPops.GetComponent<TextMeshProUGUI>();
        academicPops = AcademicPops.GetComponent<TextMeshProUGUI>();
        farmerSkillLevel = FarmerSkillLevel.GetComponent<TextMeshProUGUI>();
        minerSkillLevel = MinerSkillLevel.GetComponent<TextMeshProUGUI>();
        adminSkillLevel = AdminSkillLevel.GetComponent<TextMeshProUGUI>();
        merchantSkillLevel = MerchantSkillLevel.GetComponent<TextMeshProUGUI>();
        engineerSkillLevel = EngineerSkillLevel.GetComponent<TextMeshProUGUI>();
        academicSkillLevel = AcademicSkillLevel.GetComponent<TextMeshProUGUI>();
        farmCount = FarmCount.GetComponent<TextMeshProUGUI>();
        energyFacilityCount = EnergyFacilityCount.GetComponent<TextMeshProUGUI>();
        mineCount = MineCount.GetComponent<TextMeshProUGUI>();
        factoryCount = FactoryCount.GetComponent<TextMeshProUGUI>();
        academyCount = AcademyCount.GetComponent <TextMeshProUGUI>();
        adminFacilityCount = AdminFacilityCount.GetComponent<TextMeshProUGUI>();
        foodBalance = FoodBalance.GetComponent<TextMeshProUGUI>();
        energyBalance = EnergyBalance.GetComponent<TextMeshProUGUI>();
        basicBalance = BasicBalance.GetComponent<TextMeshProUGUI>();
        heavyBalance = HeavyBalance.GetComponent<TextMeshProUGUI>();
        rareBalance = RareBalance.GetComponent<TextMeshProUGUI>();
        adminOutput = AdminOutput.GetComponent<TextMeshProUGUI>();
        farmStaffing = FarmStaffing.GetComponent<Image>();
        energyFacilityStaffing = EnergyFacilityStaffing.GetComponent<Image>();
        factoryStaffing = FactoryStaffing.GetComponent<Image>();
        mineStaffing = MineStaffing.GetComponent<Image>();
        academyStaffing = AcademyStaffing.GetComponent<Image>();
        adminStaffing = AdminStaffing.GetComponent<Image>();
        farmEmployment = FarmerEmployment.GetComponent<Image>();
        minerEmployment = MinerEmployment.GetComponent<Image>();
        adminEmployment = AdminEmployment.GetComponent<Image>();
        engineerEmployment = EngineerEmployment.GetComponent<Image>();
        academicEmployment = AcademicEmployment.GetComponent<Image>();
        viceroyChat = gameObject.transform.Find("Title_Bar/Hover_Icon_Talk").GetComponent<Button>();
        viceroyChat.onClick.AddListener(delegate { planetViewRef.ToggleViceroyMode(name); });
    }

    void Update()
    {
        // check for updated planet data
        if (uiManagerRef.selectedPlanet != null)
        {
            if (pData == null)
            {
                pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                UpdateProductionWindowInfo();
            }

            else if (pData != null)
            {
                if (pData != gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData)
                {
                    pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                    UpdateProductionWindowInfo();
                }
            }
        }
    }

    public void UpdateProductionWindowInfo()
    {
        // pop numbers
        farmPops.text = pData.TotalFarmers.ToString("N0");
        minePops.text = pData.TotalMiners.ToString("N0");
        adminPops.text = pData.TotalAdministrators.ToString("N0");
        merchantPops.text = pData.TotalMerchants.ToString("N0");
        engineerPops.text = pData.TotalEngineers.ToString("N0");
        academicPops.text = pData.TotalAcademics.ToString("N0");

        // skill levels
        farmerSkillLevel.text = pData.AverageFarmerSkill.ToString("N0");
        minerSkillLevel.text = pData.AverageMinerSkill.ToString("N0");
        merchantSkillLevel.text = pData.AverageMerchantSkill.ToString("N0");
        adminSkillLevel.text = pData.AverageAdminSkill.ToString("N0");
        academicSkillLevel.text = pData.AverageAcademicSkill.ToString("N0");
        engineerSkillLevel.text = pData.AverageEngineerSkill.ToString("N0");

        // facilities
        farmCount.text = pData.FarmingLevel.ToString("N0");
        energyFacilityCount.text = pData.EnergyFacilityLevel.ToString("N0");
        mineCount.text = pData.MiningLevel.ToString("N0");
        factoryCount.text = pData.FactoryLevel.ToString("N0");
        academyCount.text = pData.AcademyLevel.ToString("N0");
        adminFacilityCount.text = pData.AdminLevel.ToString("N0");

        // staffing bars
        farmStaffing.fillAmount = pData.FarmsStaffed / pData.FarmingLevel;
        energyFacilityStaffing.fillAmount = pData.EnergyFacilitiesStaffed / pData.EnergyFacilityLevel;
        factoryStaffing.fillAmount = pData.FactoriesStaffed / pData.FactoryLevel;
        mineStaffing.fillAmount = pData.MinesStaffed / pData.MiningLevel;
        academyStaffing.fillAmount = pData.AcademiesStaffed / pData.AcademyLevel;
        adminStaffing.fillAmount = pData.AdminFacilitiesStaffed / pData.AdminLevel;

        farmEmployment.fillAmount = 1 - (pData.FarmsStaffed / pData.TotalFarmers);
        minerEmployment.fillAmount = 1 - (pData.MinesStaffed / pData.TotalMiners);
        adminEmployment.fillAmount = 1 - (pData.AdminFacilitiesStaffed / pData.TotalAdministrators);
        engineerEmployment.fillAmount = 1 - (pData.FactoriesStaffed / pData.TotalEngineers);
        academicEmployment.fillAmount = 1 - (pData.AcademiesStaffed / pData.TotalAcademics);


        // balances
        string foodDifference = pData.FoodDifference.ToString("N0") + ")";
        string foodStored = "";
        string foodPrefix = " (";

        if (pData.FoodStored >= 1000)
            foodStored = (pData.FoodStored / 1000f).ToString("N0") + "K";
        else
            foodStored = pData.FoodStored.ToString("N0");

        if (pData.FoodDifference > 0)
        {
            foodPrefix = "<color=green> (+";
        }
        else
        {
            foodPrefix = "<color=red> (";
        }
        foodBalance.text = foodStored + foodPrefix + foodDifference +"</color>";

        string energyDifference = pData.EnergyDifference.ToString("N0") + ")";
        string energyStored = "";
        string energyPrefix = " (";

        if (pData.EnergyStored >= 1000)
            energyStored = (pData.EnergyStored / 1000f).ToString("N0") + "K";
        else
            energyStored = pData.EnergyStored.ToString("N0");

        if (pData.EnergyDifference > 0)
        {
            
            energyPrefix = "<color=green> (+";
        }
        else
        {     
            energyPrefix = "<color=red> (";         
        }

        energyBalance.text = energyStored + energyPrefix + energyDifference + "</color>";

        string basicDifference = pData.BasicTotalDifference.ToString("N0") + ")";
        string basicStored = "";
        string basicPrefix = "(";

        if (pData.BasicStored >= 1000)
            basicStored = (pData.BasicStored / 1000f).ToString("N0") + "K";
        else
            basicStored = pData.BasicStored.ToString("N0");

        if (pData.BasicTotalDifference > 0)
        {

            basicPrefix = "<color=green> (+";
        }
        else
        {
            basicPrefix = "<color=red> (";
        }
        basicBalance.text = basicStored + basicPrefix + basicDifference + "</color>";

        string heavyDifference = pData.HeavyTotalDifference.ToString("N0") + ")";
        string heavyStored = "";
        string heavyPrefix = "(";

        if (pData.HeavyStored >= 1000)
            heavyStored = (pData.HeavyStored / 1000f).ToString("N0") + "K";
        else
            heavyStored = pData.HeavyStored.ToString("N0");

        if (pData.HeavyTotalDifference > 0)
        {

            heavyPrefix = "<color=green> (+";
        }
        else
        {
            heavyPrefix = "<color=red> (";
        }
        heavyBalance.text = heavyStored + heavyPrefix + heavyDifference + "</color>";

        string rareDifference = pData.RareTotalDifference.ToString("N0") + ")";
        string rareStored = "";
        string rarePrefix = "(";

        if (pData.RareStored >= 1000)
            rareStored = (pData.RareStored / 1000f).ToString("N0") + "K";
        else
            rareStored = pData.RareStored.ToString("N0");

        if (pData.RareTotalDifference > 0)
        {

            rarePrefix = "<color=green> (+";
        }
        else
        {
            rarePrefix = "<color=red> (";
        }
        rareBalance.text = rareStored + rarePrefix + rareDifference + "</color>";

        adminOutput.text = pData.TotalAdmin.ToString("N0");

    }
}
