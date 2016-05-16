using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.States;
using Assets.Scripts.Interfaces;

public class SetupNewGamePanel : MonoBehaviour
{

    private Button exitButton;
    private Button nextStepButton;
    private StateManager stateRef;

    // Use this for initialization
    void Start ()
    {
        exitButton = transform.Find("Exit Button").GetComponent<Button>();
        stateRef = GameObject.Find("GameManager").GetComponent<StateManager>();
        exitButton.onClick.AddListener(delegate {ExitGame(); }); // initiate new game setup screen
        nextStepButton = GameObject.Find("Move to Next Step Button").GetComponent<Button>();
        nextStepButton.onClick.AddListener(delegate { MoveToNextStep(); });
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void MoveToNextStep()
    {
        stateRef.activeState.Switch(); // the active state should be BeginState

    }

    void ExitGame()
    {
        Application.Quit();
    }
}
