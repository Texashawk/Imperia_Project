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
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();

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
                            star.GetComponent<LineRenderer>().enabled = true;
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
                    lr.SetPosition(0, new Vector3(sData.WorldLocation.x, sData.WorldLocation.y, 0));
                    lr.material = provinceLineTexture;
                    lr.SetColors(gameDataRef.CivList[0].Color, gameDataRef.CivList[0].Color);
                    lr.SetWidth(15f, 15f);

                    // loop through each sorted list and draw the vertexes
                    for (int y = 1; y < sortedTopQuadrantList.Count; y++)
                    {
                        lr.SetPosition(y, new Vector3(sortedTopQuadrantList[y].x, sortedTopQuadrantList[y].y, 0));
                        lr.SetWidth(15f, 15f);
                    }

                    for (int y = sortedTopQuadrantList.Count; y < pData.SystemList.Count; y++)
                    {
                        lr.SetPosition(y, new Vector3(sortedBottomQuadrantList[y-sortedTopQuadrantList.Count].x, sortedBottomQuadrantList[y-sortedTopQuadrantList.Count].y, 0));
                        lr.SetWidth(15f, 15f);
                    }

                    lr.SetPosition(pData.SystemList.Count, new Vector3(sortedTopQuadrantList[0].x, sortedTopQuadrantList[0].y, 0)); // the final position
                    lr.SetWidth(15f, 15f);
                }
            }
            provinceLinesDrawn = true;
        }

        void LateUpdate()
        {
            FadeOutStarMapOnZoom();     
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

            // scaling routine
            ScaleStars();

            // if camera zooms out too far, camera is considered in galaxy mode (use this in base camera script)
            if (zoomLevel != ViewManager.eViewLevel.System && gameDataRef.StarSelected == true && mainCamera.GetComponent<GalaxyCameraScript>().systemZoomActive == false && mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive == false)
            {
                ResetGalaxyView();
            }

            if (zoomLevel == ViewManager.eViewLevel.Galaxy)
            {
                ShowProvinceLines();
            }
        }

        void ScaleStars()
        {

            float mainStarScale = 20f;

            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                float scaleFactor = star.GetComponent<Star>().starData.Size / mainStarScale;
                float zoomScaleFactor = star.GetComponent<Star>().starData.Size / 40f;
                float screenFactor = Screen.width / 1920f;

                // normalize the scale
                if (scaleFactor < 1.5f)
                    scaleFactor = 1.5f;

                if (scaleFactor > 4.5f)
                    scaleFactor = 4.5f;

                if (star.tag == "Selected") // zoomed star is bigger
                {
                    star.transform.localScale = new Vector2((16f * scaleFactor) * screenFactor, ((16f * scaleFactor) * screenFactor));

                    // scale the binary/trinary children properly
                    foreach (Transform child in star.transform)
                    {
                        scaleFactor = child.GetComponent<Star>().starData.Size / 25f;
                        // normalize the scale
                        if (scaleFactor < .75f)
                            scaleFactor = .75f;
                        child.localScale = new Vector2(.5f * zoomScaleFactor, .5f * zoomScaleFactor);
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
                        scaleFactor = child.GetComponent<Star>().starData.Size / 25f;

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
            planetsDrawn = false; // test
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
            //eventScrollView.SetActive(false);
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
                        StartSystemZoom(hit.transform);
                    }
                }
            }
        }

        private void StartSystemZoom(Transform target)
        {
            Camera.main.GetComponent<GalaxyCameraScript>().starTarget = target;
            Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = true;
            Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
            Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = false;
            gameDataRef.StarSelected = true; // probably need to move to a global UI manager
            uiManagerRef.selectedSystem = target.GetComponent<Star>().starData;
            uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.System);
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
                        StarData starDat = star.GetComponent<Star>().starData;

                        if (!planetsDrawn)
                        {
                            // draw planets in system view if not yet created
                            if ((starDat.IntelLevel > eStellarIntelLevel.Low || gameDataRef.DebugMode))
                            {
                                DrawPlanets(starDat, star);
                            }
                        }
                    }
                }
            }

            planetsDrawn = true;

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
                    //DrawPlanets(starDat);
                }

                smallSystemGrid.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, ((30 - fov) * 8) / 255);  // fade in the grid
                systemUICanvas.SetActive(true); // show the display
                selectedUnitInfoCanvas.SetActive(true); // show information panel  
            }
        }

        void DrawPlanets(StarData sData, GameObject star)
        {
            //GameObject selectedStar = GetSelectedStar();
            //Vector3 starPosition = GetSelectedStar().GetComponent<Star>().transform.position;
            Vector3 starPosition = sData.WorldLocation;
            Array.Clear(planetsToDraw, 0, 5); // clear the array each time
            for (int x = 0; x < 5; x++)
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

                    Vector3 position = new Vector3(starPosition.x + ((star.transform.localScale.x / 2) + (startingPlanetMargin * screenScaleFactor)) + ((x + 1) * ((float)Screen.width / screenWidthFactor)), starPosition.y, 10); // derive the planet position from the star
                    //Vector3 position = new Vector3(starPosition.x + ((selectedStar.transform.localScale.x / 2) + (startingPlanetMargin * screenScaleFactor)) + ((x + 1) * ((float)Screen.width / screenWidthFactor)), starPosition.y, 10); // derive the planet position from the star
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
            //planetsDrawn = true; // set flag
        }
    }
}