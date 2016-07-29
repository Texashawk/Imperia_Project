using UnityEngine;
using System.Collections;
using StellarObjects;
using UnityEngine.UI;
using Managers;

namespace CameraScripts
{
    public class GalaxyCameraScript : MonoBehaviour
    {
	    private float mouseWheelValue = 0.0f;
	    private bool moveMapUp = false;
	    private bool moveMapDown = false;
	    private bool moveMapRight = false;
	    private bool moveMapLeft = false;
        private bool panActive = false;
        [HideInInspector]public bool ScrollWheelIsValid { get; set; } // can the scrollwheel be used to manipulate the camera?
	    private float zoomSpeed = 11f;
        private float zoomSensitivity = 60f;
        public const float cameraTilt = 25f; // was 20f
        public float LastZoomValue = 0f; // saved zoom from when going into system
        private Camera mainC; // camera reference

        // for late updates
        private float targetCameraZPosition = 0f;
        private float targetCameraYPosition = 0f;
        private float targetCameraXPosition = 0f;
        private float targetCameraXRotation = 0f;
        public Vector3 TargetCameraPosition = new Vector3();

        private GameData gDataRef;
        private UIManager uiManagerRef;

        [HideInInspector]public ViewManager.eViewLevel ZoomLevel;
	    private const int scrollSpeedVariable = 4000;
	    [HideInInspector]public bool systemZoomActive = false;
        [HideInInspector]public bool planetZoomActive = false;
        [HideInInspector]public bool provinceZoomActive = false;
        public bool systemZoomComplete = false;
        private bool planetZoomComplete = false;
        private bool provinceZoomComplete = false;
        private bool planetToSystemZoom = false;
	    public Transform starTarget = null;
        public Transform planetTarget = null;
        public Province provinceTarget = null;
        private Vector3 cameraSystemPosition; // stores the camera position when in system view
        private Vector3 cameraPlanetPosition; // stores the camera position when in planet view
        //private Vector3 cameraProvincePosition; // stores the camera position when in province view
        private float scaleRatio;

        private float tiltYOffset = 0f; // this is the offsetY that is used to counteract the tilt of the camera
        public int galaxyHeight;
        public int galaxyWidth;
        public const int galaxyZValue = 16000; // how far away the camera is from the galaxy map on a Z axis (8000 normal)
        private const int systemZValue = 2000; // how far the camera is away from the system planet views

        public const int galaxyMinZoomLevel = 15;
        public const int systemMinZoomLevel = 12;
        public const int planetMinZoomLevel = 1;
        public const int maxZoomLevel = 60; // 60 normal
        public const int minZoomLevel = 12;

        public float zoom;
       
	    // Use this for initialization
	    void Start ()
        {
            mainC = GameObject.Find("Main Camera").GetComponent<Camera>();
            gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
            mainC.fieldOfView = maxZoomLevel;
            zoom = mainC.fieldOfView;
            galaxyHeight = gDataRef.GalaxySizeHeight - 2500;
            galaxyWidth = gDataRef.GalaxySizeWidth - 4000;
            ScrollWheelIsValid = true;
            uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.Galaxy);
            uiManagerRef.SetActiveSecondaryMode(ViewManager.eSecondaryView.Sovereignity);
            float tangent = Mathf.Tan(cameraTilt * Mathf.Deg2Rad);
            tiltYOffset = galaxyZValue * tangent; // move the camera initially

            // initialize the main camera
            mainC.transform.position = new Vector3(0, 0 + tiltYOffset, galaxyZValue);
            mainC.transform.eulerAngles = new Vector3(180 - cameraTilt, 0, 0);
            targetCameraXPosition = mainC.transform.position.x;
            targetCameraYPosition = mainC.transform.position.y;
            targetCameraZPosition = mainC.transform.position.z;
        }
	
