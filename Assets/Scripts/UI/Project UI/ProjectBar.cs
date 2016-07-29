using UnityEngine;
using UnityEngine.UI;
using Managers;
using Projects;
using Constants;
using TMPro;
using System.Collections.Generic;

class ProjectBar : MonoBehaviour
{
    private UIManager uiManagerRef;
    private RectTransform projectBarRect;
    private GameData gameDataRef;
    private GameObject createUI;
    private Button createButton;
    private Button stretchedCreateButton;
    private GridLayoutGroup newProjectButtonGrid;
    private GridLayoutGroup activeProjectButtonGrid;
    public GameObject ProjectButton;
    public GameObject ActiveProjectButton;
    public GameObject NoActiveProjectAlert;
    private TextMeshProUGUI projectCreateButtonText; 
    private List<GameObject> projectButtonList = new List<GameObject>();
    private List<GameObject> activeProjectButtonList = new List<GameObject>();
    public bool listIsInitialized = false;
    private bool buttonIsExpanded = false;

    bool createMode = false; // is the project bar in create mode?
    

    void Awake()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        createUI = GameObject.Find("UI_ProjectScreen/Create_pressed_Types");
        projectBarRect = gameObject.GetComponent<RectTransform>();
        createButton = transform.Find("Active_Project_Container_Grid/Create").GetComponent<Button>();
        stretchedCreateButton = transform.Find("Create_pressed_Types/Create_Stretched").GetComponent<Button>();
        projectCreateButtonText = transform.Find("Active_Project_Container_Grid/Create/Text").GetComponent<TextMeshProUGUI>();
        newProjectButtonGrid = GameObject.Find("Create_pressed_Types_Grid").GetComponent<GridLayoutGroup>();
        activeProjectButtonGrid = GameObject.Find("Active_Project_Container_Grid").GetComponent<GridLayoutGroup>();

        // set up delegates for the buttons
        createButton.onClick.AddListener(delegate { UpdateProjectBarMode(); }); // initiate new game setup screen
        stretchedCreateButton.onClick.AddListener(delegate { UpdateProjectBarMode(); }); // initiate new game setup screen
        createUI.SetActive(false); // initially set create column to false
        newProjectButtonGrid.enabled = true;
    }

    void Update()
    {
        if (uiManagerRef.RequestProjectBarGraphicRefresh)
        {
            InitializeNewProjectList();
            InitializeActiveProjectList();
            uiManagerRef.RequestProjectBarGraphicRefresh = false;
        }

        buttonIsExpanded = false;

        foreach(GameObject button in projectButtonList)
        {
            if (button.GetComponent<ProjectButton>().IsExpanded)
               buttonIsExpanded = true;
        }

        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy || gameDataRef.CivList[0].Leader.ActionPoints < Constant.APsRequiredForProject) // close the bar if open when in galaxy mode or if emperor has no APs left
        {
            createMode = false;
            createUI.SetActive(false);
            projectCreateButtonText.color = Color.yellow;
            if (gameDataRef.CivList[0].Leader.ActionPoints < Constant.APsRequiredForProject)
                projectCreateButtonText.text = "Insufficient APs";
            else
                projectCreateButtonText.text = "Galaxy View";

            ClearNewProjectList();
        }
        else
        {
            projectCreateButtonText.color = Color.white;
            projectCreateButtonText.text = "Create Project";
        }

        if (buttonIsExpanded)
            newProjectButtonGrid.enabled = false;
        else
        {
            newProjectButtonGrid.enabled = true;
        }
    }

    void UpdateProjectBarMode()
    {
        if (!uiManagerRef.ModalIsActive)
        {
            if (!createMode)
            {
                createMode = true;
                createUI.SetActive(true);
                PopulateProjectBar();
            }
            else
            {
                createMode = false;
                createUI.SetActive(false);
                ClearNewProjectList();
                newProjectButtonGrid.enabled = true;
            }
        }      
    }

    private void PopulateProjectBar()
    {
        InitializeNewProjectList();
        InitializeActiveProjectList();
    }

    public void InitializeActiveProjectList()
    {
        ClearActiveProjectList(); // clear out the list
        activeProjectButtonGrid.enabled = true;
        if (gameDataRef.CivList[0].ActiveProjects.Count > 0)
        {
            NoActiveProjectAlert.SetActive(false);
            List<Project> activeProjectList = gameDataRef.CivList[0].ActiveProjects;
            foreach (Project pData in activeProjectList)
            {
                if (pData.ActivateProject == false)
                    AddActiveProjectButton(pData);
            }
        }
        else
            NoActiveProjectAlert.SetActive(true);
    }

    public void InitializeNewProjectList()
    {

        ClearNewProjectList(); // clear out the list
        newProjectButtonGrid.enabled = true; // to properly set the buttons
        if (gameDataRef.CivList[0].Leader.ActionPoints > 0)
        {
            List<Project> projectList = new List<Project>();
            foreach (Project pData in gameDataRef.ProjectDataList)
            {
                if (pData.IsActionValid(uiManagerRef.ViewLevel))
                {
                    string type = pData.Type.ToString().ToUpper();
                    string level = uiManagerRef.PrimaryViewMode.ToString().ToUpper();

                    if (type == level)
                        AddNewProjectButton(pData);
                }
            }
        }
        listIsInitialized = true;
    }

    private void AddNewProjectButton(Project pData)
    {
        GameObject go = Instantiate(ProjectButton) as GameObject;
        go.SetActive(true);
        go.name = pData.ID;
        ProjectButton PB = go.GetComponent<ProjectButton>();
        PB.SetName(pData.Name);
        PB.SetID(pData.ID);
        PB.SetIcon(pData.IconName);
        PB.SetDescription(pData.Description);
        go.transform.SetParent(ProjectButton.transform.parent);
        go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        projectButtonList.Add(go);
    }

    private void AddActiveProjectButton(Project pData)
    {
        GameObject go = Instantiate(ActiveProjectButton) as GameObject;
        go.SetActive(true);
        go.name = pData.ID;
        ActiveProjectButton PB = go.GetComponent<ActiveProjectButton>();
        PB.SetName(pData.Name);
        PB.SetID(pData.ID);
        PB.SetProjectUID(pData.UniqueID);
        PB.SetIcon(pData.IconName);
        PB.SetDescription(pData.Description);
        go.transform.SetParent(ActiveProjectButton.transform.parent);
        go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        activeProjectButtonList.Add(go);
    }

    public void ClearActiveProjectList()
    {
        foreach (GameObject go in activeProjectButtonList)
        {
            Destroy(go);
        }
        activeProjectButtonList.Clear();
    }

    public void ClearNewProjectList()
    {
        foreach (GameObject go in projectButtonList)
        {
            Destroy(go);
        }
        projectButtonList.Clear();
    }

}
