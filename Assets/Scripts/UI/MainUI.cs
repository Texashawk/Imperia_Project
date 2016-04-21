using UnityEngine;
using Managers;

public class MainUI : MonoBehaviour {

    private GameObject selectedItemPanel; // reference for the selected item panel
    private UIManager uiManagerRef; // UI Manager reference
                   
    void Start ()
    {
        selectedItemPanel = GameObject.Find("Selected Item Panel"); // selected item panel
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        SetSelectedItemPanelState();
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
}
