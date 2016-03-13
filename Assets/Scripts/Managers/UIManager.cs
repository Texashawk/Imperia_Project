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

        // constants for zoom levels 
        public const int galaxyMinZoomLevel = 120;
        public const int provinceMinZoomLevel = 40;
        public const int systemMinZoomLevel = 12;
        public const int planetMinZoomLevel = 3;
        public const int maxZoomLevel = 135;
        public const int minZoomLevel = 30;

        public eViewMode ViewMode;   // public accessor for current view mode in the game

        // Use this for initialization
        void Awake()
        {
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>(); // get global game data (date, location, version, etc)
            gCameraRef = mainCamera.GetComponent<GalaxyCameraScript>(); // get camera ref
            selectedItemPanel = GameObject.Find("Selected Item Panel"); // selected item panel
        }

        // Update is called once per frame
        void Update()
        {           
            if (ViewMode == eViewMode.Galaxy || ViewMode == eViewMode.Province)
            {
                selectedItemPanel.SetActive(false);
            }
            else
            {
                selectedItemPanel.SetActive(true);
            }
        }

        void LateUpdate()
        {
            cameraFOV = mainCamera.fieldOfView;
            ViewMode = gCameraRef.ZoomLevel; // converts the camera FOV to current view mode
        }
    }
}
