using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlanetObjects;
using StellarObjects;
using CivObjects;
using HelperFunctions;
using System.Collections.Generic;
using Managers;

namespace Assets.Scripts.UI
{   
    public class SelectedStellarItemPanel : MonoBehaviour
    {
        private TextMeshProUGUI selectedItemName;
        private TextMeshProUGUI selectedItemSecondaryInfo;
        private UIManager uiManagerRef;
        private GlobalGameData gDataRef;
        private GameObject selectedItemPanel;

        void Awake()
        {
            selectedItemName = GameObject.Find("Selected Item Line").GetComponent<TextMeshProUGUI>();
            selectedItemSecondaryInfo = GameObject.Find("Secondary Header Line").GetComponent<TextMeshProUGUI>();
            uiManagerRef = GameObject.Find("UI Engine").GetComponent<UIManager>();
            gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            selectedItemPanel = GameObject.Find("Selected Item Panel");
        }

        void Update()
        {
            // update the panel information based on activation
            UpdatePanel();
                     
        }

        void UpdatePanel() // check for selected objects
        {
            if (uiManagerRef.ViewMode == UIManager.eViewMode.System && uiManagerRef.selectedSystem != null)
            {
                StarData starDat = uiManagerRef.selectedSystem;
                selectedItemName.text = uiManagerRef.selectedSystem.Name.ToUpper();  //show text
                string starType = "";
                if (starDat.starMultipleType == StarData.eStarMultiple.Binary)
                    starType = "BINARY ";
                if (starDat.starMultipleType == StarData.eStarMultiple.Trinary)
                    starType = "TRINARY ";
                selectedItemSecondaryInfo.text = "CLASS " + starDat.SpectralClass.ToString().ToUpper() + starDat.SecondarySpectralClass.ToString("N0") + " " + starType + "STAR";

                string owner = "NO";
                foreach (Civilization civ in gDataRef.CivList)
                {
                    List<StarData> civSystemList = DataRetrivalFunctions.GetCivSystemList(civ);

                    if (civSystemList.Exists(p => p.ID == starDat.ID))
                    {
                        owner = civ.Name.ToUpper();
                        if (starDat.AssignedProvinceID != "")
                            selectedItemSecondaryInfo.text += " | " + DataRetrivalFunctions.GetProvince(starDat.AssignedProvinceID).Name.ToUpper() + " PROVINCE";
                    }
                }
            }

            else if (uiManagerRef.ViewMode == UIManager.eViewMode.Planet && uiManagerRef.selectedPlanet != null)
            {
                selectedItemName.text = uiManagerRef.selectedPlanet.Name.ToUpper();  //show text
                selectedItemSecondaryInfo.text = "CLASS " + StringConversions.ConvertToRomanNumeral((int)(uiManagerRef.selectedPlanet.Size / 10)) + " " + HelperFunctions.StringConversions.ConvertPlanetEnum(uiManagerRef.selectedPlanet.Type).ToUpper();
                selectedItemSecondaryInfo.text += " | " + DataRetrivalFunctions.GetSystem(uiManagerRef.selectedPlanet.SystemID).Name.ToUpper() + " SYSTEM";
                if (uiManagerRef.selectedSystem.AssignedProvinceID != "")
                    selectedItemSecondaryInfo.text += " | " + DataRetrivalFunctions.GetProvince(DataRetrivalFunctions.GetSystem(uiManagerRef.selectedPlanet.SystemID).AssignedProvinceID).Name.ToUpper() + " PROVINCE";
            }
        }
    }
}
