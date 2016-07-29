using UnityEngine;
using System.Collections;
using StellarObjects;
using Managers;
using CameraScripts;

namespace Managers  
{
    public class UIManager : MonoBehaviour
    { 
             
        public float cameraFOV; // current FOV of the camera

        public bool RequestTradeViewGraphicRefresh { get; set; } // the game is asking for a graphic request throughout the UI
        public bool RequestPoliticalViewGraphicRefresh { get; set; }
        public bool RequestProjectBarGraphicRefresh { get; set; }
        public bool RequestSovViewGraphicRefresh { get; set; }
        public bool ModalIsActive { get; set; }

        public Transform SelectedStellarObject; // what current stellar object is active
        public PlanetData selectedPlanet; // if a planet is selected, what kind
        public StarData selectedSystem; // if a system is selected, what kind
        public ViewManager viewManagerRef; // reference to the ViewManager
        public GameData gDataRef;
        public UIManager uiManagerRef;

        public GameObject ProjectScreen; // the Project Screen Prefab goes here
        public GameObject CharacterScreen; // the Character Screen Prefab goes here

        // constants for zoom levels 
        public const int galaxyMinZoomLevel = 120;
        public const int provinceMinZoomLevel = 40;
        public const int systemMinZoomLevel = 12;
        public const int planetMinZoomLevel = 3;
        public const int maxZoomLevel = 135;
        public const int minZoomLevel = 30;

        public ViewManager.eViewLevel ViewLevel;   // public accessor for current view mode in the game
        public ViewManager.eSecondaryView SecondaryViewMode; // primary 'main' view category
        public ViewManager.ePrimaryView PrimaryViewMode; // sub mode

        // Use this for initialization
        void Awake()
        {
            viewManagerRef = GameObject.Find("GameManager").GetComponent<ViewManager>();
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
            gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            RequestProjectBarGraphicRefresh = false;
            RequestTradeViewGraphicRefresh = false;
            RequestPoliticalViewGraphicRefresh = false;
            RequestSovViewGraphicRefresh = true; // first mode
        }

        void Start()
        {
            // set the initial views when game starts
            SetActiveViewLevel(ViewManager.eViewLevel.Galaxy);
            SetPrimaryModeToPolitical();
        }

        // Update is called once per frame
        void Update()
        {
            ViewLevel = viewManagerRef.ViewLevel;
            SecondaryViewMode = viewManagerRef.SecondaryViewMode;
            PrimaryViewMode = viewManagerRef.PrimaryViewMode;
        }

        // status change of primary mode for events from buttons
        public void SetPrimaryModeToPolitical()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Political);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Sovereignity); // for testing
            RequestGraphicRefresh();
            RequestProjectBarRefresh();
            
        }

        public void SetPrimaryModeToEconomic()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Economic);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Trade); // for testing
            RequestGraphicRefresh();
            RequestProjectBarRefresh();
            
        }

        public void SetPrimaryModeToMilitary()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Military);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Military); // for testing
            RequestGraphicRefresh();
            RequestProjectBarRefresh();
        }

        public void SetPrimaryModeToPops()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Demographic);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Sovereignity); // for testing
            RequestGraphicRefresh();
            RequestProjectBarRefresh();
        }

        public void SetActiveViewLevel(ViewManager.eViewLevel viewMode)
        {
            viewManagerRef.ViewLevel = viewMode;
            ViewLevel = viewManagerRef.ViewLevel;
        }

        public void ActivateCharacterScreen(string cID)
        {
            GameObject cScreen = Instantiate(CharacterScreen, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            Canvas canvasRef = GameObject.Find("Main UI Overlay").GetComponent<Canvas>();
            uiManagerRef.ModalIsActive = true; // set modal
            cScreen.transform.SetParent(canvasRef.transform);
            cScreen.transform.localScale = new Vector3(.6f, .6f, .6f);
        }

        public void ActivateProjectScreen(string pID, string lID)
        {
            Debug.Log("Project ID " + pID + " clicked! Drawing project screen here...");
            GameObject pScreen = Instantiate(ProjectScreen, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            pScreen.GetComponent<ProjectScreenScript>().ProjectID = pID;       // sends the project ID to the window to look up   
            pScreen.GetComponent<ProjectScreenScript>().LocationID = lID;       // sends the location of the Project to the window to look up
            Canvas canvasRef = GameObject.Find("Main UI Overlay").GetComponent<Canvas>();         
            uiManagerRef.ModalIsActive = true; // set modal
            pScreen.transform.SetParent(canvasRef.transform);
            pScreen.transform.localScale = new Vector3(.6f, .6f, .6f);           
        }

        public void SetActivePrimaryMode(ViewManager.ePrimaryView subView)
        {
            viewManagerRef.PrimaryViewMode = subView;
        }

        public void SetActiveSecondaryMode(ViewManager.eSecondaryView activeView)
        {
            viewManagerRef.SecondaryViewMode = activeView;
        }

        public void RequestProjectBarRefresh()
        {
            RequestProjectBarGraphicRefresh = true;
        }

        public void RequestGraphicRefresh()
        {
            RequestPoliticalViewGraphicRefresh = true;
            RequestProjectBarGraphicRefresh = true;
            RequestSovViewGraphicRefresh = true;
            RequestTradeViewGraphicRefresh = true;
            gDataRef.RequestGraphicRefresh = true;
        }

        void LateUpdate()
        {
            cameraFOV = Camera.main.fieldOfView;
        }

        public class StellarObjectDataBlock // block object used to show data in the galaxy view
        {
            public enum eBlockType : int
            {
                Star,
                Nebula,
                Province,
                Lower
            }

            [HideInInspector]
            public eBlockType blockType = eBlockType.Star;
            [HideInInspector]
            public GameObject stellarObject; // object that the block is tied to
            [HideInInspector]
            public GameObject textObject; // text object that the block is named
            [HideInInspector]
            public string starID = "";
            public Vector2 provinceObjectLocation; // where the province information is located
            [HideInInspector]
            public Rect provinceBounds; // where the province information is located
            [HideInInspector]
            public GameObject planetCountObject; // future UI object for additional information
            [HideInInspector]
            public Vector3 objectLocation; // where the block is
            public Color ownerColor; // civ ownership color
            [HideInInspector]
            public string ownerName; // civ name
            [HideInInspector]
            public string ownerTolerance; // civ planet tolerance
            [HideInInspector]
            public float objectRotation; // rotation of the block (for constellations and nebulas)

        }
    }
}
