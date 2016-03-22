using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TechWindow : MonoBehaviour {

    private GameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventIntelSelect.AddListener(ActivateScienceMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GameData.eSubMode.Science)
            DisplayScienceScreen();
    }

    void DisplayScienceScreen()
    {
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), ActiveTechWindow, "Science Window");
    }

    public void ActivateScienceMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Science)
            gGameDataRef.uiSubMode = GameData.eSubMode.Science;
        Debug.Log("Science SubMode Selected");

    }

    private void ActiveTechWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the Science Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
