using UnityEngine;
using System.Collections;
using StellarObjects;
using CameraScripts;

namespace Managers  
{

    public class UIManager : MonoBehaviour
    { 
        public enum eViewMode : int
        { 
            Galaxy,
            Province,
            System,
            Planet
        }

        //private Camera mainCamera; // accessor for the camera
        //private GalaxyCameraScript gCameraRef; // accessor for the camera script
        
        //private float cameraFOV; // current FOV of the camera

        public bool RequestTradeViewGraphicRefresh = false; // the game is asking for a graphic request throughout the UI
        public bool RequestPoliticalViewGraphicRefresh = false;

        public Transform SelectedStellarObject; // what current stellar object is active
        public PlanetData selectedPlanet; // if a planet is selected, what kind
        public StarData selectedSystem; // if a system is selected, what kind
        public ViewManager viewManagerRef; // reference to the ViewManager

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

        // view references


        // Use this for initialization
        void Awake()
        {
            viewManagerRef = GameObject.Find("GameManager").GetComponent<ViewManager>();
            //mainCamera = Camera.main; // GameObject.Find("Main Camera").GetComponent<Camera>(); // get global game data (date, location, version, etc)
            //gCameraRef = mainCamera.GetComponent<GalaxyCameraScript>(); // get camera ref
           
        }

        void Start()
        {
            // set the initial views when game starts
            SetActiveViewLevel(ViewManager.eViewLevel.Galaxy);
            SetActivePrimaryMode(ViewManager.ePrimaryView.Political);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Sovereignity);
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
            RequestPoliticalViewGraphicRefresh = true;
        }

        public void SetPrimaryModeToEconomic()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Economic);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Trade); // for testing
            RequestTradeViewGraphicRefresh = true;
            
        }

        public void SetPrimaryModeToMilitary()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Military);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Military); // for testing
        }

        public void SetPrimaryModeToPops()
        {
            SetActivePrimaryMode(ViewManager.ePrimaryView.Pops);
            SetActiveSecondaryMode(ViewManager.eSecondaryView.Morale); // for testing
        }

        public void SetActiveViewLevel(ViewManager.eViewLevel viewMode)
        {
            viewManagerRef.ViewLevel = viewMode;
            ViewLevel = viewManagerRef.ViewLevel;
        }

        public void SetActivePrimaryMode(ViewManager.ePrimaryView subView)
        {
            viewManagerRef.PrimaryViewMode = subView;
        }

        public void SetActiveSecondaryMode(ViewManager.eSecondaryView activeView)
        {
            viewManagerRef.SecondaryViewMode = activeView;
        }

        public void RequestGraphicRefresh()
        {
            RequestPoliticalViewGraphicRefresh = true;
            RequestTradeViewGraphicRefresh = true;
        }

        void LateUpdate()
        {
            //cameraFOV = mainCamera.fieldOfView;
            //viewMode = gCameraRef.ZoomLevel; // converts the camera FOV to current view mode
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
