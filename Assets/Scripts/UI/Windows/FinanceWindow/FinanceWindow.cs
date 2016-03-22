using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class FinanceWindow : MonoBehaviour {

    private GameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventIntelSelect.AddListener(ActivateFinanceMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GameData.eSubMode.Finance)
            DisplayFinanceScreen();
    }

    void DisplayFinanceScreen()
    {
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), ActivateFinanceWindow, "Finance Window");
    }

    public void ActivateFinanceMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GameData.eSubMode.Finance)
            gGameDataRef.uiSubMode = GameData.eSubMode.Finance;
        Debug.Log("Finance SubMode Selected");

    }

    private void ActivateFinanceWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the Finance Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
