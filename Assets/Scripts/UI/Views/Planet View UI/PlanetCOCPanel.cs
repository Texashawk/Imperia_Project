using UnityEngine;
using UnityEngine.UI;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using HelperFunctions;
using Tooltips;
using TMPro;

public class PlanetCOCPanel : MonoBehaviour {

    /// references
    GameData gDataRef;
    UIManager uiManagerRef;
    GalaxyView gScreenRef;
    GraphicAssets gAssetData;
    PlanetData pData;
    StarData sData;
    ViewManager.eViewLevel viewLevel = ViewManager.eViewLevel.Galaxy;

    // public game objects
    public GameObject Viceroy;
    public GameObject SystemGovernor;
    public GameObject ProvinceGovernor;
    public GameObject DomesticPrime;
    public GameObject Emperor;
    //public GameObject TotalRevenue;
    //public GameObject ViceroyTaxCut;
    //public GameObject SystemGovernorTaxCut;
    //public GameObject ProvinceGovernorTaxCut;
    //public GameObject DomesticPrimeTaxCut;
    //public GameObject EmperorTaxCut;

    // private components
    Image viceroy;
    Image systemGovernor;
    Image provinceGovernor;
    Image domesticPrime;
    Image emperor;
    Image viceroyRelationship;
    Image systemGovernorRelationship;
    Image provinceGovernorRelationship;
    Image domesticPrimeRelationship;
    Image viceroyHouse;
    Image systemGovernorHouse;
    Image provinceGovernorHouse;
    Image domesticPrimeHouse;
    Image emperorHouse;
    //TextMeshProUGUI totalRevenue;
    //TextMeshProUGUI viceroyRevenue;
    //TextMeshProUGUI sysGovRevenue;
    //TextMeshProUGUI provGovRevenue;
    //TextMeshProUGUI domPrimeRevenue;
    //TextMeshProUGUI emperorRevenue;

    void Awake()
    {

        // references
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        gAssetData = GameObject.Find("GameManager").GetComponent<GraphicAssets>();

        // asset pulls
        viceroy = Viceroy.transform.Find("Portrait_Container/Portrait").GetComponent<Image>();
        systemGovernor = SystemGovernor.transform.Find("Portrait_Container/Portrait").GetComponent<Image>();
        provinceGovernor = ProvinceGovernor.transform.Find("Portrait_Container/Portrait").GetComponent<Image>();
        domesticPrime = DomesticPrime.transform.Find("Portrait_Container/Portrait").GetComponent<Image>();
        emperor = Emperor.transform.Find("Portrait_Container/Portrait").GetComponent<Image>();
        viceroyRelationship = Viceroy.transform.Find("Portrait_Container/Icon_Relationship").GetComponent<Image>();
        systemGovernorRelationship = SystemGovernor.transform.Find("Portrait_Container/Icon_Relationship").GetComponent<Image>();
        provinceGovernorRelationship = ProvinceGovernor.transform.Find("Portrait_Container/Icon_Relationship").GetComponent<Image>();
        domesticPrimeRelationship = DomesticPrime.transform.Find("Portrait_Container/Icon_Relationship").GetComponent<Image>();
        viceroyHouse = Viceroy.transform.Find("Portrait_Container/Icon_House_BG/Icon_House").GetComponent<Image>();
        systemGovernorHouse = SystemGovernor.transform.Find("Portrait_Container/Icon_House_BG/Icon_House").GetComponent<Image>();
        provinceGovernorHouse = ProvinceGovernor.transform.Find("Portrait_Container/Icon_House_BG/Icon_House").GetComponent<Image>();
        domesticPrimeHouse = DomesticPrime.transform.Find("Portrait_Container/Icon_House_BG/Icon_House").GetComponent<Image>();
        emperorHouse = Emperor.transform.Find("Portrait_Container/Icon_House_BG/Icon_House").GetComponent<Image>();

        //totalRevenue = TotalRevenue.GetComponent<TextMeshProUGUI>();
        //viceroyRevenue = ViceroyTaxCut.GetComponent<TextMeshProUGUI>();
        //sysGovRevenue = SystemGovernorTaxCut.GetComponent<TextMeshProUGUI>();
        //provGovRevenue = ProvinceGovernorTaxCut.GetComponent<TextMeshProUGUI>();
        //domPrimeRevenue = DomesticPrimeTaxCut.GetComponent<TextMeshProUGUI>();
        //emperorRevenue = EmperorTaxCut.GetComponent<TextMeshProUGUI>();

    }

