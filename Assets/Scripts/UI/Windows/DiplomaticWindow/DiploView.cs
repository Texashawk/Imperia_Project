using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DiploView : MonoBehaviour {

    private GameData gGameDataRef;
    public UnityEvent eventDiploSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventDiploSelect.AddListener(ActivateDiplomaticMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GameData.eSubMode.Diplomacy)
            DisplayDiplomacyScreen();
    }

    void DisplayDiplomacyScreen()
    {
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), DiploWindow, "Diplomacy Window");
    }

    public void ActivateDiplomaticMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Diplomacy)
            gGameDataRef.uiSubMode = GameData.eSubMode.Diplomacy;
        Debug.Log("Diplomatic SubMode Selected");

    }

    private void DiploWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the Diplomatic Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
