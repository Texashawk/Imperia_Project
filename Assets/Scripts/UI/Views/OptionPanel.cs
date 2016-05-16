using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class OptionPanel : MonoBehaviour {

    enum eSubModePanel : int
    {
        NewGame,
        Credits,
        Options,
        LoadGame,
        None
    }

    int width = 0;
    int panelWidthAfterOpen = 0;
    int panelHeightAfterOpen = 0;
    float panelInitialHeight = 0;
    float panelInitialWidth = 0;

    Vector3 initialPos;
    Vector3 currentPos;
    Material cubeMaterial;
    GameObject mainGamePanel;
    Canvas panelCanvas;
    eSubModePanel subMode = eSubModePanel.None;
    public GameObject setupNewGamePanel;
    public GameObject creditsPanel;
    List<GameObject> activePanels = new List<GameObject>();
    int moveSpeed = 160; // how fast to move the panel
    bool centerPanel = false;
    bool openPanel = false;
    bool panelExpanded = false;
    bool subModePanelLoaded = false;
    private Button newGameButton;
    private Button creditsButton;
    private Button returntoMainButton;
    private Transform panelRect;
       
    void Start ()
    {
        initialPos = gameObject.transform.localPosition;
        currentPos = initialPos;
        panelExpanded = false;
        panelWidthAfterOpen = (int)(Screen.width * .9f);
        panelHeightAfterOpen = (int)(Screen.height * .85f);
        panelInitialHeight = gameObject.transform.localScale.y;
        panelInitialWidth = gameObject.transform.localScale.x;
        panelRect = gameObject.GetComponent<Transform>();
        mainGamePanel = GameObject.Find("Main Game Panel");
        panelCanvas = GameObject.Find("Panel UI Canvas").GetComponent<Canvas>();
        cubeMaterial = gameObject.GetComponent<MeshRenderer>().material;      
        newGameButton = GameObject.Find("New Game Button").GetComponent<Button>();
        creditsButton = GameObject.Find("Credits").GetComponent<Button>();
        

        // add listeners to the buttons for delegate functions
        newGameButton.onClick.AddListener(delegate { SetSubMode(eSubModePanel.NewGame); }); // initiate new game setup screen
        creditsButton.onClick.AddListener(delegate { SetSubMode(eSubModePanel.Credits); }); // initiate new game setup screen

    }
	
	// Update is called once per frame
	void Update ()
    {
        panelWidthAfterOpen = (int)(Screen.width * .9f);
        panelHeightAfterOpen = (int)(Screen.height * .85f);

        if (Input.GetKeyDown(KeyCode.Escape)) // always
            Application.Quit();

        currentPos = gameObject.transform.localPosition;

        if (centerPanel)
        {
            CenterPanel();
        }

        if (openPanel)
        {
            OpenPanel();
        }

        if (panelExpanded && !subModePanelLoaded)
            LoadSubmodePanel();
    }

    void CenterPanel()
    {
        bool xLocked = false;
        bool zLocked = false;

        mainGamePanel.SetActive(false); // deactivate the main game panel for now
        cubeMaterial.color = new Color(cubeMaterial.color.r, cubeMaterial.color.b, cubeMaterial.color.g, .9f);
        if (currentPos.x > 0)
            panelRect.Translate(new Vector3(-1, 0, 0) * moveSpeed * Time.deltaTime, Space.Self); // move the panel to the right
        else
            xLocked = true;

        if (currentPos.z > 0)
            panelRect.Translate(new Vector3(0, 0, -1) * moveSpeed * Time.deltaTime, Space.Self); // move the panel to the right
        else
            zLocked = true;

        // rotate the panel back to the front
        if (panelRect.localEulerAngles.y > 0 && gameObject.transform.localEulerAngles.y <= 350)
            panelRect.Rotate(new Vector3(0, -1, 0) * moveSpeed * Time.deltaTime, Space.World);
        
        if (xLocked && zLocked)
        {
            centerPanel = false;
            panelRect.localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    void OpenPanel()
    {
        bool widthIsHit = false;
        bool heightIsHit = false;

        if (panelRect.localScale.x < panelWidthAfterOpen)
        {
            widthIsHit = false;
            panelRect.localScale = new Vector3(panelRect.localScale.x + 24, panelRect.localScale.y, panelRect.localScale.z);
        }
        else
            widthIsHit = true;

        if (panelRect.localScale.y < panelHeightAfterOpen)
        {
            heightIsHit = false;
            panelRect.localScale = new Vector3(panelRect.localScale.x, panelRect.localScale.y + 12, panelRect.localScale.z);
        }
        else
            heightIsHit = true;

        if (widthIsHit && heightIsHit && !centerPanel)
            panelExpanded = true; // panel expansion completed
    }

    public void ReturnToStartingPosition()
    {
        foreach (GameObject panel in activePanels)
        {
            Destroy(panel);
        }

        // reset to the old position
        transform.localPosition = initialPos;
        transform.localScale = new Vector3(panelInitialWidth, panelInitialHeight, 50);
        transform.localEulerAngles = new Vector3(0, 60, 0);
        mainGamePanel.SetActive(true);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        openPanel = false;
        centerPanel = false;
        panelExpanded = false;
        subModePanelLoaded = false;
    }

    void SetSubMode(eSubModePanel mode)
    {
        ExpandMainPanel();
        switch (mode)
        {
            case eSubModePanel.NewGame:
                subMode = eSubModePanel.NewGame;
                break;
            case eSubModePanel.Credits:
                subMode = eSubModePanel.Credits;
                break;
            case eSubModePanel.Options:
                break;
            case eSubModePanel.LoadGame:
                break;
            case eSubModePanel.None:
                break;
            default:
                break;
        }           
    }

    void CreditScreen()
    {
        GameObject newGamePanel = Instantiate(creditsPanel, new Vector3(0, 0, -.5f), Quaternion.identity) as GameObject;
        newGamePanel.transform.SetParent(panelCanvas.transform);

        newGamePanel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, panelHeightAfterOpen);
        newGamePanel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, panelWidthAfterOpen);
        newGamePanel.transform.localPosition = new Vector3(0, 0, -35f);
        newGamePanel.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
        newGamePanel.transform.localScale = new Vector3(1, 1, 1);
        subModePanelLoaded = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        returntoMainButton = newGamePanel.transform.Find("Return To Main Button").GetComponent<Button>();
        returntoMainButton.onClick.AddListener(delegate { ReturnToStartingPosition(); }); // initiate new game setup screen
        activePanels.Add(newGamePanel);
    }

    void NewGameScreen()
    {
        GameObject newGamePanel = Instantiate(setupNewGamePanel, new Vector3(0, 0, -.5f), Quaternion.identity) as GameObject;
        newGamePanel.transform.SetParent(panelCanvas.transform);

        newGamePanel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, panelHeightAfterOpen);
        newGamePanel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, panelWidthAfterOpen);
        newGamePanel.transform.localPosition = new Vector3(0, 0, -35f);
        newGamePanel.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
        newGamePanel.transform.localScale = new Vector3(1,1, 1);
        subModePanelLoaded = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        returntoMainButton = newGamePanel.transform.Find("Return To Main Button").GetComponent<Button>();
        returntoMainButton.onClick.AddListener(delegate { ReturnToStartingPosition(); }); // initiate new game setup screen
        activePanels.Add(newGamePanel);
    }

    void LoadSubmodePanel()
    {       
        switch (subMode)
        {
            case eSubModePanel.NewGame:
                NewGameScreen();
                break;
            case eSubModePanel.Credits:
                CreditScreen();
                break;
            case eSubModePanel.Options:
                break;
            case eSubModePanel.LoadGame:
                break;
            case eSubModePanel.None:
                break;
            default:
                break;
        }       
    }

    void ExpandMainPanel()
    {
        centerPanel = true;
        openPanel = true;
        
    }
}
