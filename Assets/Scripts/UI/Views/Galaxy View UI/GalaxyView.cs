using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CameraScripts;
using StellarObjects;
using System.Collections.Generic;
using System;
using CivObjects;
using Constants;
using Managers;
using System.Linq;
using TMPro; // Text Mesh Pro

namespace Screens.Galaxy
{
    public class GalaxyView : MonoBehaviour
    {
        enum eGalaxyNameFilter : int
        {
            NONE,
            BIO,
            OWNERSHIP,
            UNREST
        }

        public GameObject systemGrid;
        public GameObject systemTrail;
        private ModalPanel modalPanel; // for the modal panel

        private UnityAction myYesAction;
        private UnityAction myNoAction;
        private UnityAction myCancelAction;

        // Star Data Block properties
        public GameObject starNameObject;

        // planet prefab variables
        public GameObject[] barrenPlanets;
        public GameObject[] asteroidBelts;
        public GameObject[] greenhousePlanets;
        public GameObject[] desertPlanets;
        public GameObject[] terranPlanets;
        public GameObject[] icePlanets;
        public GameObject[] dwarfJovianPlanets;
        public GameObject[] jovianPlanets;
        public GameObject[] lavaPlanets;
        public GameObject[] irradiatedPlanets;
        public GameObject[] waterPlanets;
        public GameObject[] superEarthPlanets;
        public GameObject[] brownDwarfPlanets;
        public GameObject[] iceBelts;
        public GameObject[] dustRings;
        public GameObject[] rings;
        public GameObject[] starmapNebulas;
        public GameObject[] systemNebulas;
        public Material provinceLineTexture;

        // galaxy label status
        private eGalaxyNameFilter galaxyNameFilter = eGalaxyNameFilter.OWNERSHIP; // defaults to ownership

        private Image button;

        private TextMeshProUGUI cameraZoomLevelLine;
        private TextMeshProUGUI cameraZoomValueLine;
        private GalaxyData galaxyDataRef;
        private GalaxyCameraScript gCameraRef;
        private GameObject eventScrollView;
        private bool planetSelected = false;
        private float fov = 0.0f;
        private GUIStyle gStyle;
        private float zoomValue = 0.0f;
        private float maxZoomLevel = 0.0f;
        private GameData gameDataRef;
        private UIManager uiManagerRef;
        private GameObject starmapSprite;
        private GameObject backingSphere;

        private Camera mainCamera;
        private Canvas mainUIOverlay;
        private Canvas galaxyPlanetInfoCanvas;

        private GameObject[] planetsToDraw = new GameObject[5];
        [HideInInspector] public List<GameObject> listSystemObjectsCreated = new List<GameObject>();
        [HideInInspector] public List<GameObject> listSystemPlanetsCreated = new List<GameObject>();
        [HideInInspector] public List<GameObject> listGalaxyUIObjectsCreated = new List<GameObject>(); // list of all UI elements created (lines, range circles, etc)
        private List<StellarObjectDataBlock> listTextObjectsCreated = new List<StellarObjectDataBlock>(); // list of text labels (so can move dynamically onGUI)
        public bool planetsDrawn = false; // system planets are drawn?
        private bool systemNameDrawn = false; // system names are drawn?
        private bool systemGridCreated = false; // is system grid created?
        private bool provinceLinesDrawn = false; // are province lines drawn?
        private bool systemTrailCreated = false; // is background trail created?
        private bool mapInOverviewMode = false; // is the map in province or galaxy mode?
        public bool EventPanelActive = true; // set visibilty
        private GameObject smallSystemGrid;
        private GameObject sysTrail; // the local copy of the system trail
        private GameObject systemUICanvas;
        private GameObject selectedUnitInfoCanvas;

        private TextMeshProUGUI versionNumber;
        private ViewManager.eViewLevel zoomLevel = ViewManager.eViewLevel.Galaxy;

        private GraphicAssets graphicAssets;

        void Awake()
        {
            // modal stuff
            modalPanel = ModalPanel.Instance();  // sets up a static instance of the window
            myYesAction = new UnityAction(TestYesFunction); // assign functions to actions
            myNoAction = new UnityAction(TestNoFunction);
            myCancelAction = new UnityAction(TestCancelFunction);

            starmapSprite = GameObject.Find("Y-Axis Grid");                     
            backingSphere = GameObject.Find("Backing Sphere");
                    
            versionNumber = GameObject.Find("Version Info").GetComponent<TextMeshProUGUI>();
            systemUICanvas = GameObject.Find("System UI Canvas");
            eventScrollView = GameObject.Find("Event ScrollView");
            selectedUnitInfoCanvas = GameObject.Find("Selected Unit Information Canvas");
            mainUIOverlay = GameObject.Find("Main UI Overlay").GetComponent<Canvas>();
            galaxyPlanetInfoCanvas = GameObject.Find("Galaxy Planet Info Canvas").GetComponent<Canvas>();

            // data objects
            gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
            graphicAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
            gCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // get global camera script
            uiManagerRef = GameObject.Find("UI Engine").GetComponent<UIManager>();

        }

        public void VerifyExitModalWindow()
        {
            modalPanel.Choice("Wait. You're going to leave?\nAre you sure? We have so many miles to go...", myYesAction, myNoAction, myCancelAction); // calls the modal panel
        }

        void TestYesFunction()
        {
            Debug.Log("You clicked yes!");
            Application.Quit(); // Get Out
        }

        void TestNoFunction()
        {
            Debug.Log("You clicked no.");
        }

        void TestCancelFunction()
        {
            Debug.Log("You clicked cancel.");
        }

        void Start()
        {
            galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            float tangent = Mathf.Tan(GalaxyCameraScript.cameraTilt * Mathf.Deg2Rad);
            mainCamera.transform.position = new Vector3(0f, (GalaxyCameraScript.galaxyZValue * tangent), GalaxyCameraScript.galaxyZValue); // move the camera initially
            gStyle = new GUIStyle();
            maxZoomLevel = 3500f;
            backingSphere.SetActive(false);
            systemUICanvas.SetActive(false);
            selectedUnitInfoCanvas.SetActive(false);
        }