	    // Update is called once per frame
        void Update()
        {
            scaleRatio = (Screen.height / 1920f) * (Screen.width / 1080f);
            TargetCameraPosition = new Vector3(targetCameraXPosition, targetCameraYPosition, targetCameraZPosition);

            if (gDataRef.uiSubMode == GameData.eSubMode.None && !uiManagerRef.ModalIsActive)  // only work in no submode
            {
                // run movement/zoom functions      
                UpdateCamera();         
                CheckForMapPan();         
                PanMap();
                CheckForMapZoom();
                DetermineZoomLevel(); // update zoom level of camera

                if (provinceZoomActive && !systemZoomActive && provinceTarget != null)         
                    ZoomToProvince(provinceTarget);

                if (systemZoomActive && !planetZoomActive && !systemZoomComplete && starTarget != null)
                    ZoomToSystem(starTarget);                          

                if (systemZoomActive && planetZoomActive && planetTarget != null)
                    ZoomToPlanet(planetTarget);

                if (provinceZoomActive)           
                    if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                    {
                        transform.position = new Vector3(provinceTarget.ProvinceCenter.x, provinceTarget.ProvinceCenter.y + tiltYOffset, galaxyZValue);
                        provinceZoomActive = false;
                        provinceZoomComplete = false;
                        provinceTarget = null;
                        zoom = maxZoomLevel;
                        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.Galaxy); // resets view level to galaxy
                        uiManagerRef.RequestGraphicRefresh();
                    }
            
                if (systemZoomActive)
                    if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                    {
                        Vector3 targetPosition = new Vector3();   
                        //targetPosition = new Vector3(transform.position.x - 38 - (Screen.width / 10), transform.position.y + tiltYOffset, galaxyZValue); // first set the x/y position of the camera
                        targetPosition = new Vector3(starTarget.position.x, starTarget.position.y + tiltYOffset, galaxyZValue);
                        systemZoomActive = false;
                        provinceZoomActive = false;
                        provinceZoomComplete = false;
                        zoom = LastZoomValue;

                        targetCameraXPosition = targetPosition.x;
                        targetCameraYPosition = targetPosition.y;
                        targetCameraZPosition = targetPosition.z;                     
                        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.Galaxy); // resets view level to the galaxy
                        uiManagerRef.RequestGraphicRefresh(); // reset settings when coming back to galaxy view
                        systemZoomComplete = false;                      
                    }

