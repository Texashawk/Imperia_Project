using UnityEngine;
using Managers;

public class MainUI : MonoBehaviour {

    private GameObject selectedItemPanel; // reference for the selected item panel
    private UIManager uiManagerRef; // UI Manager reference
    private GameObject projectBar;
                   
    void Start ()
    {
        selectedItemPanel = GameObject.Find("Selected Item Panel"); // selected item panel
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        projectBar = GameObject.Find("Project Bar");
    }
	
	// Update is called once per frame
	void Update ()
    {
        SetSelectedItemPanelState();
        SetProjectBarState();
	}

    void SetSelectedItemPanelState()
    {
        // Show the selected item panel
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy || uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
        {
            selectedItemPanel.SetActive(false);
        }
        else
        {
            selectedItemPanel.SetActive(true);
        }
    }

    void SetProjectBarState()
    {
        // Show the selected item panel
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
        {
            projectBar.SetActive(false);
        }
        else
        {
            projectBar.SetActive(true);
        }
    }
}
