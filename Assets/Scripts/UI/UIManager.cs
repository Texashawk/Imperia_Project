using UnityEngine;
using System.Collections;
using CameraScripts;

namespace UI.Manager
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
        private float cameraFOV; // current FOV of the camera

        public Transform SelectedStellarObject; // what current stellar object is active

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
        }

        // Update is called once per frame
        void LateUpdate()
        {
            cameraFOV = mainCamera.fieldOfView;
            ViewMode = gCameraRef.ZoomLevel; // converts the camera FOV to current view mode
        }
    }
}
