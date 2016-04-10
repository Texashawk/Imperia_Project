using UnityEngine;
using System.Collections;
using StellarObjects;
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
        [HideInInspector]public bool ScrollWheelIsValid { get; set; } // can the scrollwheel be used to manipulate the camera?
	    private float zoomSpeed = 11f;
        private float zoomSensitivity = 60f;
        public const float cameraTilt = 30f; // was 20f
        private Camera mainC; // camera reference
        private GameData gDataRef;
        private UIManager uiManagerRef;

        [HideInInspector]public ViewManager.eViewLevel ZoomLevel;
	    private const int scrollSpeedVariable = 2000;
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
        public const int galaxyZValue = 8000; // how far away the camera is from the galaxy map on a Z axis
        private const int systemZValue = 2000; // how far the camera is away from the system planet views

        public const int galaxyMinZoomLevel = 30;
        public const int systemMinZoomLevel = 12;
        public const int planetMinZoomLevel = 2;
        public const int maxZoomLevel = 50;
        public const int minZoomLevel = 15;

        public float zoom;
       
	    // Use this for initialization
	    void Start () {
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
            }
	
	    // Update is called once per frame
        void Update()
        {
            scaleRatio = (Screen.height / 1920f) * (Screen.width / 1080f);
       
            if (gDataRef.uiSubMode == GameData.eSubMode.None && !gDataRef.modalIsActive)  // only work in no submode
            {
                // run movement/zoom functions
                CheckForMapPan();
                StartCoroutine(PanMap());
                CheckForMapZoom();
                DetermineZoomLevel(); // update zoom level of camera

                // check for RMB pan of map
                //if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy && Input.GetMouseButton(1) && !systemZoomActive)
                //{
                //    Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                //    Vector3 newCameraPosition = mainC.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y - 200f, transform.position.z + 4000)); // adjust y pos 200f with normal z position
                //    transform.position = new Vector3(newCameraPosition.x, newCameraPosition.y + tiltYOffset, transform.position.z);
                //}

                if (provinceZoomActive && !systemZoomActive && provinceTarget != null)
                {
                    ZoomToProvince(provinceTarget);
                }

                if (systemZoomActive && !planetZoomActive && starTarget != null)
                {
                    //transform.position = new Vector2(starTarget.position.x, starTarget.position.y);
                    ZoomToSystem(starTarget);
                }

                //if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)  // normalize camera height in galaxy mode
                //{
                //    transform.position = new Vector3(transform.position.x, transform.position.y, galaxyZValue); // now set the height of the camera in a separate step
                //    systemZoomActive = false;
                //    planetZoomActive = false;
                //}

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
                    }
            
                if (systemZoomActive)
                    if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                    {
                        //transform.position = new Vector3(starTarget.transform.position.x, starTarget.transform.position.y + tiltYOffset, transform.position.z); // first set the x/y position of the camera   
                        transform.position = new Vector3(transform.position.x - 38 - (Screen.width / 10), transform.position.y + tiltYOffset, transform.position.z); // first set the x/y position of the camera
                        systemZoomActive = false;
                        provinceZoomActive = false;
                        provinceZoomComplete = false;
                        systemZoomComplete = true; // was false
                        zoom = maxZoomLevel;                                            
                        transform.position = new Vector3(transform.position.x, transform.position.y, galaxyZValue); // now set the height of the camera in a separate step
                        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.Galaxy); // resets view level to the galaxy
                        //starTarget = null;  
                    }

                if (planetZoomActive)
                    if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                    {
                        planetTarget.transform.localScale = new Vector3(planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize,planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize,1); // reset the scale
                        planetTarget.Rotate(0, 0, 90);
                        planetTarget = null;
                        planetZoomActive = false;
                        planetZoomComplete = false;
                        provinceZoomActive = false;
                        provinceZoomComplete = false;
                        systemZoomActive = true;
                        systemZoomComplete = false;
                        zoom = systemMinZoomLevel;
                        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.System);
                    }

                // check each cycle for camera height lock
                if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, galaxyZValue);
                    systemZoomComplete = false;
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
                transform.eulerAngles = new Vector3(180, 0, 0); // was 30,0,0
            else
            {
                transform.eulerAngles = new Vector3(180 - cameraTilt, 0, 0); // was 30,0,0
            }
        }

        void CheckForMapPan()
        {
            if (ZoomLevel == ViewManager.eViewLevel.Galaxy) // don't pan the map if in system or planet view or if in province mode
            {
                // check for map movement
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    moveMapUp = true;
                else
                    moveMapUp = false;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    moveMapLeft = true;
                else
                    moveMapLeft = false;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    moveMapRight = true;
                else
                    moveMapRight = false;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    moveMapDown = true;
                else
                    moveMapDown = false;
            }
        }

        IEnumerator PanMap()
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
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Lerp(yLocation, moveUpVector, Time.deltaTime * 3), transform.position.z);
                }

                if (moveMapDown && ((yLocation <= galaxyHeight + tiltYOffset) && (yLocation >= -galaxyHeight + tiltYOffset)))
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Lerp(yLocation, moveDownVector, Time.deltaTime * 3), transform.position.z);
                }
                
                if (moveMapRight && ((xLocation <= galaxyWidth) && (xLocation >= -galaxyWidth)))
                {
                    transform.position = new Vector3(Mathf.Lerp(xLocation, moveRightVector, Time.deltaTime * 3), transform.position.y, transform.position.z);
                }

                if (moveMapLeft && ((xLocation <= galaxyWidth) && (xLocation >= -galaxyWidth)))
                {
                    transform.position = new Vector3(Mathf.Lerp(xLocation, moveLeftVector, Time.deltaTime * 3), transform.position.y, transform.position.z);
                }

                // normalize to prevent scrolling outside of map
                if (transform.position.x > galaxyWidth)
                    transform.position = new Vector3(galaxyWidth, transform.position.y, transform.position.z);
                if (transform.position.x < -galaxyWidth)
                    transform.position = new Vector3(-galaxyWidth, transform.position.y, transform.position.z);

                if (transform.position.y > galaxyHeight + tiltYOffset)
                    transform.position = new Vector3(transform.position.x, galaxyHeight + tiltYOffset, transform.position.z);
                if (transform.position.y < -galaxyHeight + tiltYOffset)
                    transform.position = new Vector3(transform.position.x, -galaxyHeight + tiltYOffset, transform.position.z);

                yield return null;
            }
        }

        void LateUpdate()
        {
            mouseWheelValue = Input.GetAxis("Mouse ScrollWheel");
            mainC.fieldOfView = Mathf.Lerp(mainC.fieldOfView, zoom, Time.deltaTime * zoomSpeed);

           
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
                StartCoroutine(ProvinceZoom(tgtPosition));
            }

            provinceZoomComplete = true;

        }

	    void ZoomToSystem(Transform target)
	    {
            
            Vector3 tgtPosition;
            
            int leftCameraMove = 0;

            if (Screen.width >= 1600)
                leftCameraMove = 420;
            else if (Screen.width >= 1280)
                leftCameraMove = 460;
            else
                leftCameraMove = 490;

            if (!systemZoomComplete)
            {
                if (!planetToSystemZoom)
                {
                    tgtPosition = new Vector3(target.position.x + (leftCameraMove * (Screen.width / 1920f)) + (target.localScale.x / 2), target.position.y + 75 * (Screen.height / 1080f), systemZValue);
                    zoom = systemMinZoomLevel; // set system view zoom level
                    StartCoroutine(SystemZoom(tgtPosition));
                }            
                else
                {
                    tgtPosition = new Vector3(target.position.x + (leftCameraMove * (Screen.width / 1920f)) + (target.localScale.x / 2), target.position.y + 75 * (Screen.height / 1080f), systemZValue);
                    transform.position = new Vector3 (cameraPlanetPosition.x,cameraPlanetPosition.y, systemZValue);
                    
                    zoom = systemMinZoomLevel; // set system view zoom level
                    StartCoroutine(SystemZoom(tgtPosition));
                }

                target = null; // reset the target  
                systemZoomComplete = true;
                planetToSystemZoom = false;       
            }   
	    }

        void ZoomToPlanet(Transform planet) // need to tweak where the planet shows up on the screen (aiming for lower-left)
        {       
            if (!planetZoomComplete)
            {                
                Vector3 tgtPosition = new Vector3(planet.position.x, planet.position.y - (55f * scaleRatio), systemZValue);
                planet.localScale = new Vector3(25 * scaleRatio, 25 * scaleRatio, 1); // then normalize the size of the planet to show close-up (number is the nominal scale)
                zoom = planetMinZoomLevel; // set planet view zoom level
                StartCoroutine(PlanetZoom(tgtPosition));
                planet.Rotate(0, 0, -90);
                cameraPlanetPosition = transform.position;
                planetZoomComplete = true; // set views as true
                planetToSystemZoom = true;
            }
        }

        IEnumerator PlanetZoom(Vector3 tgtPosition)
        {
            while ((((int)transform.position.x != (int)tgtPosition.x | (int)transform.position.y != (int)tgtPosition.y) | (int)transform.position.z != (int)tgtPosition.z) && (planetZoomActive))
            {
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, tgtPosition.x, Time.deltaTime * (zoomSpeed + 5)), Mathf.Lerp(transform.position.y, tgtPosition.y, Time.deltaTime * (zoomSpeed + 5)), tgtPosition.z); // interpolate the movement
                cameraPlanetPosition = transform.position;
                planetToSystemZoom = true;
                yield return null;          
            }
        }
 
        IEnumerator SystemZoom(Vector3 tgtPosition)
        {
        
            while((((int)transform.position.x != (int)tgtPosition.x | (int)transform.position.y != (int)tgtPosition.y) | (int)transform.position.z != (int)tgtPosition.z) && (!planetZoomActive) && (!provinceZoomActive) && (systemZoomActive))
            {      
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, tgtPosition.x, Time.deltaTime * (zoomSpeed + 3)), Mathf.Lerp(transform.position.y, tgtPosition.y, Time.deltaTime * (zoomSpeed + 3)), tgtPosition.z); // interpolate the movement
                yield return null;        
            }

            cameraSystemPosition = transform.position; // assign the camera system position
        
        }

        IEnumerator ProvinceZoom(Vector3 tgtPosition)
        {

            while (((int)transform.position.x != (int)tgtPosition.x | (int)transform.position.y != (int)tgtPosition.y) && (!systemZoomActive) && (!planetZoomActive) && (provinceZoomActive))
            {
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, tgtPosition.x, Time.deltaTime * (zoomSpeed + 2)), Mathf.Lerp(transform.position.y, tgtPosition.y, Time.deltaTime * (zoomSpeed + 2)), tgtPosition.z); // interpolate the movement
                yield return null;
            }

            //cameraSystemPosition = transform.position; // assign the camera system position

        }

        void DetermineZoomLevel()
        {
            ZoomLevel = uiManagerRef.ViewLevel;
        }
    }
}
