using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CameraScripts;
using System;
using UnityEngine.UI;
using Screens.Galaxy;
using StellarObjects;
using UnityEngine.Events;
using Managers;

public class SystemView : MonoBehaviour {

    private GalaxyCameraScript gScriptRef;
    private GraphicAssets graphicsDataRef;
    private GameData gameDataRef;
    private GalaxyData galaxyDataRef;
    private UIManager uiManagerRef;
    private Canvas canvasRef;
    private GalaxyView gScreenRef;
    public GameObject systemPlanetSummaryPanel;
    private List<GameObject> systemObjectsDrawnList = new List<GameObject>();
    private float alphaValue = 0f; // change back to 0 when fix the issue with corouting and vars taking too much CPU
    private bool DrawPanelSummary = false;
    private Text lowIntelLevelPlanetData;
    private Text noIntelLevelPlanetData;
    private Text noStellarObjectText;

    //events
    public class PointerClickEvent : UnityEvent<bool> { }; // empty class
    public static PointerClickEvent OnPointerEnter = new PointerClickEvent();

    // consts
    public const int HighIntelLevel = 6;
    public const int LowIntelLevel = 3;

    void Awake()
    {
        gScriptRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // tie the game camera script to the data
        canvasRef = GameObject.Find("System UI Canvas").GetComponent<Canvas>();
        galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        lowIntelLevelPlanetData = GameObject.Find("Low Intel Level Text").GetComponent<Text>();
        noIntelLevelPlanetData = GameObject.Find("No Intel Level Text").GetComponent<Text>();
        noStellarObjectText = GameObject.Find("No Stellar Object Text").GetComponent<Text>();
        lowIntelLevelPlanetData.enabled = false;
    }

    void OnGUI()
    {
                    
    }

