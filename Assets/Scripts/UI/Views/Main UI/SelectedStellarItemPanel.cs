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
        private GameData gDataRef;
        private GameObject selectedItemPanel;

        void Awake()
        {
            selectedItemName = transform.Find("Title_Bar/Title").GetComponent<TextMeshProUGUI>();
            selectedItemSecondaryInfo = transform.Find("Subtitle").GetComponent<TextMeshProUGUI>();
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
            gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            selectedItemPanel = gameObject;
        }

        void Update()
        {
            // update the panel information based on activation         
            UpdatePanel();                 
        }

        void UpdatePanel() // check for selected objects
        {
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.System && uiManagerRef.selectedSystem != null)
            {
                StarData starDat = uiManagerRef.selectedSystem;
                selectedItemName.text = uiManagerRef.selectedSystem.Name + " System";  //show text
                string starType = "";
                if (starDat.starMultipleType == StarData.eStarMultiple.Binary)
                    starType = "Binary ";
                if (starDat.starMultipleType == StarData.eStarMultiple.Trinary)
                    starType = "Trinary ";
                selectedItemSecondaryInfo.text = "Class " + starDat.SpectralClass.ToString() + starDat.SecondarySpectralClass.ToString("N0") + " " + starType + "Star";

                string owner = "No";
                foreach (Civilization civ in gDataRef.CivList)
                {
                    List<StarData> civSystemList = DataRetrivalFunctions.GetCivSystemList(civ);

                    if (civSystemList.Exists(p => p.ID == starDat.ID))
                    {
                        owner = civ.Name.ToUpper();
                        if (starDat.AssignedProvinceID != "")
                            selectedItemSecondaryInfo.text += " | " + DataRetrivalFunctions.GetProvince(starDat.AssignedProvinceID).Name + " Province";
                    }
                }
            }

            //else if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet && uiManagerRef.selectedPlanet != null)
            //{
            //    selectedItemName.text = uiManagerRef.selectedPlanet.Name;  //show text
            //    selectedItemSecondaryInfo.text = "Class " + StringConversions.ConvertToRomanNumeral((int)(uiManagerRef.selectedPlanet.Size / 10)) + " " + HelperFunctions.StringConversions.ConvertPlanetEnum(uiManagerRef.selectedPlanet.Type);
            //    selectedItemSecondaryInfo.text += " | " + DataRetrivalFunctions.GetSystem(uiManagerRef.selectedPlanet.SystemID).Name + " System";
            //    if (uiManagerRef.selectedSystem.AssignedProvinceID != "")
            //        selectedItemSecondaryInfo.text += " | " + DataRetrivalFunctions.GetProvince(DataRetrivalFunctions.GetSystem(uiManagerRef.selectedPlanet.SystemID).AssignedProvinceID).Name + " Province";
            //}
        }
    }
}
