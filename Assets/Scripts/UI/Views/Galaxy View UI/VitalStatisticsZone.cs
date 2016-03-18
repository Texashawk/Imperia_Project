using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VitalStatisticsZone : MonoBehaviour {

    private TextMeshProUGUI gameDate;
    private TextMeshProUGUI emperorPoSup;
    private TextMeshProUGUI emperorStatusText;
    private TextMeshProUGUI emperorBenevolentInfluence;
    private TextMeshProUGUI emperorTyrannicalInfluence;
    private TextMeshProUGUI emperorPower;
    private GlobalGameData gameDataRef;

    // Use this for initialization
    void Awake ()
    {
        // initialize all empire bar stats
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // get global game data (date, location, version, etc)
        gameDate = GameObject.Find("GameDate").GetComponent<TextMeshProUGUI>();
        emperorPoSup = GameObject.Find("Popular Support").GetComponent<TextMeshProUGUI>();       
        //emperorStatusText = GameObject.Find("Emperor Status").GetComponent<TextMeshProUGUI>();
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
       
        emperorPoSup.text = (HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").EmperorPoSup * 100).ToString("N0") + "%";
        emperorBenevolentInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").BenevolentInfluence.ToString("N0");
        //emperorStatusText.text = "EMPEROR " + HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").Name.ToUpper() + ", YOUR GLORIOUS REIGN BEGINS. MAY THE CELESTIAL EMPIRE LAST FOREVER.";
        emperorPower.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").Influence.ToString("N0");
        emperorTyrannicalInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").TyrannicalInfluence.ToString("N0");    
    }
}
