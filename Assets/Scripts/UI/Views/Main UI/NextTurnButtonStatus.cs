using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NextTurnButtonStatus : MonoBehaviour, IPointerClickHandler
{

    TurnEngine tEngineData;
    Button thisButton;

	// Use this for initialization
	void Start ()
    {
        tEngineData = GameObject.Find("GameManager").GetComponent<TurnEngine>(); // reference to the turn engine status
        thisButton = gameObject.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tEngineData.TurnGenerationActive)
        {
            thisButton.interactable = false;
        }

        else
        {
            thisButton.interactable = true;
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (thisButton.interactable)
            tEngineData.NewTurnRequest();
    }
}
