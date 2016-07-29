using UnityEngine;
using TMPro;

public class ShowGameCreationStats : MonoBehaviour
{
    GalaxyData gData;
    GameData gameData;
    TextMeshProUGUI planetsGenerated;
    TextMeshProUGUI housesGenerated;
    TextMeshProUGUI charactersGenerated;
    TextMeshProUGUI statusText;
    TurnEngine tEngineData;

    void Start()
    {
        gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        statusText = GameObject.Find("Status Text").GetComponent<TextMeshProUGUI>();
        gameData = GameObject.Find("GameManager").GetComponent<GameData>();
        planetsGenerated = GameObject.Find("Planets Generated").GetComponent<TextMeshProUGUI>();
        housesGenerated = GameObject.Find("Houses Generated").GetComponent<TextMeshProUGUI>();
        charactersGenerated = GameObject.Find("Characters Generated").GetComponent<TextMeshProUGUI>();
        tEngineData = GameObject.Find("GameManager").GetComponent<TurnEngine>();
    }

    // Update is called once per frame
    void Update()
    {
        planetsGenerated.text = "Stars Generated: " + gData.GalaxyStarDataList.Count.ToString("N0"); // test to see if it draws star counts
        housesGenerated.text = "Houses Founded: " + gameData.HouseList.Count.ToString("N0"); // test to see if it draws planet counts
        charactersGenerated.text = "Characters Born: " + gameData.CharacterList.Count.ToString("N0"); // test to see if it draws planet counts
        statusText.text = tEngineData.InitializationStatus;
    }
}
