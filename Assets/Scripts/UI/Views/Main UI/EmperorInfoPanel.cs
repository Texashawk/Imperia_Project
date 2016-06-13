using UnityEngine;
using CharacterObjects;
using HelperFunctions;
using TMPro;

public class EmperorInfoPanel : MonoBehaviour {

    TextMeshProUGUI AP;
    TextMeshProUGUI power;
    TextMeshProUGUI location;
    GameData gameDataRef;

	// Use this for initialization
	void Start ()

    {
        AP = gameObject.transform.Find("BTN_AP/AP_Text_Count").GetComponent<TextMeshProUGUI>();
        power = gameObject.transform.Find("Attributes/Power/Icon_Status/Power_Counter").GetComponent<TextMeshProUGUI>();
        location = transform.Find("Location/Text").GetComponent<TextMeshProUGUI>();
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
    }
	
	// Update is called once per frame
	void Update ()

    {
        UpdateEmperorInfoPanel();
	}

    void UpdateEmperorInfoPanel()
    {
        Emperor leader = HelperFunctions.DataRetrivalFunctions.GetCivilization("CIV0").PlayerEmperor;
        AP.text = leader.ActionPoints.ToString("N0");
        power.text = leader.Power.ToString("N0");
        location.text = DataRetrivalFunctions.GetPlanet(leader.PlanetLocationID).Name;
    }
}
