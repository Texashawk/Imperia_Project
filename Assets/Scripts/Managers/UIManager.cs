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

        private Camera mainCamera; // accessor for the camera
        private GalaxyCameraScript gCameraRef; // accessor for the camera script
        private GameObject selectedItemPanel; // reference for the selected item panel
        private float cameraFOV; // current FOV of the camera

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

        public ViewManager.eViewLevel ViewMode;   // public accessor for current view mode in the game

        // Use this for initialization
        void Awake()
        {
            viewManagerRef = GameObject.Find("UI Engine").GetComponent<ViewManager>();
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>(); // get global game data (date, location, version, etc)
            gCameraRef = mainCamera.GetComponent<GalaxyCameraScript>(); // get camera ref
            selectedItemPanel = GameObject.Find("Selected Item Panel"); // selected item panel
        }

        // Update is called once per frame
        void Update()
        {
            ViewMode = viewManagerRef.ViewLevel;

            // Show the selected item panel (won't be here though, need to move to the actual script
            if (ViewMode == ViewManager.eViewLevel.Galaxy|| ViewMode == ViewManager.eViewLevel.Province)
            {
                selectedItemPanel.SetActive(false);
            }
            else
            {
                selectedItemPanel.SetActive(true);
            }
        }

        public void SetActiveViewLevel(ViewManager.eViewLevel viewMode)
        {
            viewManagerRef.ViewLevel = viewMode;
            ViewMode = viewManagerRef.ViewLevel;
        }

        public void SetActiveSubMode(ViewManager.eSubView subView)
        {
            viewManagerRef.SubModeView = subView;
        }

        public void SetActiveView(ViewManager.ePrimaryView activeView)
        {
            viewManagerRef.PrimaryGalaxyViewMode = activeView;
        }

        void LateUpdate()
        {
            cameraFOV = mainCamera.fieldOfView;
            //viewMode = gCameraRef.ZoomLevel; // converts the camera FOV to current view mode
        }
    }
}
