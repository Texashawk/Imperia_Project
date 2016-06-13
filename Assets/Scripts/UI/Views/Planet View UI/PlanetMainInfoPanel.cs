using UnityEngine;
using System.Collections;
using Managers;
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
    GameData gDataRef;

	// Use this for initialization
	void Start ()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
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

        }

    }
}