    void Update()
    {
        //if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.System)
        //{
            
        //}

        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.System)
        {
            ShowSystemView();
            if (DrawPanelSummary)
                FadePlanetSummaryPanels();

            if (gameDataRef.RequestGraphicRefresh)
            {
                ResetDrawStates();
                gameDataRef.RequestGraphicRefresh = false;
            }
        }
        else
            ResetDrawStates();
    }

    void ShowSystemView()
    {
        GameObject selectedStar = gScreenRef.GetSelectedStar();
        StarData selectedStarData = selectedStar.GetComponent<Star>().starData;

        DrawSystemInformation();

        if (!DrawPanelSummary && (Math.Round(Camera.main.fieldOfView,2) < GalaxyCameraScript.systemMinZoomLevel + .05) && (Math.Round(Camera.main.fieldOfView,2) > GalaxyCameraScript.systemMinZoomLevel - .05))
        {

            if (selectedStarData.PlanetList.Count > 0)
            {
                // display planet summaries or warnings depending on system intel level
                if (selectedStarData.IntelLevel > eStellarIntelLevel.Medium || gameDataRef.DebugMode)
                {
                    DrawPlanetSummaryPanels();
                }
                else if (selectedStarData.IntelLevel == eStellarIntelLevel.Medium || selectedStarData.IntelLevel == eStellarIntelLevel.Medium)
                {
                    lowIntelLevelPlanetData.enabled = true;
                    StartCoroutine(FadeInAlpha());
                    lowIntelLevelPlanetData.color = new Color(lowIntelLevelPlanetData.color.r, lowIntelLevelPlanetData.color.g, lowIntelLevelPlanetData.color.b, alphaValue / 255f);
                }
                else
                {
                    noIntelLevelPlanetData.enabled = true;
                    StartCoroutine(FadeInAlpha());
                    noIntelLevelPlanetData.color = new Color(noIntelLevelPlanetData.color.r, noIntelLevelPlanetData.color.g, noIntelLevelPlanetData.color.b, alphaValue / 255f);
                }
            }
            // no planets in system, and intel is high enough to know that
            else if (selectedStarData.PlanetList.Count == 0 && (selectedStarData.IntelLevel > eStellarIntelLevel.None || gameDataRef.DebugMode))
            {
                noIntelLevelPlanetData.enabled = false;
                lowIntelLevelPlanetData.enabled = false;
                noStellarObjectText.enabled = true;
                StartCoroutine(FadeInAlpha());
                noStellarObjectText.color = new Color(noStellarObjectText.color.r, noStellarObjectText.color.g, noStellarObjectText.color.b, alphaValue / 255f);
            }
            else
            {
                noIntelLevelPlanetData.enabled = true;
                StartCoroutine(FadeInAlpha());
                noIntelLevelPlanetData.color = new Color(noIntelLevelPlanetData.color.r, noIntelLevelPlanetData.color.g, noIntelLevelPlanetData.color.b, alphaValue / 255f);
            }
        }
    }

    void DrawSystemInformation()
    {     
        GameObject selectedStar = gScreenRef.GetSelectedStar();
        StarData selectedStarData = selectedStar.GetComponent<Star>().starData;
    }

    void DrawPlanetSummaryPanels()
    {
        // test
        GameObject selectedStar = gScreenRef.GetSelectedStar();
        StarData selectedStarData = selectedStar.GetComponent<Star>().starData;
        int planetCount = 0;
        
            for (int x = 0; x < 5; x++)
            {
                if (selectedStarData.PlanetSpots[x] != null)
                {
                    Vector3 boxLocation;
                    PlanetData planetData = selectedStarData.PlanetSpots[x]; // ref for planet's data
                    //Vector3 nameVector = Camera.main.WorldToScreenPoint(gScreenRef.listSystemPlanetsCreated[planetCount].transform.position); // gets the screen point of the planet's transform position
                    Vector3 nameVector = Camera.main.WorldToScreenPoint(gScreenRef.listSystemPlanetsCreated.Find(p => p.name == selectedStarData.PlanetSpots[x].Name).transform.position); // gets the screen point of the planet's transform position
                    // set the planet data box position relative to the planet's world location
                    boxLocation = new Vector3(nameVector.x, nameVector.y - 120, 0); // where the text box is located
                    GameObject pPanel = Instantiate(systemPlanetSummaryPanel, boxLocation, Quaternion.identity) as GameObject; // draw the panel
                    pPanel.transform.SetParent(canvasRef.transform);
                    pPanel.transform.localScale = new Vector3(1f, 1f, 1f);
                    pPanel.transform.localPosition = new Vector3(boxLocation.x - (Screen.width / 2), boxLocation.y - (Screen.height / 2), -10);                  
                    pPanel.transform.Rotate(0, 0, 0); // rotate each panel on its x axis for 'curved into the screen' effect
                    pPanel.GetComponent<PlanetDataBox>().PopulateDataBox(planetData.ID); // populate the panel
               
                    pPanel.GetComponent<Image>().color = new Color(1, 1, 1, 0); // dim out the color
                    systemObjectsDrawnList.Add(pPanel);

                    // scale the panel vs. size
                    float widthScaleRatio = Screen.width / 1920f;
                    float heightScaleRatio = Screen.height / 1080f;
                    float usedRatio = 0f;
                    if (heightScaleRatio > widthScaleRatio)
                        usedRatio = widthScaleRatio;
                    else
                        usedRatio = heightScaleRatio;
                    //if (widthScaleRatio < .83f) // normalize the ratio to not be too thin
                    //    widthScaleRatio = .83f;
                    pPanel.transform.localScale = new Vector3(usedRatio, usedRatio, 1);

                    // increase the count
                    planetCount += 1;
                }
            }        
        DrawPanelSummary = true;
    }

    void FadePlanetSummaryPanels()
    {
        StartCoroutine(FadeInAlpha());
        Color baseColor = new Color();        
        Color fadeColor = new Color(1, 1, 1, alphaValue / 255f);

        for (int y = 0; y < systemObjectsDrawnList.Count; y++)
        {
           GameObject pPanel = systemObjectsDrawnList[y];
           systemObjectsDrawnList[y].GetComponent<Image>().color = fadeColor;
           baseColor = pPanel.transform.Find("Planet Name").GetComponent<Text>().color;
           pPanel.transform.Find("Planet Name").GetComponent<Text>().color = new Color(baseColor.r,baseColor.g,baseColor.b,fadeColor.a);
           pPanel.transform.Find("Planet Type").GetComponent<Text>().color = fadeColor;

           baseColor = pPanel.transform.Find("Planet Status").GetComponent<Text>().color;
           pPanel.transform.Find("Planet Status").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a);

           baseColor = pPanel.transform.Find("Planet Size").GetComponent<Text>().color; // get the original color
           pPanel.transform.Find("Planet Size").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // and then add the fade alpha

           baseColor = pPanel.transform.Find("Energy Level").GetComponent<Text>().color; // get the original color
           pPanel.transform.Find("Energy Level").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // assign the name to the text object child

           baseColor = pPanel.transform.Find("Bio Level").GetComponent<Text>().color; // get the original color
           pPanel.transform.Find("Bio Level").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // assign the name to the text object child

           baseColor = pPanel.transform.Find("Rare Materials Level").GetComponent<Text>().color; // get the original color
           pPanel.transform.Find("Rare Materials Level").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // assign the name to the text object child

           baseColor = pPanel.transform.Find("Alpha Materials Level").GetComponent<Text>().color; // get the original color
           pPanel.transform.Find("Alpha Materials Level").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // assign the name to the text object child

           baseColor = pPanel.transform.Find("Heavy Materials Level").GetComponent<Text>().color; // get the original color
           pPanel.transform.Find("Heavy Materials Level").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // assign the name to the text object child

           pPanel.transform.Find("Scan Level").GetComponent<Text>().color = fadeColor;

           baseColor = pPanel.transform.Find("Planet Trait 1").GetComponent<Text>().color;
           pPanel.transform.Find("Planet Trait 1").GetComponent<Text>().color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeColor.a); // assign the name to the text object child

           pPanel.transform.Find("Planet Trait 2").GetComponent<Text>().color = fadeColor;
        }
    }

    void ResetDrawStates()
    {
        
        lowIntelLevelPlanetData.enabled = false;
        noIntelLevelPlanetData.enabled = false;
        noStellarObjectText.enabled = false;
        DrawPanelSummary = false; // reset all 'draw flags'
        
        for (int x = 0; x < systemObjectsDrawnList.Count; x++)
        {
            Destroy(systemObjectsDrawnList[x]);         
        }
        systemObjectsDrawnList.Clear();
        alphaValue = 0f;
    }

    IEnumerator FadeInAlpha()
    {
        while (alphaValue < 255f)
        {
            alphaValue += .75f;
            yield return null;
        }
    }

    IEnumerator Wait1Second()
    {
        yield return new WaitForSeconds(1.0f);       
    }

}
