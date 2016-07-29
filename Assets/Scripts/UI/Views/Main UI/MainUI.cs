using UnityEngine;
using Managers;

public class MainUI : MonoBehaviour {

    private GameObject selectedItemPanel; // reference for the selected item panel
    private GameObject planetInfoPanel; // reference for the planet info panel
    public GameObject COCPanel;
    private UIManager uiManagerRef; // UI Manager reference
    private GameObject eScroll;
    private ViewManager.eViewLevel currentViewLevel = ViewManager.eViewLevel.Planet;
                   
    void Start ()
    {
        selectedItemPanel = GameObject.Find("UI_SY_Planet_Name"); // selected item panel
        planetInfoPanel = GameObject.Find("UI_PL_Planet_Name");
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        eScroll = GameObject.Find("Event ScrollView");
        //COCPanel.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        SetSelectedItemPanelState();
        SetEventViewState();
       
	}

    void SetSelectedItemPanelState()
    {
        // Show the selected item panel
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
        {
            selectedItemPanel.SetActive(false);
            planetInfoPanel.SetActive(false);
            COCPanel.SetActive(false);
        }
        else if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
        {
            selectedItemPanel.SetActive(true);
            planetInfoPanel.SetActive(false);
            COCPanel.SetActive(true);
        }
        else if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.System)
        {
            selectedItemPanel.SetActive(true);
            planetInfoPanel.SetActive(false);
            if (uiManagerRef.selectedSystem.IsOccupied)
                COCPanel.SetActive(true);
            else
                COCPanel.SetActive(false);
        }
        else if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
        {
            selectedItemPanel.SetActive(false);
            planetInfoPanel.SetActive(true);
            if (uiManagerRef.selectedPlanet.IsInhabited)
                COCPanel.SetActive(true);
            else
                COCPanel.SetActive(false);
        }       
    }

    void SetEventViewState()
    {
        // Show the selected item panel
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy || uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
        {
            eScroll.SetActive(false);
        }
        else
        {
            eScroll.SetActive(false);
        }
    }
}
