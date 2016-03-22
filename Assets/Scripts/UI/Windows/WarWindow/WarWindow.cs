using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class WarWindow : MonoBehaviour {

    private GameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventIntelSelect.AddListener(ActivateWarMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GameData.eSubMode.War)
            DisplayWarScreen();
    }

    void DisplayWarScreen()
    {
        //GUI.Label(new Rect(20, 90, 200, 25), "Submode: Intel View");
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), ActiveWarWindow, "War Window");
    }

    public void ActivateWarMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.War)
            gGameDataRef.uiSubMode = GameData.eSubMode.War;
        Debug.Log("War SubMode Selected");

    }

    private void ActiveWarWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the War Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
