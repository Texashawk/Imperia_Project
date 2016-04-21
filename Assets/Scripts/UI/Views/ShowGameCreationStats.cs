using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowGameCreationStats : MonoBehaviour
{

    GalaxyData gData;
    GameData gameData;
    Text planetsGenerated;
    Text housesGenerated;
    Text charactersGenerated;
    Text statusText;
    TurnEngine tEngineData;

    // Use this for initialization
    void Start()
    {
        gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        statusText = GameObject.Find("Status Text").GetComponent<Text>();
        gameData = GameObject.Find("GameManager").GetComponent<GameData>();
        planetsGenerated = GameObject.Find("Planets Generated").GetComponent<Text>();
        housesGenerated = GameObject.Find("Houses Generated").GetComponent<Text>();
        charactersGenerated = GameObject.Find("Characters Generated").GetComponent<Text>();
        tEngineData = GameObject.Find("GameManager").GetComponent<TurnEngine>();
    }

    // Update is called once per frame
    void Update()
    {
        planetsGenerated.text = "STARS GENERATED: " + gData.GalaxyStarDataList.Count.ToString("N0"); // test to see if it draws star counts
        housesGenerated.text = "HOUSES GENERATED: " + gameData.HouseList.Count.ToString("N0"); // test to see if it draws planet counts
        charactersGenerated.text = "CHARACTERS GENERATED: " + gameData.CharacterList.Count.ToString("N0"); // test to see if it draws planet counts
        statusText.text = tEngineData.InitializationStatus;
    }
}
