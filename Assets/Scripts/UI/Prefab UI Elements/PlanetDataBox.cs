using UnityEngine;
using UnityEngine.UI;
using HelperFunctions;
using StellarObjects;
using Tooltips;

public class PlanetDataBox : MonoBehaviour {

    PlanetData pData; // reference for planet data
    GlobalGameData gameDataRef; // to determine mode
    GraphicAssets graphicsDataRef; // for character pictures and other global pictures

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }

    public void PopulateDataBox(string pID)
    {
        pData = DataRetrivalFunctions.GetPlanet(pID);

        // show viceroy image if eligible
        string cID = DataRetrivalFunctions.GetPlanetViceroyID(pData.ID);
        if (cID != "none")
        {
            Transform vPanel = transform.Find("Viceroy Image");
            vPanel.gameObject.SetActive(true);
            string vID = DataRetrivalFunctions.GetCharacter(cID).PictureID;
            vPanel.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == vID);
            vPanel.GetComponent<CharacterTooltip>().InitializeTooltipData(DataRetrivalFunctions.GetCharacter(cID), -20f);
            vPanel.GetComponent<CharacterScreenActivation>().InitializeData(DataRetrivalFunctions.GetCharacter(cID));
        }
        else
        {
            transform.Find("Viceroy Image").gameObject.SetActive(false);
        }

        // moons only show if there are any
        if (pData.Moons > 0)
        {
            Transform mPanel = transform.Find("Moon Panel");
            mPanel.gameObject.SetActive(true);
            mPanel.transform.Find("Moon Count").GetComponent<Text>().text = pData.Moons.ToString("N0");
        }
        else
            transform.Find("Moon Panel").gameObject.SetActive(false);

        string isExporting = "";
        if (pData.TradeAmount != 0)
        {
            isExporting = "($" + pData.TradeAmount.ToString("N2") + ")";
        }
        
        transform.Find("Planet Name").GetComponent<Text>().text = pData.Name.ToUpper() + " " + isExporting; // assign the name to the text object child
        transform.Find("Planet Name").GetComponent<Text>().color = DataRetrivalFunctions.FindPlanetOwnerColor(pData);
        transform.Find("Planet Type").GetComponent<Text>().text = "CLASS " + StringConversions.ConvertToRomanNumeral((int)(pData.Size / 10)) + " " + StringConversions.ConvertPlanetEnum(pData.Type).ToUpper(); // assign the name to the text object child
        transform.Find("Planet Type").GetComponent<Text>().color = Color.yellow;
        transform.Find("Planet Status").GetComponent<Text>().text = DataRetrivalFunctions.FindOwnerName(pData);
        transform.Find("Planet Status").GetComponent<Text>().color = Color.cyan;

        // assign each child text value and color
        if (!gameDataRef.DebugMode) // if not in debug mode, just show the text
        {
            if (pData.SurfaceScanLevel >= 1.0f)
            {
                transform.Find("Energy Level").GetComponent<Text>().text = StringConversions.ConvertValueToDescription(pData.Energy); // assign the name to the text object child
                transform.Find("Energy Level").GetComponent<Text>().color = StringConversions.GetTextValueColor(pData.Energy);
            }
            if (pData.AtmosphereScanLevel >= 1.0f)
            {
                
                transform.Find("Bio Level").GetComponent<Text>().text = StringConversions.ConvertValueToDescription(pData.AdjustedBio); // assign the name to the text object child
                //transform.Find("Planet Size").GetComponent<Text>().color = HelperFunctions.StringConversions.GetTextValueColor(pData.Size);
                transform.Find("Bio Level").GetComponent<Text>().color = StringConversions.GetTextValueColor(pData.Bio);
            }

            if (pData.InteriorScanLevel >= 1.0f)
            {
                transform.Find("Rare Materials Level").GetComponent<Text>().text = StringConversions.ConvertValueToDescription(pData.RareMaterials); // assign the name to the text object child
                transform.Find("Alpha Materials Level").GetComponent<Text>().text = StringConversions.ConvertValueToDescription(pData.BasicMaterials); // assign the name to the text object child
                transform.Find("Heavy Materials Level").GetComponent<Text>().text = StringConversions.ConvertValueToDescription(pData.HeavyMaterials); // assign the name to the text object child
                transform.Find("Rare Materials Level").GetComponent<Text>().color = StringConversions.GetTextValueColor(pData.RareMaterials);
                transform.Find("Alpha Materials Level").GetComponent<Text>().color = StringConversions.GetTextValueColor(pData.BasicMaterials);
                transform.Find("Heavy Materials Level").GetComponent<Text>().color = StringConversions.GetTextValueColor(pData.HeavyMaterials);
            }

            if (pData.IntelLevel == 10)
            {
                transform.Find("Planet Size").GetComponent<Text>().text = pData.AverageDevelopmentLevel.ToString("N1"); // assign the name to the text object child
                transform.Find("Treasury Level").GetComponent<Text>().text = StringConversions.ConvertFloatDollarToText(pData.GrossPlanetaryProduct); // assign the name to the text object child
                if (pData.GrossPlanetaryProduct < 0)
                {
                    transform.Find("Treasury Level").GetComponent<Text>().color = Color.red;
                }
                else
                {
                    transform.Find("Treasury Level").GetComponent<Text>().color = Color.green;
                }
                string pChangeSign = "";
                if (pData.PopulationChangeLastTurn > 0)
                    pChangeSign = "+";
                transform.Find("Total Admin Level").GetComponent<Text>().text = pData.TotalAdmin.ToString("N0"); // assign the name to the text object child
                transform.Find("Manufacturing Level").GetComponent<Text>().text = pData.FactoriesStaffed.ToString("N1") + "/" + pData.ManufacturingLevel.ToString("N0"); // assign the name to the text object child
                transform.Find("Population Level").GetComponent<Text>().text = StringConversions.ConvertIntToText(pData.TotalPopulation) + "(" + pChangeSign + HelperFunctions.StringConversions.ConvertIntToText(pData.PopulationChangeLastTurn) + ")"; // assign the name to the text object child
                transform.Find("Farming Level").GetComponent<Text>().text = pData.FarmsStaffed.ToString("N1") + "/" + pData.FarmingLevel.ToString("N0"); // assign the name to the text object child
                transform.Find("High Tech Level").GetComponent<Text>().text = pData.HighTechFacilitiesStaffed.ToString("N0") + "/" + pData.HighTechLevel.ToString("N0"); // assign the name to the text object child
                transform.Find("Science Level").GetComponent<Text>().text = pData.LabsStaffed.ToString("N1") + "/" + pData.ScienceLevel.ToString("N0"); // assign the name to the text object child
                transform.Find("Mining Level").GetComponent<Text>().text = pData.MinesStaffed.ToString("N1") + "/" + pData.MiningLevel.ToString("N0"); // assign the name to the text object child
                transform.Find("Government Level").GetComponent<Text>().text = pData.GovernmentFacilitiesStaffed.ToString("N1") + "/" + pData.GovernmentLevel.ToString("N0"); // assign the name to the text object child
                transform.Find("Posup Level").GetComponent<Text>().text = pData.PopularSupportLevel.ToString("P1"); // assign the name to the text object child
                transform.Find("Unemployment Level").GetComponent<Text>().text = pData.UnemploymentLevel.ToString("P1"); // assign the name to the text object child
            }

            if (!pData.IsInhabited)
            {
                transform.Find("Treasury Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Total Admin Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Manufacturing Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Population Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Farming Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("High Tech Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Science Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Mining Level").GetComponent<Text>().text = "-";
                transform.Find("Government Level").GetComponent<Text>().text = "-";
                transform.Find("Posup Level").GetComponent<Text>().text = "-"; // assign the name to the text object child
                transform.Find("Unemployment Level").GetComponent<Text>().text = "-";
            }
        }
        else // assign the actual value
        {      
            transform.Find("Energy Level").GetComponent<Text>().text = pData.Energy.ToString("N0"); // assign the name to the text object child
            transform.Find("Energy Level").GetComponent<Text>().color = StringConversions.GetTextValueColor(pData.Energy);    
            transform.Find("Bio Level").GetComponent<Text>().text = pData.AdjustedBio.ToString("N0") + "(" + pData.Bio.ToString("N0") + ")"; // assign the name to the text object child
            transform.Find("Planet Size").GetComponent<Text>().text = pData.AdjustedMaxHabitableTiles.ToString("N0") + "(" + pData.MaxHabitableTiles.ToString("N0") + ")"; // assign the name to the text object child            
            transform.Find("Rare Materials Level").GetComponent<Text>().text = pData.RareMaterials.ToString("N0"); // assign the name to the text object child
            transform.Find("Alpha Materials Level").GetComponent<Text>().text = pData.BasicMaterials.ToString("N0"); // assign the name to the text object child
            transform.Find("Heavy Materials Level").GetComponent<Text>().text = pData.HeavyMaterials.ToString("N0"); // assign the name to the text object child        
            transform.Find("Treasury Level").GetComponent<Text>().text = StringConversions.ConvertFloatDollarToText(pData.GrossPlanetaryProduct); // assign the name to the text object child
            if (pData.GrossPlanetaryProduct < 0)
            {
                transform.Find("Treasury Level").GetComponent<Text>().color = Color.red;
            }
            else
            {
                transform.Find("Treasury Level").GetComponent<Text>().color = Color.green;
            }
            transform.Find("Total Admin Level").GetComponent<Text>().text = pData.TotalAdmin.ToString("N0"); // assign the name to the text object child
            transform.Find("Manufacturing Level").GetComponent<Text>().text = pData.ManufacturingLevel.ToString("N0"); // assign the name to the text object child
            transform.Find("Population Level").GetComponent<Text>().text = StringConversions.ConvertIntToText(pData.TotalPopulation); // assign the name to the text object child
            
        }
        transform.Find("Scan Level").GetComponent<Text>().text = pData.ScanLevel.ToString("P0"); // assign the name to the text object child

        // draw traits
        if (pData.PlanetTraits.Count > 0)
        {
            transform.Find("Planet Trait 1").GetComponent<Text>().text = pData.PlanetTraits[0].Name.ToUpper();
            transform.Find("Planet Trait 1").GetComponent<Text>().color = Color.white;
        }
        else
            transform.Find("Planet Trait 1").GetComponent<Text>().color = Color.grey;

        if (pData.PlanetTraits.Count > 1)
            transform.Find("Planet Trait 2").GetComponent<Text>().text = pData.PlanetTraits[1].Name.ToUpper();
        else
            transform.Find("Planet Trait 2").GetComponent<Text>().text = "";
    }
}