    void Update()
    {
        // check for updated view
        if (viewLevel != uiManagerRef.ViewLevel)
        {
            viewLevel = uiManagerRef.ViewLevel;
            UpdateCOCPanelInfo();           
        }      
    }

    // see if anything (and what specifically) needs to be changed within COC
    void CheckUpdateables()
    {
        if (uiManagerRef.selectedPlanet != null)
        {
            if (pData == null)
            {
                pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                //UpdateCOCPanelInfo();
            }

            else if (pData != null)
            {
                if (pData != gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData)
                {
                    pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                    //UpdateCOCPanelInfo();
                }
            }
        }

        if (uiManagerRef.selectedSystem != null)
        {
            if (sData == null)
            {
                sData = gScreenRef.GetSelectedStar().GetComponent<Star>().starData;
                //UpdateCOCPanelInfo();
            }

            else if (sData != null)
            {
                if (sData != gScreenRef.GetSelectedStar().GetComponent<Star>().starData)
                {
                    sData = gScreenRef.GetSelectedStar().GetComponent<Star>().starData;
                    //UpdateCOCPanelInfo();
                }
            }
        }
    }

    void UpdateCOCPanelInfo()
    {
        CheckUpdateables();
        if (pData != null)
        {
            //totalRevenue.text = pData.GrossPlanetaryProduct.ToString("N0") + "bn";
        }


        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
        {
            if (pData.Viceroy != null)
            {
                Viceroy.SetActive(true);
                viceroy.sprite = gAssetData.CharacterList.Find(p => p.name == pData.Viceroy.PictureID);
                if (gAssetData.HouseIconList.Exists(p => p.name == pData.Viceroy.AssignedHouse.IconID))
                    viceroyHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == pData.Viceroy.AssignedHouse.IconID);
                else
                    viceroyHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == "Icon_House_Hawken");
                viceroyRelationship.sprite = gAssetData.RelationshipIconList.Find(p => p.name == pData.Viceroy.Relationships[pData.Owner.LeaderID].RelationshipIcon);
                //viceroyRevenue.text = pData.Owner.ViceroyBaseTaxCut.ToString("P0") + " (" + (pData.GrossPlanetaryProduct * pData.Owner.ViceroyBaseTaxCut).ToString("N0") + "bn)";
                viceroy.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(pData.Viceroy, (viceroy.transform.GetComponent<RectTransform>().rect.width / 5f)); // set up the tooltip
                viceroy.transform.GetComponent<CharacterScreenActivation>().InitializeData(pData.Viceroy);
            }
            else
            {
                //viceroyRevenue.text = "N/A";
            }
        }
        else
            Viceroy.SetActive(false);

        if (sData.Governor != null)
        {
            systemGovernor.sprite = gAssetData.CharacterList.Find(p => p.name == sData.Governor.PictureID);
            systemGovernorRelationship.sprite = gAssetData.RelationshipIconList.Find(p => p.name == sData.Governor.Relationships[sData.OwningCiv.LeaderID].RelationshipIcon);
            if (gAssetData.HouseIconList.Exists(p => p.name == sData.Governor.AssignedHouse.IconID))
                systemGovernorHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == sData.Governor.AssignedHouse.IconID);
            else
                systemGovernorHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == "Icon_House_Hawken");
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
            {
                //sysGovRevenue.text = pData.Owner.SystemGovernorBaseTaxCut.ToString("P0") + " (" + (pData.GrossPlanetaryProduct * pData.Owner.SystemGovernorBaseTaxCut).ToString("N0") + "bn)";
            }
           
            systemGovernor.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(sData.Governor, (systemGovernor.transform.GetComponent<RectTransform>().rect.width / 5f)); // set up the tooltip
            systemGovernor.transform.GetComponent<CharacterScreenActivation>().InitializeData(sData.Governor);
        }
        else
        {
            //sysGovRevenue.text = "N/A";
        }

        if (sData.Province.Governor != null)
        {
            provinceGovernor.sprite = gAssetData.CharacterList.Find(p => p.name == sData.Province.Governor.PictureID);
            provinceGovernorRelationship.sprite = gAssetData.RelationshipIconList.Find(p => p.name == sData.Province.Governor.Relationships[sData.OwningCiv.LeaderID].RelationshipIcon);
            if (gAssetData.HouseIconList.Exists(p => p.name == sData.Province.Governor.AssignedHouse.IconID))
                provinceGovernorHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == sData.Province.Governor.AssignedHouse.IconID);
            else
                provinceGovernorHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == "Icon_House_Hawken");
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
            {
                //provGovRevenue.text = pData.Owner.ProvinceGovernorBaseTaxCut.ToString("P0") + " (" + (pData.GrossPlanetaryProduct * pData.Owner.ProvinceGovernorBaseTaxCut).ToString("N0") + "bn)";
            }
           
            provinceGovernor.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(sData.Province.Governor, (provinceGovernor.transform.GetComponent<RectTransform>().rect.width / 5f)); // set up the tooltip
            provinceGovernor.transform.GetComponent<CharacterScreenActivation>().InitializeData(sData.Province.Governor);
        }
        else
        {
            //provGovRevenue.text = "N/A";
        }

        if (DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime) != null)
        {
            domesticPrime.sprite = gAssetData.CharacterList.Find(p => p.name == DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime).PictureID);
            domesticPrimeRelationship.sprite = gAssetData.RelationshipIconList.Find(p => p.name == DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime).Relationships[sData.OwningCiv.LeaderID].RelationshipIcon);
            if (gAssetData.HouseIconList.Exists(p => p.name == DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime).AssignedHouse.IconID))
                domesticPrimeHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime).AssignedHouse.IconID);
            else
                domesticPrimeHouse.sprite = gAssetData.HouseIconList.Find(p => p.name == "Icon_House_Hawken");
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
            {
                //domPrimeRevenue.text = pData.Owner.DomesticPrimeBaseTaxCut.ToString("P0") + " (" + (pData.GrossPlanetaryProduct * pData.Owner.DomesticPrimeBaseTaxCut).ToString("N0") + "bn)";
            }
            
            domesticPrime.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime), (domesticPrime.transform.GetComponent<RectTransform>().rect.width / 5f)); // set up the tooltip
            domesticPrime.transform.GetComponent<CharacterScreenActivation>().InitializeData(DataRetrivalFunctions.GetPrime(CharacterObjects.Character.eRole.DomesticPrime));
        }
        else
        {
            //domPrimeRevenue.text = "N/A";
        }

        if (sData.OwningCiv.Leader != null)
        {
            emperor.sprite = gAssetData.CharacterList.Find(p => p.name == sData.OwningCiv.Leader.PictureID);
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
            {
                //emperorRevenue.text = pData.Owner.EmperorBaseTaxCut.ToString("P0") + " (" + (pData.GrossPlanetaryProduct * pData.Owner.EmperorBaseTaxCut).ToString("N0") + "bn)";
            }
            emperor.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(sData.OwningCiv.Leader, (emperor.transform.GetComponent<RectTransform>().rect.width / 5f)); // set up the tooltip
        }
        else
        {
            //emperorRevenue.text = "N/A";
        }

    }
}
