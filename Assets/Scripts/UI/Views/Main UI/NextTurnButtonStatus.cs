using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class NextTurnButtonStatus : MonoBehaviour, IPointerClickHandler
{

    TurnEngine tEngineData;
    GameData gData;
    Button thisButton;
    TextMeshProUGUI dateText;

	// Use this for initialization
	void Start ()
    {
        tEngineData = GameObject.Find("GameManager").GetComponent<TurnEngine>(); // reference to the turn engine status
        gData = GameObject.Find("GameManager").GetComponent<GameData>(); // reference to the turn engine status
        thisButton = gameObject.GetComponent<Button>();
        dateText = gameObject.transform.Find("BTN_End_Turn/Date").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tEngineData.TurnGenerationActive)
        {
            thisButton.interactable = false;
            dateText.text = "PROCESSING"; // text shown when processing a new turn
        }

        else
        {
            thisButton.interactable = true;
            dateText.text = gData.GameDate.ToString("F1");
        }

        
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (thisButton.interactable)
            tEngineData.NewTurnRequest();
    }
}
