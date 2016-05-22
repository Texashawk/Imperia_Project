using UnityEngine;
using CharacterObjects;
using TMPro;

public class EmperorInfoPanel : MonoBehaviour {

    TextMeshProUGUI AP;
    GameData gameDataRef;

	// Use this for initialization
	void Start ()

    {
        AP = gameObject.transform.Find("Action Points").GetComponent<TextMeshProUGUI>();
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
    }
}
