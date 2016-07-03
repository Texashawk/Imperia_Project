using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using PlanetObjects;
using CameraScripts;
using Screens.Galaxy;
using HelperFunctions;
using Managers;

public class PlanetView : MonoBehaviour {

    private GalaxyCameraScript gScriptRef;
    private GameData gameDataRef;
    private GraphicAssets graphicsDataRef;
    private UIManager uiManagerRef;
    private GalaxyView gScreen;

    // UI elements
    public GameObject BuildPlanPanel;
    public GameObject TradePanel;
    public GameObject ProductionPanel;
    public GameObject ViceroyPanel;
    public GameObject ChainOfCommandPanel;
    public GameObject GPPPanel;

    public GameObject TradeButton;
    public GameObject BuildPlanButton;
    public GameObject ProductionButton;
    public GameObject GPPButton;

    private Image tradeButtonBar;
    private Image buildPlanButtonBar;
    private Image productionButtonBar;
    private Image gppButtonBar;
     
    public GameObject habitableTile;
    public GameObject uninhabitableTile;
    private GameObject tileMapPanel;

    private PlanetData pData;
   
    private List<GameObject> tileList = new List<GameObject>();
    private List<GameObject> planetObjectsDrawnList = new List<GameObject>();
    private bool planetDataLoaded = false;  //ensures that only one check is made every time script is run
    private bool tileMapGenerated = false;
    private Quaternion tilePanelOriginalRotation;
    
    private bool planetInfoInitialized = false;
    private GameObject planetCanvas;
    private float screenWidthRatio;
    private float screenHeightRatio;
    private GameObject selectedPlanet;
    private Color planetColor;

    // ui bools
    private bool regionDisplayMode = false; // off to start
    private bool leadershipPanelUpdated = false; // to update leadership panel
    private bool regionDisplayTiltActive = false; // to show tilt status
    private bool showTradeWindow = false;
    private bool showBuildPlanWindow = false;
    private bool showProductionWindow = false;
    private bool showGPPWindow = false;

    // tile panel movement vars
    private float tilePanelCurrentAngle = 0f;