                if (planetZoomActive)
                    if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                    {
                        planetTarget.transform.localScale = new Vector3(planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize,planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize, 
                            planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize); // reset the scale
                        if (planetTarget.GetComponent<SpriteRenderer>() != null)
                        {
                            planetTarget.Rotate(0, 0, 90);
                        }
                        planetTarget = null;
                        planetZoomActive = false;
                        planetZoomComplete = false;
                        provinceZoomActive = false;
                        provinceZoomComplete = false;
                        systemZoomActive = true;
                        systemZoomComplete = false;
                        zoom = systemMinZoomLevel;
                        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.System);
                        uiManagerRef.RequestGraphicRefresh();
                    }
            }
        }

        void CheckForMapZoom()
        {       
            if (!systemZoomActive && !planetZoomActive && !provinceZoomActive)
            {
                // keyboard uses the zoom
                if (Input.GetKeyDown(KeyCode.KeypadPlus) || (Input.GetKeyDown(KeyCode.R)))
                    zoom -= 10;

                if (Input.GetKeyDown(KeyCode.KeypadMinus) || (Input.GetKeyDown(KeyCode.F)))
                    zoom += 10;

                if (ScrollWheelIsValid)
                {
                    zoom -= mouseWheelValue * zoomSensitivity;
                    zoom = Mathf.Clamp(zoom, minZoomLevel, maxZoomLevel);
                }
            }

            // set normal angle of camera
            if (uiManagerRef.ViewLevel != ViewManager.eViewLevel.Galaxy && systemZoomActive)
            {
                //targetCameraXRotation = 180f;
                transform.eulerAngles = new Vector3(180, 0, 0); // was 30,0,0
            }

            else
            {
                //targetCameraXRotation = (180f - cameraTilt);
                transform.eulerAngles = new Vector3(180 - cameraTilt, 0, 0); // was 30,0,0
            }
        }

        void CheckForMapPan()
        {
            if (ZoomLevel == ViewManager.eViewLevel.Galaxy) // don't pan the map if in system or planet view or if in province mode
            {
                panActive = false;
                // check for map movement
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= (Screen.height - 5))
                {
                    moveMapUp = true;
                    panActive = true;
                }
                else
                    moveMapUp = false;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= 5)
                {
                    moveMapLeft = true;
                    panActive = true;
                }
                else
                    moveMapLeft = false;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= (Screen.width - 5))
                {
                    moveMapRight = true;
                    panActive = true;
                }
                else
                    moveMapRight = false;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= 5)
                {
                    moveMapDown = true;
                    panActive = true;
                }
                else
                    moveMapDown = false;
            }
        }

        void PanMap()
        {
            // scroll map (need to add bounds checks)          
            float yLocation = transform.position.y;
            float xLocation = transform.position.x;

            float moveUpVector = transform.position.y - scrollSpeedVariable;
            float moveDownVector = transform.position.y + scrollSpeedVariable;
            float moveRightVector = transform.position.x + scrollSpeedVariable;
            float moveLeftVector = transform.position.x - scrollSpeedVariable;
            
            if (ZoomLevel == ViewManager.eViewLevel.Galaxy) // don't pan map if in system or planet mode
            {
                if (moveMapUp && ((yLocation <= galaxyHeight + Mathf.Round(tiltYOffset)) && (yLocation >= -galaxyHeight + Mathf.Round(tiltYOffset))))     
                    targetCameraYPosition = Mathf.Lerp(yLocation, moveUpVector, Time.deltaTime * 3);
                
                if (moveMapDown && ((yLocation <= galaxyHeight + tiltYOffset) && (yLocation >= -galaxyHeight + tiltYOffset)))                               
                    targetCameraYPosition = Mathf.Lerp(yLocation, moveDownVector, Time.deltaTime * 3);               
                
                if (moveMapRight && ((xLocation <= galaxyWidth) && (xLocation >= -galaxyWidth)))                            
                    targetCameraXPosition = Mathf.Lerp(xLocation, moveRightVector, Time.deltaTime * 3);              

                if (moveMapLeft && ((xLocation <= galaxyWidth) && (xLocation >= -galaxyWidth)))                               
                    targetCameraXPosition = Mathf.Lerp(xLocation, moveLeftVector, Time.deltaTime * 3);                

                // normalize to prevent scrolling outside of map
                if (transform.position.x > galaxyWidth)
                    targetCameraXPosition = galaxyWidth;
                if (transform.position.x < -galaxyWidth)
                    targetCameraXPosition = -galaxyWidth;
               
                if (transform.position.y > galaxyHeight + tiltYOffset)
                    targetCameraYPosition = galaxyHeight + tiltYOffset;
                if (transform.position.y < -galaxyHeight + tiltYOffset)
                    targetCameraYPosition = -galaxyHeight + tiltYOffset;
            }
        }

        void UpdateCamera()
        {
            
            float xCameraPosition = 0f;
            float yCameraPosition = 0f;
            float zCameraPosition = 0f;
            float xCameraRotation = 0f;
            float panSpeed = 0f;
           
            mouseWheelValue = Input.GetAxis("Mouse ScrollWheel");
            mainC.fieldOfView = Mathf.Lerp(mainC.fieldOfView, zoom, Time.deltaTime * zoomSpeed);

            if (panActive)  // scrolling speed
                panSpeed = 125f;
            
            else // zoom in speed
                panSpeed = 12f;

            // experimental - remove it this doesn't work (zooming and moving the camera by update instead of by coroutine)
            xCameraPosition = Mathf.Lerp(mainC.transform.position.x, targetCameraXPosition, Time.deltaTime * panSpeed);
            yCameraPosition = Mathf.Lerp(mainC.transform.position.y, targetCameraYPosition, Time.deltaTime * panSpeed);
            zCameraPosition = Mathf.Lerp(mainC.transform.position.z, targetCameraZPosition, Time.deltaTime * (panSpeed / 1.5f));
            //xCameraRotation = Mathf.Lerp(mainC.transform.eulerAngles.x, targetCameraXRotation, Time.deltaTime * 2f);

            // now set the position/rotation gradually
            mainC.transform.position = new Vector3(xCameraPosition, yCameraPosition, zCameraPosition);
           // mainC.transform.eulerAngles = new Vector3(xCameraRotation, 0,0);          
        }

        void ZoomToProvince(Province target)
        {
            float provWidth = target.ProvinceBounds.width;
            float provHeight = target.ProvinceBounds.height;
            float constraintBound = 0f;

            // determine which side is larger; that will set the max zoom so that the whole province is seen
            if (provWidth > provHeight)
                constraintBound = provWidth;
            else
                constraintBound = provHeight;

            // zoom to the province
            if (!provinceZoomComplete)
            {
                Vector3 tgtPosition = target.ProvinceCenter;
                zoom = 30 + constraintBound / 120; // test, will need to account for non-linear zoom
            }

            provinceZoomComplete = true;
        }

	    void ZoomToSystem(Transform target)
	    {
            
            Vector3 tgtPosition;
            int leftCameraMove = 0;

            if (Screen.width >= 1600)
                leftCameraMove = 380;
            else if (Screen.width >= 1280)
                leftCameraMove = 410;
            else
                leftCameraMove = 430;

            if (!systemZoomComplete)
            {
                if (!planetToSystemZoom)
                {
                    tgtPosition = new Vector3(target.position.x + (leftCameraMove * (Screen.width / 1920f)) + (target.localScale.x / 2), target.position.y + 120 * (Screen.height / 1080f), systemZValue);
                    zoom = systemMinZoomLevel; // set system view zoom level
                    targetCameraXPosition = tgtPosition.x;
                    targetCameraYPosition = tgtPosition.y;
                    targetCameraZPosition = tgtPosition.z;                
                }            
                else
                {
                    tgtPosition = new Vector3(target.position.x + (leftCameraMove * (Screen.width / 1920f)) + (target.localScale.x / 2), target.position.y + 120 * (Screen.height / 1080f), systemZValue);
                    transform.position = new Vector3 (cameraPlanetPosition.x,cameraPlanetPosition.y, cameraPlanetPosition.z);
                    
                    zoom = systemMinZoomLevel; // set system view zoom level
                    targetCameraXPosition = tgtPosition.x;
                    targetCameraYPosition = tgtPosition.y;
                    targetCameraZPosition = tgtPosition.z;
                    planetToSystemZoom = false;
                }

                target = null; // reset the target
                TargetCameraPosition = new Vector3(targetCameraXPosition, targetCameraYPosition, targetCameraZPosition); // for system view
                systemZoomComplete = true;
                       
            }   
	    }

        void ZoomToPlanet(Transform planet) // need to tweak where the planet shows up on the screen (aiming for lower-left)
        {       
            if (!planetZoomComplete)
            {                
                Vector3 tgtPosition = new Vector3(planet.position.x, planet.position.y - (35f * scaleRatio), systemZValue);
                if (planet.GetComponent<MeshRenderer>() != null)
                {
                    //planet.localScale = new Vector3(120 * scaleRatio, 120 * scaleRatio, 120 * scaleRatio); // then normalize the size of the planet to show close-up (number is the nominal scale)
                }
                else
                {
                    // planet.localScale = new Vector3(25 * scaleRatio, 25 * scaleRatio, 25 * scaleRatio); // then normalize the size of the planet to show close-up (number is the nominal scale)
                    planet.Rotate(0, 0, -90);
                }
                zoom = planetMinZoomLevel; // set planet view zoom level
                targetCameraXPosition = tgtPosition.x;
                targetCameraYPosition = tgtPosition.y;
                targetCameraZPosition = tgtPosition.z;
                
                cameraPlanetPosition = new Vector3(targetCameraXPosition, targetCameraYPosition, targetCameraZPosition);
                planetZoomComplete = true; // set views as true
                planetToSystemZoom = true;
            }
        }

        void DetermineZoomLevel()
        {
            ZoomLevel = uiManagerRef.ViewLevel;
        }
    }
}