        void OnGUI()
        {        
            DisplayVersionInfo();
            if ((uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy || zoomLevel == ViewManager.eViewLevel.Province))
            {
                DrawGalaxyMapUI();
                // hide all other stars
                if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
                {
                    HideStellarObjectsNotInSelectedProvince();                  
                    DisplayProvinceData();
                }
                else
                {
                    ShowStellarObjects();               
                    ShowProvinceLines();
                    ShowStellarDataBlocks();
                    selectedUnitInfoCanvas.SetActive(false);                   
                }
            }
            else
            {
                HideGalaxyMapUI();
            }

            if ((uiManagerRef.ViewLevel == ViewManager.eViewLevel.System) && (GetSelectedStar() != null))
            {
                planetSelected = false;
                DisplaySystemData();
                HideStellarDataBlocks();
                HideProvinceLines();
                if (GetSelectedStar().GetComponent<LineRenderer>() != null)
                {
                    GetSelectedStar().GetComponent<LineRenderer>().enabled = false;
                }
            }
            else if ((uiManagerRef.ViewLevel == ViewManager.eViewLevel.Planet) && (GetSelectedPlanet() != null))
            {
                DisplayPlanetData();
            }
                 
  
            if (!provinceLinesDrawn)
            {
                DrawProvinceLines();
            }
        }

        void HideProvinceLines()
        {
            if (provinceLinesDrawn)
            {
                foreach (GameObject star in galaxyDataRef.GalaxyStarList)
                {
                    if (star.GetComponent<LineRenderer>() != null)
                    {
                        star.GetComponent<LineRenderer>().enabled = false;
                    }
                }
            }
        }

        public void ToggleEventPanelDisplay()
        {
            if (EventPanelActive)
            {
                EventPanelActive = false;
                return;
            }
            else
            {
                EventPanelActive = true;
                return;
            }
        }

        void ShowProvinceLines()
        {
            if (provinceLinesDrawn)
            {
                foreach (GameObject star in galaxyDataRef.GalaxyStarList)
                {
                    if (star.GetComponent<LineRenderer>() != null)
                    {
                        if (star.GetComponent<Star>().starData.Province != null)
                        {
                            //Color lineColor = star.GetComponent<Star>().starData.Province.OwningCiv.Color;
                            //lineColor = new Color(lineColor.r, lineColor.g, lineColor.b, .7f);
                            star.GetComponent<LineRenderer>().enabled = true;
                            //star.GetComponent<LineRenderer>().SetColors(lineColor, lineColor);
                        }                      
                    }
                }
            }
        }

        void DrawProvinceLines()
        {
            foreach (Province pData in galaxyDataRef.ProvinceList)
            {
                if (pData.OwningCivID == gameDataRef.CivList[0].ID && pData.SystemList.Count > 1)
                {                
                    List<Vector3> copiedPositionList = new List<Vector3>(); // empty list of line positions
                    List<Vector3> topQuadrantList = new List<Vector3>(); // list that contains all star locations in the upper quadrant
                    List<Vector3> bottomQuadrantList = new List<Vector3>(); // list that contains all star locations in the lower quadrant
                    
                    for (int a = 0; a < pData.SystemList.Count; a++)
                    {
                        copiedPositionList.Add(pData.SystemList[a].WorldLocation); // copy each position into a local copy
                    }

                    // Now use the center of the province to check each quadrant and add to the list
                    Vector3 provCenter = pData.ProvinceCenter;

                    // add points to the respective quad lists based on relative position to the center
                    foreach (Vector3 point in copiedPositionList)
                    {
                        if (point.y < provCenter.y)
                            topQuadrantList.Add(point);                     
                        if (point.y > provCenter.y)
                            bottomQuadrantList.Add(point);                       
                    }

                    // now sort the lists by X value
                    List<Vector3> sortedTopQuadrantList = topQuadrantList.OrderBy(list => list.x).ToList(); // first reorder the points in ascending X order...
                    List<Vector3> sortedBottomQuadrantList = bottomQuadrantList.OrderByDescending(list => list.x).ToList(); // .. then reorder the lower points in descending X order!

                    StarData sData = pData.SystemList.Find(p => p.WorldLocation == sortedTopQuadrantList[0]); // find the first star using the anchor coordinates
                    GameObject.Find(sData.Name).AddComponent<LineRenderer>(); // attach a new line renderer
                    LineRenderer lr = GameObject.Find(sData.Name).GetComponent<LineRenderer>(); // and then create a reference to it

                    // set the line renderer constants for the province line
                    lr.SetVertexCount(pData.SystemList.Count + 1);
                    lr.SetPosition(0, new Vector3(sData.WorldLocation.x, sData.WorldLocation.y, -60));
                    lr.material = provinceLineTexture;
                    lr.SetColors(gameDataRef.CivList[0].Color, gameDataRef.CivList[0].Color);
                    lr.SetWidth(15f, 15f);

                    // loop through each sorted list and draw the vertexes
                    for (int y = 1; y < sortedTopQuadrantList.Count; y++)
                    {
                        lr.SetPosition(y, new Vector3(sortedTopQuadrantList[y].x, sortedTopQuadrantList[y].y, -60));
                        lr.SetWidth(15f, 15f);
                    }

                    for (int y = sortedTopQuadrantList.Count; y < pData.SystemList.Count; y++)
                    {
                        lr.SetPosition(y, new Vector3(sortedBottomQuadrantList[y-sortedTopQuadrantList.Count].x, sortedBottomQuadrantList[y-sortedTopQuadrantList.Count].y, -60));
                        lr.SetWidth(15f, 15f);
                    }

                    lr.SetPosition(pData.SystemList.Count, new Vector3(sortedTopQuadrantList[0].x, sortedTopQuadrantList[0].y, -60)); // the final position
                    lr.SetWidth(15f, 15f);
                }
            }
            provinceLinesDrawn = true;
        }

        void LateUpdate()
        {
            FadeOutStarMapOnZoom();
            if (systemNameDrawn && zoomLevel < ViewManager.eViewLevel.System)
            {
                if (!mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive)
                {
                    ShowStellarDataBlocks();
                }
                UpdateStellarDataBlocks();
            }        
        }

