using UnityEngine;
using UnityEngine.UI;
using GameEvents;
using CameraScripts;
using StellarObjects;
using Managers;
using TMPro;


public class EventButton : MonoBehaviour
{
    private string buttonID;
    //private string characterID;
    private string planetLocationID;
    private string systemLocationID;
    private GraphicAssets gAssets;
    public TextMeshProUGUI ButtonText;
    public Image ButtonImage;
    public EventScrollView ScrollView;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }

    public void SetAlertText(string alert)
    {
        ButtonText.text = alert;
    }

    public void SetAlertLevel(GameEvent.eEventLevel alertLevel)
    {
        Color orange = new Color(1f,.55f,0f);
        switch (alertLevel)
        {
            case GameEvent.eEventLevel.Informational:
                ButtonText.color = Color.white;
                break;

            case GameEvent.eEventLevel.Positive:
                ButtonText.color = Color.green;
                break;

            case GameEvent.eEventLevel.Moderate:
                ButtonText.color = Color.yellow;
                break;

            case GameEvent.eEventLevel.Serious:
                ButtonText.color = orange;
                break;

            case GameEvent.eEventLevel.Critical:
                ButtonText.color = Color.red;
                break;

            default:
                ButtonText.color = Color.white;
                break;
        }
    }

    public void SetID(string ID)
    {
        buttonID = ID;
    }

    public void SetPicture(GameEvent.eEventType picID)
    {
        ButtonImage.sprite = gAssets.EventPicList.Find(p => p.name.ToLower() == picID.ToString().ToLower());
    }

    public void SetLocation (string sLocation, string pLocation)
    {
        planetLocationID = pLocation;
        systemLocationID = sLocation;
    }

    public void Button_Click()
    {
        GalaxyData galData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();

        //invoke zoom sequence
        if (systemLocationID != null)
        {
            Transform star = galData.GalaxyStarList.Find(p => p.name == HelperFunctions.DataRetrivalFunctions.GetSystem(systemLocationID).Name).transform;
            if (star != null)
            {
                star.GetComponent<Star>().tag = "Selected"; // to allow system screen generation
                StartSystemZoom(star);
                //Camera.main.GetComponent<GalaxyCameraScript>().starTarget = star;
                //star.GetComponent<Star>().tag = "Selected";
                //Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = true;
                //Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
                //Camera.main.GetComponent<GalaxyCameraScript>().ScrollWheelIsValid = true; // allow scrolling once again
                //gData.StarSelected = true;
            }
        
        }
    }

    private void StartSystemZoom(Transform target) // when button is pressed, zooms to system (needs some work!)
    {
        GalaxyData galData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        UIManager uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();

        Camera.main.GetComponent<GalaxyCameraScript>().starTarget = target;
        Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = true;
        Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
        Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = false;
        gameDataRef.StarSelected = true; // probably need to move to a global UI manager
        uiManagerRef.selectedSystem = target.GetComponent<Star>().starData;
        uiManagerRef.SetActiveViewLevel(ViewManager.eViewLevel.System);
    }
}
