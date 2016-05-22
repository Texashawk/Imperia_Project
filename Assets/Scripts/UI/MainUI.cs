using UnityEngine;
using Managers;

public class MainUI : MonoBehaviour {

    private GameObject selectedItemPanel; // reference for the selected item panel
    private UIManager uiManagerRef; // UI Manager reference
    //private GameObject projectBar;
   // private ProjectScrollView pScroll;
    private GameObject eScroll;
    private ViewManager.eViewLevel currentViewLevel = ViewManager.eViewLevel.Planet;
                   
    void Start ()
    {
        selectedItemPanel = GameObject.Find("Selected Item Panel"); // selected item panel
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        //projectBar = GameObject.Find("Project Bar");
        //pScroll = GameObject.Find("Project Selection View").GetComponent<ProjectScrollView>();
        eScroll = GameObject.Find("Event ScrollView");
    }
	
	// Update is called once per frame
	void Update ()
    {
        SetSelectedItemPanelState();
        //SetProjectBarState();
        SetEventViewState();
        
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

    void SetEventViewState()
    {
        // Show the selected item panel
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy || uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
        {
            eScroll.SetActive(true);
        }
        else
        {
            eScroll.SetActive(false);
        }
    }

    void SetProjectBarState()
    {
        // Show the selected item panel
        //if (currentViewLevel != uiManagerRef.ViewLevel)
        //{
        //    currentViewLevel = uiManagerRef.ViewLevel;
        //    if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
        //    {
        //        if (projectBar.activeInHierarchy)
        //        {
        //            pScroll.listIsInitialized = false; // reset the list
        //            projectBar.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        projectBar.SetActive(true);
        //        projectBar.transform.localScale = new Vector3(.8f, .8f, 1f);
        //        pScroll.InitializeList();
        //    }
        //}       
    }
}