        // Update is called once per frame
        void Update()
        {
            // get updated zoom levels
            zoomValue = mainCamera.GetComponent<GalaxyCameraScript>().zoom;
            zoomLevel = mainCamera.GetComponent<GalaxyCameraScript>().ZoomLevel;

            // determine if in overview mode
            if (zoomLevel == ViewManager.eViewLevel.Province || zoomLevel == ViewManager.eViewLevel.Galaxy)
            {
                mapInOverviewMode = true;
            }
            else
            {
                mapInOverviewMode = false;
            }

            // draw civ range circles if debug mode changes
            if (gameDataRef.RequestGraphicRefresh)
            {
                foreach (GameObject uiObject in listGalaxyUIObjectsCreated)
                {
                    GameObject.Destroy(uiObject);
                }
                listGalaxyUIObjectsCreated.Clear(); // clear out any range circles, lines, system pips, etc            
                DrawGalaxyMapUI(); // refresh UI
                gameDataRef.RequestGraphicRefresh = false;
            }

            //test for a modal window
            if (Input.GetKey(KeyCode.Escape))
            {
                VerifyExitModalWindow(); // call the exit modal window
            }
           
            fov = mainCamera.GetComponent<Camera>().fieldOfView; // field of view

            if (zoomLevel < ViewManager.eViewLevel.System && gameDataRef.uiSubMode == GameData.eSubMode.None && !gameDataRef.modalIsActive) // must be in no submode (astrographic)
                CheckForStarSelection();
            if (zoomLevel == ViewManager.eViewLevel.System && gameDataRef.uiSubMode == GameData.eSubMode.None && !gameDataRef.modalIsActive)
            {
                GetSelectedStar().GetComponent<SpriteRenderer>().enabled = true; // show the stars
                GetSelectedStar().GetComponent<Light>().enabled = true; // show the lights
                ClearSelectedPlanet(); // reset all selections
                ShowAllPlanets();
                CheckForPlanetSelection();
            }

            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                float scaleFactor = star.GetComponent<Star>().starData.Size / 50f;
                float screenFactor = Screen.width / 1920f;

                // normalize the scale
                if (scaleFactor < 1)
                    scaleFactor = 1;

                if (star.tag == "Selected") // zoomed star is bigger
                {
                    star.transform.localScale = new Vector2((30f  * scaleFactor) * screenFactor, ((30f * scaleFactor) * screenFactor));

                    // scale the binary/trinary children properly
                    foreach (Transform child in star.transform)
                    {
                        scaleFactor = child.GetComponent<Star>().starData.Size / 25;
                        // normalize the scale
                        if (scaleFactor < .75f)
                            scaleFactor = .75f;
                        child.localScale = new Vector2(.5f * scaleFactor, .5f * scaleFactor);
                        child.GetComponent<Light>().range = 1.05f * child.GetComponent<SpriteRenderer>().bounds.size.x;
                    }
                    star.transform.eulerAngles = new Vector3(0, 0, 0); // flatten out the tilt
                }
                else
                {
                    star.transform.localScale = new Vector2(6f * scaleFactor, 6f * scaleFactor);

                    // scale the binary/trinary children properly
                    foreach (Transform child in star.transform)
                    {
                        scaleFactor = child.GetComponent<Star>().starData.Size / 25;

                        // normalize the scale
                        if (scaleFactor < .75f)
                            scaleFactor = .75f;
                        child.localScale = new Vector2(.5f * scaleFactor, .5f * scaleFactor);
                        child.GetComponent<Light>().range = 1.5f * child.GetComponent<SpriteRenderer>().bounds.size.x;
                    }
                    star.transform.eulerAngles = new Vector3(-GalaxyCameraScript.cameraTilt, 0, 0); // tilt the star
                    star.GetComponent<Light>().range = 1.2f * star.GetComponent<SpriteRenderer>().bounds.size.x;
                }
            }

            // if camera zooms out too far, camera is considered in galaxy mode (use this in base camera script)
            if (zoomLevel != ViewManager.eViewLevel.System && gameDataRef.StarSelected == true && mainCamera.GetComponent<GalaxyCameraScript>().systemZoomActive == false && mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive == false)
            {
                ResetGalaxyView();
            }

            if (zoomLevel == ViewManager.eViewLevel.Galaxy)
            {
                ShowProvinceLines();
            }
            
            if (!systemNameDrawn)
            {
                GenerateStellarDataBlocks();
                //GenerateProvinceNames();
            }
        }

        void ResetGalaxyView()
        {
            gameDataRef.StarSelected = false;
            uiManagerRef.selectedPlanet = null;
            uiManagerRef.selectedSystem = null;
            RemoveSystemPlanets();
            GetSelectedStar().transform.position = GetSelectedStar().GetComponent<Star>().starData.WorldLocation; // set star back to its correct position
            ClearSelectedStar();
            ShowStellarObjects();        
            backingSphere.SetActive(false); // turn off the sphere
            GameObject.DestroyObject(smallSystemGrid);
            GameObject.DestroyObject(sysTrail);
            systemGridCreated = false;
            systemTrailCreated = false;
            systemUICanvas.SetActive(false); // don't show panel
            selectedUnitInfoCanvas.SetActive(false);
        }

        void DrawGalaxyMapUI()
        {
            if (gameDataRef.RequestGraphicRefresh)
            {
                eventScrollView.GetComponent<EventScrollView>().InitializeList(); // clear the list when made active
            }

            if (mapInOverviewMode)
            {
                              
                // turn off/on the event window but always turn off if there are no events to show
                if (gameDataRef.CivList[0].LastTurnEvents.Count > 0)                 
                    eventScrollView.SetActive(true);            
                else
                    eventScrollView.SetActive(false);
                    
            }
            else
            {
                eventScrollView.GetComponent<EventScrollView>().InitializeList(); // clear the list when made active
                eventScrollView.SetActive(false);              
            }         
        }

        void HideGalaxyMapUI()
        {
            eventScrollView.SetActive(false);
        }

        void CheckForStarSelection()
        {
            if (Input.GetMouseButtonDown(0) && gameDataRef.StarSelected != true)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 15000); // test

