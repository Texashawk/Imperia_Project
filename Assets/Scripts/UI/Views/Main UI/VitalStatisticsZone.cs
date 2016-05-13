using UnityEngine;
using TMPro;

public class VitalStatisticsZone : MonoBehaviour {

    private TextMeshProUGUI gameDate;
    private TextMeshProUGUI emperorPoSup;
    private TextMeshProUGUI emperorBenevolentInfluence;
    private TextMeshProUGUI emperorTyrannicalInfluence;
    private TextMeshProUGUI emperorPower;
    private GameData gameDataRef;

    // Use this for initialization
    void Awake ()
    {
        // initialize all empire bar stats
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
        gameDate = GameObject.Find("GameDate").GetComponent<TextMeshProUGUI>();
        emperorPoSup = GameObject.Find("Popular Support").GetComponent<TextMeshProUGUI>();       
        emperorPower = GameObject.Find("Emperor Power").GetComponent<TextMeshProUGUI>();
        emperorBenevolentInfluence = GameObject.Find("Benevolent Influence").GetComponent<TextMeshProUGUI>();    
        emperorTyrannicalInfluence = GameObject.Find("Tyrannical Influence").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update ()
    {
        UpdateVitalStatisticsZone();
	}

    void UpdateVitalStatisticsZone()
    {       
        gameDate.text = gameDataRef.GameDate.ToString("F1"); // display date     
        emperorPoSup.text = (HelperFunctions.DataRetrivalFunctions.GetCivilization("CIV0").PlayerEmperor.EmperorPoSup * 100).ToString("N0") + "%";
        emperorBenevolentInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivilization("CIV0").PlayerEmperor.BenevolentInfluence.ToString("N0");
        emperorPower.text = HelperFunctions.DataRetrivalFunctions.GetCivilization("CIV0").PlayerEmperor.EmperorPower.ToString("N0");
        emperorTyrannicalInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivilization("CIV0").PlayerEmperor.TyrannicalInfluence.ToString("N0");    
    }
}
