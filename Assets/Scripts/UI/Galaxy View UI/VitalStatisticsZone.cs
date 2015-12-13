using UnityEngine;
using UnityEngine.UI;

public class VitalStatisticsZone : MonoBehaviour {

    private Text gameDate;
    private Text emperorPoSup;
    private Text emperorAP;
    private Text emperorStatusText;
    private Text emperorBenevolentInfluence;
    private Text emperorPragmaticInfluence;
    private Text emperorTyrannicalInfluence;
    private Text emperorPower;
    private GlobalGameData gameDataRef;

    // Use this for initialization
    void Awake ()
    {
        // initialize all empire bar stats
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // get global game data (date, location, version, etc)
        gameDate = GameObject.Find("GameDate").GetComponent<Text>();
        emperorPoSup = GameObject.Find("Popular Support").GetComponent<Text>();
        emperorAP = GameObject.Find("Action Points").GetComponent<Text>();
        emperorStatusText = GameObject.Find("Emperor Status").GetComponent<Text>();
        emperorPower = GameObject.Find("Emperor Power").GetComponent<Text>();
        emperorBenevolentInfluence = GameObject.Find("Benevolent Influence").GetComponent<Text>();
        //emperorPragmaticInfluence = GameObject.Find("Pragmatic Influence").GetComponent<Text>();
        emperorTyrannicalInfluence = GameObject.Find("Tyrannical Influence").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update ()
    {
        UpdateVitalStatisticsZone();
	}

    void UpdateVitalStatisticsZone()
    {       
        gameDate.text = gameDataRef.GameDate.ToString("F1"); // display date
        emperorAP.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").ActionPoints.ToString("N0"); // get human leader
        emperorPoSup.text = (HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").EmperorPoSup * 100).ToString("N0") + "%";
        emperorBenevolentInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").BenevolentInfluence.ToString("N0");
        emperorStatusText.text = "EMPEROR " + HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").Name.ToUpper() + " I, YOUR GLORIOUS REIGN BEGINS. MAY THE CELESTIAL EMPIRE LAST FOREVER.";
        emperorPower.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").Influence.ToString("N0");
        //emperorPragmaticInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").PragmaticInfluence.ToString("N0");
        emperorTyrannicalInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").TyrannicalInfluence.ToString("N0");    
    }
}