                if ((hit.collider != null))
                {
                    if (hit.transform.tag == "Star")
                    {
                        ClearSelectedStar(); // reset all selections
                        hit.transform.GetComponent<Star>().tag = "Selected"; // select the star

                        //invoke zoom sequence
                        Camera.main.GetComponent<GalaxyCameraScript>().starTarget = hit.transform;
                        Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = true;
                        Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
                        Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = false;
                        gameDataRef.StarSelected = true; // probably need to move to a global UI manager
                        uiManagerRef.selectedSystem = hit.transform.GetComponent<Star>().starData;
                        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.System);
                    }
                }
            }
        }

        //void GenerateProvinceNames()
        //{
        //    foreach (Province pData in galaxyDataRef.ProvinceList)
        //    {
        //        Vector3 textLocation = new Vector3(pData.ProvinceCenter.x, pData.ProvinceCenter.y, 0); // where the text box is located
        //        GameObject provinceName = Instantiate(starNameObject, pData.ProvinceCenter, Quaternion.identity) as GameObject;
        //        StellarObjectDataBlock starDataBlock = new StellarObjectDataBlock();  // create a new star data block

        //        provinceName.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the blocks to the panel
        //        provinceName.transform.localPosition = textLocation; //new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)
        //        provinceName.transform.localScale = new Vector3(1, 1, 1); // do not scale
        //        provinceName.tag = "Province"; // tag for selection
        //        provinceName.name = pData.ID;
        //        starDataBlock.objectRotation = 0f;
        //        starDataBlock.ownerName = pData.Name.ToUpper() + " PROVINCE";
        //        starDataBlock.provinceBounds = pData.ProvinceBounds; // to set the size of the label
        //        starDataBlock.provinceObjectLocation = textLocation; // provinceName.transform.localPosition;
        //        starDataBlock.ownerColor = HelperFunctions.DataRetrivalFunctions.FindProvinceOwnerColor(pData);
        //        starDataBlock.blockType = StellarObjectDataBlock.eBlockType.Province;
        //        starDataBlock.textObject = provinceName;
        //        listTextObjectsCreated.Add(starDataBlock);
        //    }
        //}

        void CheckForPlanetSelection()
        {
            if (Input.GetMouseButtonDown(0) && planetSelected != true)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, maxZoomLevel + 20);

                if (hit.collider != null && hit.transform.tag == "Planet")
                {
                    hit.transform.GetComponent<Planet>().tag = "Selected"; // select the planet

                    //invoke zoom sequence
                    Camera.main.GetComponent<GalaxyCameraScript>().planetTarget = hit.transform;
                    Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = true;
                    Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = false;
                    GetSelectedStar().GetComponent<SpriteRenderer>().enabled = false; // hide the stars
                    GetSelectedStar().GetComponent<Light>().enabled = false; // hide the light halo
                    uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.Planet);                   
                    uiManagerRef.selectedPlanet = hit.transform.GetComponent<Planet>().planetData;
                    planetSelected = true;
                }
            }
        }

        void RemoveSystemPlanets()
        {
            planetsDrawn = false;
            foreach (GameObject p in listSystemPlanetsCreated)
            {
                Destroy(p);
            }
            listSystemPlanetsCreated.Clear(); // clear the planets
            Array.Clear(planetsToDraw, 0, planetsToDraw.Length);
        }

        void DisplayVersionInfo()
        {
            versionNumber.text = "build " + gameDataRef.gameVersion;
        }

        void FadeOutStarMapOnZoom()
        {
            float fadeIntensity = 10.5f;

            if (fov <= GalaxyCameraScript.maxZoomLevel && fov > GalaxyCameraScript.minZoomLevel)
            {
                float alphaValue = ((255 - ((GalaxyCameraScript.maxZoomLevel - fov) * fadeIntensity)) / 255) * 1f;
                if (alphaValue > .5f) // normalize alpha
                    alphaValue = .5f;
                if (alphaValue < 0)
                    alphaValue = 0;

                starmapSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alphaValue);
            }
        }      

        void HideStellarDataBlocks()
        {
            foreach (StellarObjectDataBlock star in listTextObjectsCreated)
            {
                star.textObject.SetActive(false);
            }
        }

        void ShowStellarDataBlocks()
        {
            foreach (StellarObjectDataBlock star in listTextObjectsCreated)
            {
                star.textObject.SetActive(true);
            }
        }


        void ClearSelectedStar()
        {
            //reset the selected star by changing all tags to 'Star'
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star.tag != "Companion Star")
                    star.tag = "Star";
            }
        }

        public GameObject GetSelectedStar()
        {
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star.tag == "Selected")
                {
                    uiManagerRef.SelectedStellarObject = star.transform;
                    return star; // return the star
                }
            }
            return null; // no star, error
        }

        void UpdateStellarDataBlocks()
        {
            Vector3 textLocation;

            foreach (StellarObjectDataBlock text in listTextObjectsCreated)
            {

                if (text.textObject.activeSelf)
                {
                    Color textColor = new Color();

                    if (galaxyNameFilter == eGalaxyNameFilter.BIO) // check for bio status
                    {
                        if (text.blockType == StellarObjectDataBlock.eBlockType.Star)
                        {
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;
                            textColor = CheckBioStatus(starData);
                            text.textObject.GetComponent<TextMeshProUGUI>().text = starData.Name.ToUpper();
                        }
                        else
                        {
                            text.textObject.SetActive(false);
                        }
                        textColor = Color.white;
                    }
                    else if (galaxyNameFilter == eGalaxyNameFilter.NONE)
                    {
                        if (text.blockType == StellarObjectDataBlock.eBlockType.Star)
                        {
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;

                            text.textObject.GetComponentInChildren<TextMeshProUGUI>().text = starData.Name.ToUpper(); //+ civNames;
                        }
                        textColor = Color.gray;
                    }
                    else if (galaxyNameFilter == eGalaxyNameFilter.OWNERSHIP)
                    {
                        if (text.blockType == StellarObjectDataBlock.eBlockType.Star)
                        {
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;
                            if (!gameDataRef.DebugMode)
                            {
                                text.textObject.GetComponentInChildren<TextMeshProUGUI>().text = starData.Name.ToUpper();
                                if (starData.IntelValue > Constants.Constant.MediumIntelLevelMax)
                                    textColor = text.ownerColor;
                                else
                                    textColor = Color.grey;
                            }
                            else
                            {
                                text.textObject.GetComponentInChildren<TextMeshProUGUI>().text = starData.Name.ToUpper() + "(" + starData.WorldLocation.x.ToString("N0") + "," + starData.WorldLocation.y.ToString("N0") + ")" + text.ownerName + text.ownerTolerance;// stellarObject.civNames;
                                textColor = text.ownerColor;
                            }
                        }

                        // show province-level data, fade in and out
                        //else if (text.blockType == StellarObjectDataBlock.eBlockType.Lower)
                        //{
                        //    float alphaLevel = 0.0f;
                        //    StarData starData = text.stellarObject.GetComponent<Star>().starData;
                        //    Color scanColor = new Color();

                        //    if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
                        //    {
                        //        if (starData.IntelLevel == eStellarIntelLevel.None)
                        //            scanColor = Color.grey;
                        //        // get color of zoom level
                        //        else if (starData.IntelLevel == eStellarIntelLevel.Low)
                        //            scanColor = Color.red;
                        //        else if (starData.IntelLevel == eStellarIntelLevel.Medium)
                        //            scanColor = Color.yellow;
                        //        else
                        //            scanColor = Color.green;
                        //        alphaLevel = GetAlphaGalaxyFadeValue();
                        //        text.textObject.SetActive(true);
                        //        text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 10;
                        //        text.textObject.GetComponent<TextMeshProUGUI>().text = starData.IntelLevel.ToString().ToUpper() + " INTEL";

                        //        textColor = new Color(scanColor.r, scanColor.g, scanColor.b, alphaLevel);
                        //    }
                        //    if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
                        //    {


                        //        alphaLevel = GetAlphaProvinceFadeValue();
                        //        text.textObject.SetActive(true);
                        //        text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 10;
                        //        if (starData.AssignedProvinceID != "")
                        //        {
                        //            if (gameDataRef.DebugMode || starData.IntelValue == 10)
                        //            {
                        //                text.textObject.GetComponent<TextMeshProUGUI>().text = HelperFunctions.DataRetrivalFunctions.GetProvince(starData.AssignedProvinceID).Name.ToUpper() + " PROVINCE";
                        //                textColor = new Color(text.ownerColor.r, text.ownerColor.g, text.ownerColor.b, alphaLevel);
                        //            }
                        //            else
                        //            {
                        //                text.textObject.GetComponent<TextMeshProUGUI>().text = "UNKNOWN";
                        //                textColor = new Color(Color.grey.r, Color.grey.g, Color.grey.b, alphaLevel);
                        //            }
                        //        }
                        //        else
                        //        {
                        //            if (gameDataRef.DebugMode || starData.IntelValue == 10)
                        //                text.textObject.GetComponent<TextMeshProUGUI>().text = "NONE";
                        //            else
                        //                text.textObject.GetComponent<TextMeshProUGUI>().text = "UNKNOWN";
                        //            textColor = new Color(Color.grey.r, Color.grey.g, Color.grey.b, alphaLevel);
                        //        }
                        //    }
                        //    else
                        //    {
                                
                        //    }
                        //}
                        //else if (text.blockType == StellarObjectDataBlock.eBlockType.Province)
                        //{
                        //    float alphaLevel = 0.0f;
                        //    if (zoomLevel == ViewManager.eViewLevel.Province)
                        //    {
                        //        alphaLevel = GetAlphaProvinceFadeValue();
                        //        text.textObject.SetActive(true);

                        //        // set the size of the province label
                        //        if (text.provinceBounds.width + text.provinceBounds.height > 5000)
                        //        {
                        //            text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 100;
                        //        }
                        //        else if (text.provinceBounds.width + text.provinceBounds.height <= 1000)
                        //        {
                        //            text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 20;
                        //        }
                        //        else
                        //            text.textObject.GetComponent<TextMeshProUGUI>().fontSize = (int)((text.provinceBounds.width + text.provinceBounds.height) / 50);

                        //        // set size of bounds for province object
                        //        text.textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(text.textObject.GetComponent<TextMeshProUGUI>().fontSize * text.ownerName.Length, text.textObject.GetComponent<TextMeshProUGUI>().fontSize);

                        //        if (gameDataRef.DebugMode)
                        //        {
                        //            text.textObject.GetComponent<TextMeshProUGUI>().text = text.ownerName.ToUpper();
                        //            textColor = new Color(text.ownerColor.r, text.ownerColor.g, text.ownerColor.b, alphaLevel);
                        //        }
                        //        else
                        //        {
                        //            text.textObject.GetComponent<TextMeshProUGUI>().text = text.ownerName.ToUpper();
                        //            textColor = new Color(text.ownerColor.r, text.ownerColor.g, text.ownerColor.b, alphaLevel);
                        //        }

                        //    }
                        //}
                    }
                    else
                        textColor = Color.white;

                    // adjust text size of nebulas when zoomed out a lot
                    if (text.blockType == StellarObjectDataBlock.eBlockType.Nebula)
                    {
                        if (fov > 80)
                            text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 15;
                        else if (fov > 60)
                            text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 20;
                        else
                            text.textObject.GetComponent<TextMeshProUGUI>().fontSize = 30;
                    }

                    text.textObject.GetComponentInChildren<TextMeshProUGUI>().color = textColor; // change text to color of the owning civ

                    // reset location of data line blocks
                    Vector3 nameVector;
                    //if (text.blockType != StellarObjectDataBlock.eBlockType.Province)
                    //{
                        nameVector = Camera.main.WorldToScreenPoint(text.stellarObject.transform.position); // gets the screen point of the star's transform position
                    //}
                    //else
                    //{
                    //    nameVector = Camera.main.WorldToScreenPoint(text.provinceObjectLocation); // gets the screen point of the star's transform position
                    //}
                    //if (text.blockType == StellarObjectDataBlock.eBlockType.Star) // if a system data block
                    textLocation = nameVector;
                
                    //else if (text.blockType == StellarObjectDataBlock.eBlockType.Province) // if a province name
                    //{
                    //    textLocation = nameVector;
                    //    text.textObject.GetComponentInChildren<Image>().enabled = false; // turn off line
                    //    text.textObject.GetComponent<TextMeshProUGUI>().fontSizeMin = 15; // big province names!
                    //    text.textObject.GetComponent<TextMeshProUGUI>().enableAutoSizing = false; // big province names!
                    //}

                    //else // if lower system data block
                    //{                    
                    //    //textLocation = nameVector;
                    //    textLocation = new Vector3(nameVector.x, nameVector.y -13, 0); // where the text box is located
                    //    text.textObject.GetComponentInChildren<Image>().enabled = false; // turn off line
                    //    text.textObject.GetComponent<TextMeshProUGUI>().characterSpacing = 3; // shorten spacing of characters
                    //    text.textObject.GetComponent<TextMeshProUGUI>().fontSizeMax = 11; // shorten spacing of characters
                    //}
                    text.textObject.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)

                }
            }
        }

        private float GetAlphaProvinceFadeValue()
        {
            float alphaLevel = 0.0f;

            alphaLevel = 290 - (fov * 2.0f);
            alphaLevel = alphaLevel / 255;

            if (alphaLevel < 0)
                alphaLevel = 0;

            if (alphaLevel > .8)
                alphaLevel = 1;

            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Province)
                alphaLevel = 1;

            return alphaLevel;
        }

        private float GetAlphaGalaxyFadeValue()
        {
            float alphaLevel = 255f;

            alphaLevel = -800f + (fov * 17.2f); // was 8f
            alphaLevel = alphaLevel / 255f;
            if (alphaLevel < 0)
                alphaLevel = 0;
            if (alphaLevel > .9)
                alphaLevel = 1;

            return alphaLevel;
        }

        private Color CheckBioStatus(StarData starData)
        {
            bool hasGoodPlanets = false;
            bool hasOKPlanets = false;
            bool hasPoorPlanets = false;
            Color textColor = new Color();

            foreach (PlanetData planet in starData.PlanetList)  // test to check for changing label colors dynamically
            {
                if (starData.PlanetList.Count > 0)
                {
                    if (planet.Bio > 70)
                        hasGoodPlanets = true;
                    else if (planet.Bio > 40)
                        hasOKPlanets = true;
                    else
                        hasPoorPlanets = true;
                }
                else
                {
                    textColor = Color.grey;
                    return textColor;
                }
            }

            // set the color depending on planet bio
            if (hasGoodPlanets)
                textColor = Color.green;
            else if (hasOKPlanets)
                textColor = Color.yellow;
            else if (hasPoorPlanets)
                textColor = Color.red;
           
            return textColor;
        }

        void ClearSelectedPlanet()
        {
            //reset the selected planet by changing all tags to 'Planet'
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                planet.tag = "Planet";
            }
            uiManagerRef.selectedPlanet = null;
        }

        public GameObject GetSelectedPlanet() // find the selected planet in the system (will have to change eventually!)
        {
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                if (planet.tag == "Selected")
                {
                    return planet; // return the planet
                }
            }
            return null; // no star, error
        }

        void GenerateStellarDataBlocks()
        {
            Vector3 textLocation;
            Vector3 textLocation2;
           
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.GetComponent<Star>().tag != "Companion Star") // if not a companion star
                    {
                        string civNames = "";
                        string civTolerance = "";
                        bool civOwnerFound = false;
                        Color civColor = Color.grey;

                        var nameVector = Camera.main.WorldToScreenPoint(star.transform.position); // gets the screen point of the star's transform position
                        textLocation = new Vector3(nameVector.x, nameVector.y, 0); // where the text box is located
                        textLocation2 = new Vector3(nameVector.x, nameVector.y, 0); // where the lower text box is located

                        // create the text objects
                        GameObject systemUIDataLine = Instantiate(starNameObject, textLocation, Quaternion.identity) as GameObject;                      
                        StellarObjectDataBlock starDataBlock = new StellarObjectDataBlock();  // create a new star data block
                        //GameObject provinceUIDataLine = Instantiate(starNameObject, textLocation2, Quaternion.identity) as GameObject;
                        //StellarObjectDataBlock lowerProvinceDataBlock = new StellarObjectDataBlock(); // create the lower data block for province info

                        systemUIDataLine.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the blocks to the panel
                        //provinceUIDataLine.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true);  // attach the blocks to the panel

                        // add the owning civs if present      
                        string civNumber = "-1"; // for the icons
                                        
                        foreach (Civilization civ in gameDataRef.CivList)
                        {
                            List<StarData> civSystems = new List<StarData>();
                            civSystems = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                            if (!civOwnerFound)
                            {
                                foreach (StarData sys in civSystems)
                                {
                                    if (sys.ID == star.GetComponent<Star>().starData.ID)
                                    {
                                        bool systemIsCapital = false;
                                        foreach (PlanetData pData in sys.PlanetList)
                                        {
                                            if (pData.ID == civ.CapitalPlanetID)
                                            {
                                                systemIsCapital = true;
                                                break;
                                            }
                                        }
                                        if (systemIsCapital)
                                            civNames += " (" + civ.Name.ToUpper() + " HOME SYSTEM," + civ.Size.ToString() + ")";
                                        else
                                            civNames += " (" + civ.Name.ToUpper() + "," + civ.Size.ToString() + ")";

                                        civNames += " (" + civ.Type.ToString() + ")";
                                        if (sys.AssignedProvinceID != "")
                                            civNames += "(PROV NAME: " + HelperFunctions.DataRetrivalFunctions.GetProvince(sys.AssignedProvinceID).Name.ToUpper() + ")";
                                        civColor = civ.Color;
                                        civTolerance = "(TOL: " + civ.PlanetMinTolerance.ToString("N0") + ")";
                                        civNumber = civ.ID.Substring(3); // get the number from the ID of the civilization
                                        systemUIDataLine.transform.Find("Empire Icon").GetComponent<Image>().sprite = graphicAssets.EmpireCrestList.Find(p => p.name == "CREST" + civNumber); // attach the crest
                                        if (sys.IntelLevel > eStellarIntelLevel.Medium)
                                        {
                                            systemUIDataLine.transform.Find("Empire Icon").GetComponent<Image>().enabled = true;
                                            systemUIDataLine.transform.localScale = new Vector3(1, 1, 1); // do not scale
                                        }
                                        else
                                        {
                                            systemUIDataLine.transform.Find("Empire Icon").GetComponent<Image>().enabled = false;
                                            //systemUIDataLine.transform.localScale = new Vector3(1, 1, 1); // do not scale
                                        }
                                        civOwnerFound = true;
                                        break;
                                    }
                                    if (sys.IntelLevel > eStellarIntelLevel.Medium)
                                    {
                                        systemUIDataLine.transform.Find("Empire Icon").GetComponent<Image>().enabled = true;
                                        systemUIDataLine.transform.localScale = new Vector3(1, 1, 1); // do not scale
                                    }
                                    else
                                        systemUIDataLine.transform.localScale = new Vector3(.7f, .7f, 1); // scale systems with low intel down
                                }
                                
                            }      
                        }
                        if (!civOwnerFound)
                            systemUIDataLine.transform.Find("Empire Icon").GetComponent<Image>().enabled = false; // don't show icons of stars that don't have an owner!
                        if (!gameDataRef.DebugMode)
                            systemUIDataLine.GetComponentInChildren<TextMeshProUGUI>().text = star.GetComponent<Star>().starData.Name.ToUpper() + "[" + star.GetComponent<Star>().starData.IntelValue.ToString("N0") + "]"; //+ civNames;
                        else
                            systemUIDataLine.GetComponentInChildren<TextMeshProUGUI>().text = star.GetComponent<Star>().starData.Name.ToUpper() + civNames;
                        systemUIDataLine.GetComponentInChildren<TextMeshProUGUI>().color = civColor;
                        
                        systemUIDataLine.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2),textLocation.y - (Screen.height / 2),0); // reset after making a parent to canvas relative coordinates (pivot in center)
                        
                        systemUIDataLine.name = star.GetComponent<Star>().starData.ID;

                        //provinceUIDataLine.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)
                        //provinceUIDataLine.transform.localScale = new Vector3(1, 1, 1); // do not scale
                        //provinceUIDataLine.name = star.GetComponent<Star>().starData.ID;

                        // assign to the star data block
                        starDataBlock.objectRotation = 0f;
                        starDataBlock.ownerName = civNames;
                        starDataBlock.blockType = StellarObjectDataBlock.eBlockType.Star;
                        starDataBlock.stellarObject = star;
                        starDataBlock.ownerColor = civColor;
                        
                        starDataBlock.ownerTolerance = civTolerance;
                        starDataBlock.textObject = systemUIDataLine;
                        listTextObjectsCreated.Add(starDataBlock);
                    }
                }
            }

            //foreach (GameObject nebula in galaxyDataRef.stellarPhenonomaList)
            //{
                //if (nebula != null)
                //{
                //        var nameVector = Camera.main.WorldToScreenPoint(nebula.transform.position); // gets the screen point of the star's transform position
                //        Vector2 vectorTwo = GUIUtility.ScreenToGUIPoint(new Vector2(nameVector.x, nameVector.y)); // gets center of star on screen position
                //        textLocation = new Vector3(nameVector.x, nameVector.y, 0); // where the text box is located

                //        // create the text object
                //        GameObject stellarObjectName = Instantiate(starNameObject, textLocation, Quaternion.identity) as GameObject;
                //        StellarObjectDataBlock stellarObjectDataBlock = new StellarObjectDataBlock();  // create a new star data block
                        
                //        stellarObjectName.transform.SetParent(galaxyPlanetInfoCanvas.transform, true);
                //        stellarObjectName.GetComponentInChildren<TextMeshProUGUI>().text = nebula.GetComponent<Nebula>().nebulaData.Name.ToUpper();
                //        stellarObjectName.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 1); // reset after making a parent to canvas relative coordinates (pivot in center)
                //        stellarObjectName.transform.localScale = new Vector3(1, 1, 1); // do not scale
                //        stellarObjectName.GetComponentInChildren<TextMeshProUGUI>().characterSpacing = 40; // space out the letters
                //        stellarObjectName.GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;

                //        // assign to the star data block
                //        stellarObjectDataBlock.objectRotation = nebula.GetComponent<Nebula>().nebulaData.TextRotation;
                //        stellarObjectDataBlock.blockType = StellarObjectDataBlock.eBlockType.Nebula;
                //        stellarObjectDataBlock.stellarObject = nebula;
                //        stellarObjectDataBlock.textObject = stellarObjectName;
                //        listTextObjectsCreated.Add(stellarObjectDataBlock);

                //        stellarObjectName.transform.Rotate(new Vector3(0, 0, stellarObjectDataBlock.objectRotation));
                //}
            //}
            systemNameDrawn = true;        
        }

        void HideStellarObjectsNotInSelectedProvince()
        {
            Province selectedProvince = null;

            if (Camera.main.GetComponent<GalaxyCameraScript>().provinceTarget != null)
                selectedProvince = Camera.main.GetComponent<GalaxyCameraScript>().provinceTarget;
            else
                return;

            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.tag == "Star")
                    {
                        if (!selectedProvince.SystemList.Exists(p => p.ID == star.GetComponent<Star>().starData.ID))
                        {
                            star.GetComponent<SpriteRenderer>().enabled = false;
                            star.GetComponent<Light>().enabled = false;
                            if (star.GetComponent<LineRenderer>() != null)
                            {
                                star.GetComponent<LineRenderer>().enabled = false;
                            }


                            foreach (Transform child in star.transform)
                            {
                                child.GetComponent<SpriteRenderer>().enabled = false;
                                child.GetComponent<Light>().enabled = false;
                            }
                        }
                    }
                }
            }

            // hide data blocks
            foreach (StellarObjectDataBlock star in listTextObjectsCreated)
            {
                if (!selectedProvince.SystemList.Exists(p => p.ID == star.textObject.name))
                    star.textObject.SetActive(false);
            }
        }

        void HideStellarObjects()
        {
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.tag == "Star")
                    {
                        star.GetComponent<SpriteRenderer>().enabled = false;
                        star.GetComponent<Light>().enabled = false;
                        

                        foreach (Transform child in star.transform)
                        {
                            child.GetComponent<SpriteRenderer>().enabled = false;
                            child.GetComponent<Light>().enabled = false;
                        }
                    }
                }
            }

            foreach (GameObject nebula in galaxyDataRef.stellarPhenonomaList)
            {
                if (nebula != null)
                {
                    if (nebula.tag == "StellarObject")
                    {
                        nebula.GetComponent<SpriteRenderer>().enabled = false;
                        
                    }
                }
            }
        }

        void HideUnselectedPlanets()
        {
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                if (planet != null)
                {
                    if (planet.tag == "Planet")
                    {
                        planet.GetComponent<SpriteRenderer>().enabled = false;
                        foreach (Transform child in planet.transform)
                            child.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }

        void ShowStellarObjects()
        {
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.tag == "Star")
                    {
                        star.GetComponent<SpriteRenderer>().enabled = true;
                        star.GetComponent<Light>().enabled = true;
                        if (provinceLinesDrawn)
                        {
                            if (star.GetComponent<LineRenderer>() != null)
                            {
                                star.GetComponent<LineRenderer>().enabled = true;
                            }
                        }
                        foreach (Transform child in star.transform)
                        {
                            child.GetComponent<SpriteRenderer>().enabled = true;
                            child.GetComponent<Light>().enabled = true;
                        }
                    }
                }
            }

            foreach (GameObject nebula in galaxyDataRef.stellarPhenonomaList)
            {
                if (nebula != null)
                {
                    if (nebula.tag == "StellarObject")
                    {
                        nebula.GetComponent<SpriteRenderer>().enabled = true;                       
                    }
                }
            }
        }

        void ShowAllPlanets()
        {
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                if (planet != null)
                {
                    if (planet.tag == "Planet")
                    {
                        planet.GetComponent<SpriteRenderer>().enabled = true;
                        foreach (Transform child in planet.transform)
                            child.GetComponent<SpriteRenderer>().enabled = true;
                    }
                }
            }
        }

        void DisplayProvinceData()
        {
            HideStellarObjectsNotInSelectedProvince();
            selectedUnitInfoCanvas.SetActive(true); // show information panel
            
        }

        private Province GetSelectedProvince()
        {
            if (gCameraRef.provinceTarget != null)
            {
                return gCameraRef.provinceTarget;
            }
            else
                return null;
        }

        void DisplayPlanetData() // This and the display system data functions will need to move to their respective view classes for continuity purposes.
        {
            HideUnselectedPlanets();
            smallSystemGrid.SetActive(false); // hides the grid
            sysTrail.SetActive(false); // hides the trail        
        }

        void DisplaySystemData()
        {

            StarData starDat = GetSelectedStar().GetComponent<Star>().starData; // reference
            backingSphere.SetActive(true); // turn on the sphere

            if (fov < 80)  // hide stars during zoom in
            {
                GetSelectedStar().GetComponent<Light>().enabled = false; // cut the light
                gStyle.normal.textColor = new Color(1, 1, 1, ((30 - fov) * 10) / 255); // fade in the text color

                if (fov < 60)
                {
                    HideStellarObjects(); // hide stars around the star when in close         
                    starmapSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); // fade it to 0
                }

                // draw the labels
                var nameVector = Camera.main.WorldToScreenPoint(GetSelectedStar().transform.position);
                Vector2 vectorTwo = GUIUtility.ScreenToGUIPoint(new Vector2(nameVector.x, nameVector.y));

                // create system grid if not yet created
                if (!systemGridCreated)
                {
                    Vector3 position = new Vector3(GetSelectedStar().transform.position.x - 50, GetSelectedStar().transform.position.y, GetSelectedStar().transform.position.z);

                    smallSystemGrid = Instantiate(systemGrid, position, Quaternion.Euler(279, 180, 180)) as GameObject; // was 90,0,0, with top-down turn
                    smallSystemGrid.transform.localScale = new Vector3(smallSystemGrid.transform.localScale.x * ((float)Screen.width / 1920f), smallSystemGrid.transform.localScale.y * ((float)Screen.width / 1920f),
                        smallSystemGrid.transform.localScale.z * ((float)Screen.width / 1920f));
                    systemGridCreated = true;
                }
                else
                {
                    smallSystemGrid.SetActive(true);
                }

                // create system trail if not yet created
                if (!systemTrailCreated)
                {
                    Vector3 starPosition = GetSelectedStar().GetComponent<Star>().transform.position;
                    Vector3 trailLocation = new Vector3(starPosition.x - 35, starPosition.y, 0);
                    sysTrail = Instantiate(systemTrail, trailLocation, Quaternion.identity) as GameObject;
                    systemTrailCreated = true;
                }
                else
                {
                    sysTrail.SetActive(true); // shows the trail
                }

                // draw planets in system view if not yet created
                if (!planetsDrawn && (starDat.IntelLevel > eStellarIntelLevel.Low || gameDataRef.DebugMode))
                {
                    DrawPlanets(starDat);
                }

                smallSystemGrid.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, ((30 - fov) * 8) / 255);  // fade in the grid
                systemUICanvas.SetActive(true); // show the display
                selectedUnitInfoCanvas.SetActive(true); // show information panel  
            }
        }

        void DrawPlanets(StarData sData)
        {
            GameObject selectedStar = GetSelectedStar();
            Vector3 starPosition = GetSelectedStar().GetComponent<Star>().transform.position;

            for (int x = 0; x < 6; x++)
            {
                if (sData.PlanetSpots[x] != null)
                {
                    // add planet to draw, if a sprite exists
                    if (sData.PlanetSpots[x].PlanetSpriteNumber != -1)
                        switch (sData.PlanetSpots[x].Type)
                        {
                            case PlanetData.ePlanetType.AsteroidBelt:
                                {
                                    planetsToDraw[x] = asteroidBelts[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Barren:
                                {
                                    planetsToDraw[x] = barrenPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Greenhouse:
                                {
                                    planetsToDraw[x] = greenhousePlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Desert:
                                {
                                    planetsToDraw[x] = desertPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Terran:
                                {
                                    planetsToDraw[x] = terranPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Ice:
                                {
                                    planetsToDraw[x] = icePlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.IceGiant:
                                {
                                    planetsToDraw[x] = dwarfJovianPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.GasGiant:
                                {
                                    planetsToDraw[x] = jovianPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Lava:
                                {
                                    planetsToDraw[x] = lavaPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Irradiated:
                                {
                                    planetsToDraw[x] = irradiatedPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.SuperEarth:
                                {
                                    planetsToDraw[x] = superEarthPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Ocean:
                                {
                                    planetsToDraw[x] = waterPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.BrownDwarf:
                                {
                                    planetsToDraw[x] = brownDwarfPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.IceBelt:
                                {
                                    planetsToDraw[x] = iceBelts[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.DustRing:
                                {
                                    planetsToDraw[x] = dustRings[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                }
            }

            for (int x = 0; x < 5; x++)
            {
                if (planetsToDraw[x] != null)
                {
                    // planet draw routine
                    PlanetData pData = new PlanetData();
                    float startingPlanetMargin = 110;
                    float screenScaleFactor = (float)Screen.width / 1920f;
                    float screenWidthFactor = 0f;

                    if (Screen.width >= 1600)
                    {
                        screenWidthFactor = 15f;
                    }
                    else if (Screen.width >= 1400)
                    {
                        screenWidthFactor = 14f;
                    }
                    else if (Screen.width >= 1280)
                        screenWidthFactor = 14f;

                    else
                        screenWidthFactor = 12.3f;

                    pData = sData.PlanetSpots[x];

                    Vector3 position = new Vector3(starPosition.x + ((selectedStar.transform.localScale.x / 2) + (startingPlanetMargin * screenScaleFactor)) + ((x + 1) * ((float)Screen.width / screenWidthFactor)), starPosition.y, 10); // derive the planet position from the star
                    GameObject planet = Instantiate(planetsToDraw[x], position, Quaternion.Euler(0, 180, 0)) as GameObject; // draw the planet
                    planet.AddComponent<Planet>(); // add the planet container
                    planet.GetComponent<Planet>().planetData = pData; // and attach the data
                    planet.name = pData.Name;
                    planet.GetComponent<CircleCollider2D>().radius = 2.0f;
                    int size = 0;

                    // normalize the size of the object
                    if (pData.Type == PlanetData.ePlanetType.IceBelt || pData.Type == PlanetData.ePlanetType.AsteroidBelt || pData.Type == PlanetData.ePlanetType.IceBelt)
                        size = 50;
                    else
                        size = pData.Size;                        
                    
                    // size of the planet (need to increase)                                     
                    pData.PlanetSystemScaleSize = size *.25f; // set the planet's size as it should be in the system view (will probably set with the gameobject)

                    planet.transform.localScale = new Vector2(pData.PlanetSystemScaleSize, pData.PlanetSystemScaleSize); // scale the sprite depending on size
                    planet.transform.Rotate(180, 180, 0);
                    listSystemPlanetsCreated.Add(planet); // add the planet to the drawn list (for destruction later as a group)

                    //draw rings if they exist
                    if (pData.Rings)
                    {
                        GameObject ring = Instantiate(rings[pData.PlanetRingSpriteNumber], position, Quaternion.Euler(0, 0, 0)) as GameObject; // create the ring if it exists
                        ring.transform.SetParent(planet.transform, false); // set as child of planet
                        ring.transform.Rotate(new Vector3(0, 0, 0 + pData.PlanetRingTilt));
                        ring.transform.localScale = new Vector3(1, 1, 1);
                        ring.transform.localPosition = new Vector3(0, 0, 0);
                        ring.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    }
                }
            }
            planetsDrawn = true; // set flag
        }
    }

    public class StellarObjectDataBlock
    {
        public enum eBlockType : int
        {
            Star,
            Nebula,
            Province,
            Lower
        }

        [HideInInspector] public eBlockType blockType = eBlockType.Star;
        [HideInInspector] public GameObject stellarObject; // object that the block is tied to
        [HideInInspector] public GameObject textObject; // text object that the block is named
        [HideInInspector] public Vector2 provinceObjectLocation; // where the province information is located
        [HideInInspector] public Rect provinceBounds; // where the province information is located
        [HideInInspector] public GameObject planetCountObject; // future UI object for additional information
        [HideInInspector] public Color ownerColor; // civ ownership color
        [HideInInspector] public string ownerName; // civ name
        [HideInInspector] public string ownerTolerance; // civ planet tolerance
        [HideInInspector] public float objectRotation; // rotation of the block (for constellations and nebulas)

    }
}