    void Awake()
    {    
        tileMapPanel = GameObject.Find("Tile Map Panel");      
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();     
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        planetCanvas = GameObject.Find("Planet UI Canvas");
        TradeButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleTradePanel(); }); 
        BuildPlanButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleBuildPlanPanel(); });
        ProductionButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleProductionPanel(); });
        GPPButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleGPPPanel(); });
        tradeButtonBar = TradeButton.transform.Find("Active_State").GetComponent<Image>();
        buildPlanButtonBar = BuildPlanButton.transform.Find("Active_State").GetComponent<Image>();
        productionButtonBar = ProductionButton.transform.Find("Active_State").GetComponent<Image>();
        gppButtonBar = GPPButton.transform.Find("Active_State").GetComponent<Image>();
    }

	void Start () 
    {
        gScriptRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // tie the game camera script to the data
        gScreen = GameObject.Find("GameEngine").GetComponent<GalaxyView>();           
        tileMapPanel.SetActive(false);     
        tilePanelOriginalRotation = tileMapPanel.transform.rotation;
	}

    void OnGUI()
    {
        // set ratios against native resolution (1920 X 1080)
        screenWidthRatio = ((float)Screen.width / 1920f);
        screenHeightRatio = ((float)Screen.height / 1080f);

        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet)
        {
            planetCanvas.SetActive(true);
            if (!planetDataLoaded) // load selected planet data into pData ref if needed or changed
            {
                pData = gScreen.GetSelectedPlanet().GetComponent<Planet>().planetData;
                selectedPlanet = gScreen.GetSelectedPlanet();             
                planetDataLoaded = true;             
                planetInfoInitialized = true;
                ShowPlanetView();
            }
           
            //if (!tileMapGenerated)
            //    if (pData.Size > 0)
            //    {
            //        GenerateTileMap();   
            //    }

            UpdateUIPanels();
        }
        else
        {
            ResetDrawStates();
            planetCanvas.SetActive(false);
        }      
    }

    void UpdateUIPanels()
    {
        
        //if (DataRetrivalFunctions.GetSystem(pData.SystemID).IntelValue < 10 && !gameDataRef.DebugMode)
        //{
        //    regionDisplayMode = false; // reset background;
        //}
        //if (!regionDisplayMode)
        //{
        //    tileMapPanel.SetActive(false);
        //    tileMapPanel.transform.rotation = tilePanelOriginalRotation;
        //}
        //else
        //{          
        //    tileMapPanel.SetActive(true);                  
        //    // flatten the panel if desired
        //    if (Input.GetKeyUp(KeyCode.F))
        //    {
        //        ToggleRegionDisplayTiltMode();
        //    }
        //}

        switch (uiManagerRef.PrimaryViewMode)
        {
            case ViewManager.ePrimaryView.Economic:
                ShowEconomicMode();
                break;
            case ViewManager.ePrimaryView.Political:
                ShowPoliticalMode();
                break;
            case ViewManager.ePrimaryView.Military:
                ShowMilitaryMode();
                break;
            case ViewManager.ePrimaryView.Demographic:
                ShowDemographicMode();
                break;
            default:
                break;
        }
    }

    void ToggleTradePanel()
    {
        if (!showTradeWindow) 
            showTradeWindow = true;            
        
        else
            showTradeWindow = false;       
    }

    void ToggleBuildPlanPanel()
    {
        if (!showBuildPlanWindow)
            showBuildPlanWindow = true;

        else
            showBuildPlanWindow = false;
    }

    void ToggleProductionPanel()
    {
        if (!showProductionWindow)
            showProductionWindow = true;

        else
            showProductionWindow = false;
    }

    void ToggleGPPPanel()
    {
        if (!showGPPWindow)
            showGPPWindow = true;

        else
            showGPPWindow = false;
    }

    void ResetDrawStates()
    {               
        planetDataLoaded = false; // reset data load toggle
        leadershipPanelUpdated = false;

        foreach (GameObject tile in tileList)
            DestroyObject(tile);

        tileList.Clear();
        
        regionDisplayTiltActive = false;      
        tileMapGenerated = false;     
        planetInfoInitialized = false;
        tileMapPanel.SetActive(false);

        ChainOfCommandPanel.SetActive(false);
        ViceroyPanel.SetActive(false);

        // delete all other objects in scene
        foreach (GameObject pObject in planetObjectsDrawnList)
        {
            Destroy(pObject);
        }
        planetObjectsDrawnList.Clear();
        selectedPlanet = null; //clear the selected planet                  
    }

    void HideAllPanels()
    {
        // set all items to hide
        BuildPlanPanel.SetActive(false);      
        TradePanel.SetActive(false);
        ProductionPanel.SetActive(false);
        GPPPanel.SetActive(false);
    }

    void ShowPlanetView()
    {
        if (DataRetrivalFunctions.GetSystem(pData.SystemID).IntelValue >= 5 || gameDataRef.DebugMode)
        {
            if (pData.IsInhabited)
            {
                ChainOfCommandPanel.SetActive(true);
                ViceroyPanel.SetActive(true);
            }
        }   
    }

    void ShowDemographicMode()
    {
        HideAllPanels();

        if (pData.IsInhabited)
        {
            TradeButton.SetActive(false);
            BuildPlanButton.SetActive(false);
            ProductionButton.SetActive(false);
            GPPButton.SetActive(false);
        }
        else
        {

        }        
    }

    void ShowMilitaryMode()
    {
        HideAllPanels();

        if (pData.IsInhabited)
        {
            TradeButton.SetActive(false);
            BuildPlanButton.SetActive(false);
            ProductionButton.SetActive(false);
            GPPButton.SetActive(false);
        }
        else
        {

        }
    }

    void ShowPoliticalMode()
    {
        HideAllPanels();

        if (pData.IsInhabited)
        {
            TradeButton.SetActive(false);
            BuildPlanButton.SetActive(false);
            ProductionButton.SetActive(false);
            GPPButton.SetActive(false);
        }
        else
        {

        }
    }

    void ShowEconomicMode()
    {
        if (pData.IsInhabited)
        {
            if (showBuildPlanWindow)
            {
                BuildPlanPanel.SetActive(true);
                buildPlanButtonBar.enabled = true;
            }
            else
            {
                BuildPlanPanel.SetActive(false);
                buildPlanButtonBar.enabled = false;
            }

            if (showTradeWindow)
            {
                TradePanel.SetActive(true);
                tradeButtonBar.enabled = true;
            }
            else
            {
                TradePanel.SetActive(false);
                tradeButtonBar.enabled = false;
            }

            if (showProductionWindow)
            {
                ProductionPanel.SetActive(true);
                productionButtonBar.enabled = true;
            }
            else
            {
                ProductionPanel.SetActive(false);
                productionButtonBar.enabled = false;
            }
            if (showGPPWindow)
            {
                GPPPanel.SetActive(true);
                gppButtonBar.enabled = true;
            }
            else
            {
                GPPPanel.SetActive(false);
                gppButtonBar.enabled = false;
            }

            // activate buttons
            TradeButton.SetActive(true);
            BuildPlanButton.SetActive(true);
            ProductionButton.SetActive(true);
            GPPButton.SetActive(true);
        }
        else
        {
            BuildPlanPanel.SetActive(false);
            TradePanel.SetActive(false);
            ProductionPanel.SetActive(false);
            GPPPanel.SetActive(false);

            TradeButton.SetActive(false);
            BuildPlanButton.SetActive(false);
            ProductionButton.SetActive(false);
            GPPButton.SetActive(false);
        }
    }

    //void UpdateLeadershipChain()
    //{
    //    StarData sData = HelperFunctions.DataRetrivalFunctions.GetSystem(pData.SystemID); // get system ID
    
    //    // define the 4 groups of data
    //  //  Transform domesticPrimeInfo = edictPanel.transform.Find("Domestic Prime Image");
    //  //  Transform provinceGovernorInfo = edictPanel.transform.Find("Province Governor Image");
    //  //  Transform systemGovernorInfo = edictPanel.transform.Find("System Governor Image");
    //  //  Transform viceroyInfo = edictPanel.transform.Find("Viceroy Image");

    //    if (pData.IsInhabited)
    //    {
    //        if (!gameDataRef.CivList[0].PlanetIDList.Exists(p => p == pData.ID)) // if the planet is not owned by your civ, disable the panels except for viceroy
    //        {
    //            domesticPrimeInfo.gameObject.SetActive(false);
    //            provinceGovernorInfo.gameObject.SetActive(false);
    //            systemGovernorInfo.gameObject.SetActive(false);
    //            if (HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID) != "none")
    //            {
    //                viceroyInfo.gameObject.SetActive(true);
    //                string vID = HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID);
    //                Character vGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(vID);
    //                viceroyInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(vGov, -21f); // set up the tooltip
    //                viceroyInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == vGov.PictureID);
    //                string charName = vGov.Name.ToUpper();
    //                if (vGov.HouseID != null)
    //                {
    //                    charName += " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(vGov.HouseID).Name.ToUpper();
    //                }
    //                viceroyInfo.Find("Viceroy Name").GetComponent<Text>().text = charName;
    //                if (HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID) != null)
    //                {
    //                    viceroyInfo.Find("Viceroy Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID).Name.ToUpper();
    //                }
    //            }
    //            else
    //            {
    //                viceroyInfo.gameObject.SetActive(false);
    //            }
    //            return;
    //        }
    //    }

    //    // set domestic prime image
    //    if (DataRetrivalFunctions.GetPrime(Character.eRole.DomesticPrime) != "none")
    //    {
    //        domesticPrimeInfo.gameObject.SetActive(true);
    //        string pID = DataRetrivalFunctions.GetPrime(Character.eRole.DomesticPrime);
    //        Character pGov = DataRetrivalFunctions.GetCharacter(pID);
    //        domesticPrimeInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(pGov, -25f);
    //        domesticPrimeInfo.GetComponent<CharacterScreenActivation>().InitializeData(pGov);
    //        domesticPrimeInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == pGov.PictureID);
    //        domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().text = pGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(pGov.HouseID).Name.ToUpper();

    //        if (pGov.AssignedHouse.Rank == House.eHouseRank.Great)
    //        {
    //            domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
    //        }
    //        else if (pGov.AssignedHouse.Rank == House.eHouseRank.Minor)
    //        {
    //            domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().color = Color.green; // if great house, change color
    //        }
    //        else
    //        {
    //            domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().color = Color.white; // if great house, change color
    //        }

    //        if (DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID) != null)
    //        {
    //            domesticPrimeInfo.Find("Domestic Prime Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID).Name.ToUpper();
    //        }
    //    }
    //    else
    //    {
    //        domesticPrimeInfo.gameObject.SetActive(false);
    //    }
        
    //    // set province governor image  
    //    if (DataRetrivalFunctions.GetProvinceGovernorID(sData.AssignedProvinceID) != "none")
    //    {
    //        provinceGovernorInfo.gameObject.SetActive(true);         
    //        string pID = DataRetrivalFunctions.GetProvinceGovernorID(sData.AssignedProvinceID);
    //        Character pGov = DataRetrivalFunctions.GetCharacter(pID);
    //        provinceGovernorInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(pGov, -23f);
    //        provinceGovernorInfo.GetComponent<CharacterScreenActivation>().InitializeData(pGov);
    //        provinceGovernorInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == pGov.PictureID);
    //        provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().text = pGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(pGov.HouseID).Name.ToUpper();
    //        if (pGov.AssignedHouse.Rank == House.eHouseRank.Great)
    //        {
    //            provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
    //        }
    //        else if (pGov.AssignedHouse.Rank == House.eHouseRank.Minor)
    //        {
    //            provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().color = Color.green; // if great house, change color
    //        }
    //        else
    //        {
    //            provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().color = Color.white; // if great house, change color
    //        }
    //        if (DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID) != null)
    //        {
    //            provinceGovernorInfo.Find("Province Governor Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID).Name.ToUpper();
    //        }

    //        // set color of label for support
    //        Color supportColor;
    //        if (pData.ProvGovSupport == eSupportLevel.Full)
    //        {
    //            supportColor = Color.green;
    //        }
    //        else if (pData.ProvGovSupport == eSupportLevel.Partial)
    //        {
    //            supportColor = Color.yellow;
    //        }
    //        else
    //        {
    //            supportColor = Color.red;
    //        }
    //        provinceGovernorInfo.Find("Province Governor Label").GetComponent<Text>().color = supportColor;
    //    }
    //    else
    //    {
    //        provinceGovernorInfo.gameObject.SetActive(false);
    //    }

    //    // set system governor image
    //    if (DataRetrivalFunctions.GetSystemGovernorID(sData.ID) != "none")
    //    {
    //        systemGovernorInfo.gameObject.SetActive(true);
    //        string sID = DataRetrivalFunctions.GetSystemGovernorID(sData.ID);
    //        Character sGov = DataRetrivalFunctions.GetCharacter(sID);
    //        systemGovernorInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(sGov, -22f);
    //        systemGovernorInfo.GetComponent<CharacterScreenActivation>().InitializeData(sGov);
    //        systemGovernorInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == sGov.PictureID);
    //        systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().text = sGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(sGov.HouseID).Name.ToUpper();
    //        if (sGov.AssignedHouse.Rank == House.eHouseRank.Great)
    //        {
    //            systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
    //        }
    //        else if (sGov.AssignedHouse.Rank == House.eHouseRank.Minor)
    //        {
    //            systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().color = Color.green; // if great house, change color
    //        }
    //        else
    //        {
    //            systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().color = Color.white; // if great house, change color
    //        }
    //        if (HelperFunctions.DataRetrivalFunctions.GetPlanet(sGov.PlanetLocationID) != null)
    //        {
    //            systemGovernorInfo.Find("System Governor Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(sGov.PlanetLocationID).Name.ToUpper();
    //        }

    //        // set color of label for support
    //        Color supportColor;
    //        if (pData.SysGovSupport == eSupportLevel.Full)
    //        {
    //            supportColor = Color.green;
    //        }
    //        else if (pData.SysGovSupport == eSupportLevel.Partial)
    //        {
    //            supportColor = Color.yellow;
    //        }
    //        else
    //        {
    //            supportColor = Color.red;
    //        }
    //        systemGovernorInfo.Find("System Governor Label").GetComponent<Text>().color = supportColor;
    //    }
    //    else
    //    {
    //        systemGovernorInfo.gameObject.SetActive(false);
    //    }

    //    // set viceroy image
    //    if (DataRetrivalFunctions.GetPlanetViceroyID(pData.ID) != "none")
    //    {
    //        viceroyInfo.gameObject.SetActive(true);
    //        string vID = HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID);
    //        Character vGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(vID);
    //        viceroyInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(vGov, -21f); // set up the tooltip
    //        viceroyInfo.GetComponent<CharacterScreenActivation>().InitializeData(vGov); // set up data for character screen
    //        viceroyInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == vGov.PictureID);
    //        viceroyInfo.Find("Viceroy Name").GetComponent<Text>().text = vGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(vGov.HouseID).Name.ToUpper();
    //        if (vGov.AssignedHouse.Rank == House.eHouseRank.Great)
    //        {
    //            viceroyInfo.Find("Viceroy Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
    //        }
    //        else if (vGov.AssignedHouse.Rank == House.eHouseRank.Minor)
    //        {
    //            viceroyInfo.Find("Viceroy Name").GetComponent<Text>().color = Color.green; // if great house, change color
    //        }
    //        else
    //        {
    //            viceroyInfo.Find("Viceroy Name").GetComponent<Text>().color = Color.white; // if great house, change color
    //        }
    //        if (HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID) != null)
    //        {
    //            viceroyInfo.Find("Viceroy Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID).Name.ToUpper();
    //        }
    //    }
    //    else
    //    {
    //        viceroyInfo.gameObject.SetActive(false);
    //    }
    //}

    void GenerateTileMap()
    {
        tileMapPanel.SetActive(true); // active tile panel
        tileMapPanel.transform.rotation = tilePanelOriginalRotation; // reset the rotation to the original

        // draw tiles
        int maxRowCount = 0;
        int maxColCount = 0;
        int curRow = 0;
        int curCol = 0;
        Vector2 topLeftOfTileMap;
        int tileSizeInUnits = 0; // size of tile prefab
        float squareRoot = Mathf.Sqrt(pData.MaxTiles); // get the square root
        int squareRound = Mathf.RoundToInt(squareRoot); // then round it

        maxColCount = squareRound; // set # of rows
        maxRowCount = squareRound;

        if (squareRound <= squareRoot) // if rounded down, add 1 to the squareround to always make rows equal
            maxRowCount += 1;

        maxColCount -= 1;
           
        for (int x = 0; x < pData.RegionList.Count; x++)
        {
            Region tempTile = new Region();
            GameObject tileImage;

            tempTile = pData.RegionList[x];

            if (tempTile.IsHabitable)
            {
                tileImage = Instantiate(habitableTile, new Vector3(0, 0, 0), Quaternion.identity) as GameObject; // create a new tile from prefab
            }
            else
                tileImage = Instantiate(uninhabitableTile, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            tileImage.transform.SetParent(tileMapPanel.transform, true); // assign to tile map
            tileList.Add(tileImage); // add to tile objects to be destroyed when view ends
            tileImage.name = tempTile.ID;
            tileImage.transform.localScale = new Vector3(42f, 42f, 0f); // make same size          

            // convert to screen coordinates
            tileSizeInUnits = (int)tileImage.GetComponent<RectTransform>().rect.width;

            topLeftOfTileMap = new Vector3(-(maxColCount * tileSizeInUnits) / 2, ((maxRowCount * tileSizeInUnits) - tileSizeInUnits) / 2, 0);
            tileImage.transform.localPosition = new Vector3(topLeftOfTileMap.x + (curCol * tileSizeInUnits), topLeftOfTileMap.y - (curRow * tileSizeInUnits), 0f);

            // update tile row and column coordinates
            if (curCol < maxColCount)
                curCol += 1;
            else
            {
                curCol = 0;
                curRow += 1;
            }

            // show development level current/max (eventually this will be a mode and moved to an outside function to query each tile in the tile list to change the value)

            // highlight if a targeted region in a build plan, otherwise show general habitability with graduated color
            if (pData.RegionList[x].IsHabitable)
            {
                float red = ((100 - (float)pData.RegionList[x].BioRating) * 2.55f) / 255f;
                float green = ((float)pData.RegionList[x].BioRating * 2.55f) / 255f;
                Color habColor = new Color(red, green, 0f);
                tileImage.GetComponent<MeshRenderer>().material.color = habColor;
            }
            else
                tileImage.GetComponent<MeshRenderer>().material.color = Color.grey;

            if (pData.RegionList[x].IsTargetedRegionForBuild)
                tileImage.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        
        tileMapGenerated = true;        
    }

    public void ToggleRegionDisplayMode()
    {
        if (regionDisplayMode)
            regionDisplayMode = false;
        else
        {
            regionDisplayMode = true;
            StartCoroutine(TiltRegionPanel(.35f));

            int x = 0;
            foreach (GameObject tile in tileList)
            {
                float height = (float)pData.RegionList[x].HabitatationInfrastructureLevel + 1f;
                StartCoroutine(RaiseBars(height, tile));
                x += 1;
            }
           
        }
    }

    public void ToggleRegionDisplayTiltMode()
    {
        if (regionDisplayTiltActive)
            regionDisplayTiltActive = false;
        else
            regionDisplayTiltActive = true;

        if (!regionDisplayTiltActive)
        {
            StartCoroutine(TiltRegionPanel(.35f));
        }
        else
        {
            StartCoroutine(FlattenRegionPanel());
        }
    }

    IEnumerator TiltRegionPanel(float targetAngleTilt)
    {

        while (tileMapPanel.transform.rotation.x < targetAngleTilt)
        {          
            tileMapPanel.transform.Rotate(2.2f, 0f, 0f); // give it the tilted look
            yield return null;
            regionDisplayTiltActive = true;
        }
    }

    IEnumerator FlattenRegionPanel()
    {

        while (tileMapPanel.transform.rotation.x > 0f)
        {
            tileMapPanel.transform.Rotate(-2.2f, 0f, 0f); // give it the tilted look
            yield return null;
            regionDisplayTiltActive = false;
        }
    }

    IEnumerator RaiseBars(float targetHeight, GameObject tile)
    {
        while (tile.transform.localScale.z < targetHeight)
        {
            tile.transform.localScale = new Vector3(45f,45f,tile.transform.localScale.z + 1.3f);
            tile.transform.localPosition = new Vector3(tile.transform.localPosition.x,tile.transform.localPosition.y, 0f - (tile.transform.localScale.z / 2f));
            yield return null;
        }
    }
  
}
