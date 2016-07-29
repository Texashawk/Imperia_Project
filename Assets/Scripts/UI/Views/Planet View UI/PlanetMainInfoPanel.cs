using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Managers;
using CharacterObjects;
using CivObjects;
using TMPro;

public class PlanetMainInfoPanel : MonoBehaviour
{

    TextMeshProUGUI PlanetName;
    TextMeshProUGUI SecondaryInfo;
    TextMeshProUGUI Population;
    UIManager uiManagerRef;
    TextMeshProUGUI ADM;
    TextMeshProUGUI DevelopmentLevel;
    TextMeshProUGUI Type;
    Image planetRank;
    Image CivRulingHouse;
    Image HoldingHouse;
    GraphicAssets gAssetData;
    GameData gDataRef;

	void Start ()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        gAssetData = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();

        CivRulingHouse = transform.Find("House").GetComponent<Image>();
        planetRank = transform.Find("Title_Bar/Rank").GetComponent<Image>();
        HoldingHouse = transform.Find("House_Holding").GetComponent<Image>();       
        PlanetName = transform.Find("Title_Bar/Title").GetComponent<TextMeshProUGUI>();
        SecondaryInfo = transform.Find("Subtitle").GetComponent<TextMeshProUGUI>();
        Population = transform.Find("Stats_Container_Grid/Population/Counter_Population").GetComponent<TextMeshProUGUI>();
        ADM = transform.Find("Stats_Container_Grid/ADM/Counter_ADM").GetComponent<TextMeshProUGUI>();
        DevelopmentLevel = transform.Find("Stats_Container_Grid/Development/Counter_Development").GetComponent<TextMeshProUGUI>();
        Type = transform.Find("Stats_Container_Grid/Type/Counter_Type").GetComponent<TextMeshProUGUI>();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet && uiManagerRef.selectedPlanet != null)
        {
            PlanetName.text = uiManagerRef.selectedPlanet.Name;
            SecondaryInfo.text = uiManagerRef.selectedPlanet.System.Name + " System | " + uiManagerRef.selectedPlanet.System.Province.Name + " Province";
            Population.text = uiManagerRef.selectedPlanet.TotalPopulation.ToString("N0") + " M";
            ADM.text = uiManagerRef.selectedPlanet.TotalAdmin.ToString("N0");
            DevelopmentLevel.text = uiManagerRef.selectedPlanet.AverageDevelopmentLevel.ToString("N0");
            Type.text = uiManagerRef.selectedPlanet.Type.ToString();

            // populate the planet rank
            if (uiManagerRef.selectedPlanet.Rank == StellarObjects.PlanetData.ePlanetRank.EstablishedColony)
                planetRank.sprite = gAssetData.PlanetRankList.Find(p => p.name == "Icon_Rank_Viceroy");
            else if (uiManagerRef.selectedPlanet.Rank == StellarObjects.PlanetData.ePlanetRank.SystemCapital)
                planetRank.sprite = gAssetData.PlanetRankList.Find(p => p.name == "Icon_Rank_SY_Governor");
            else if (uiManagerRef.selectedPlanet.Rank == StellarObjects.PlanetData.ePlanetRank.ProvinceCapital)
                planetRank.sprite = gAssetData.PlanetRankList.Find(p => p.name == "Icon_Rank_PR_Governor");
            else if (uiManagerRef.selectedPlanet.Rank == StellarObjects.PlanetData.ePlanetRank.ImperialCapital)
                planetRank.sprite = gAssetData.PlanetRankList.Find(p => p.name == "Icon_Rank_Emperor");
            else
                planetRank.enabled = false;

            if (gAssetData.EmpireCrestList.Find(p => p.name == uiManagerRef.selectedPlanet.Owner.CrestFile) != null)
            {
                CivRulingHouse.sprite = gAssetData.EmpireCrestList.Find(p => p.name == uiManagerRef.selectedPlanet.Owner.CrestFile);
            }

            House owningHouse = HelperFunctions.DataRetrivalFunctions.GetHouse(uiManagerRef.selectedPlanet.HouseIDHolding);
            if (owningHouse != null)
            {
                HoldingHouse.sprite = gAssetData.HouseCrestList.Find(p => p.name == owningHouse.BannerID);
            }
        }
    }
}
