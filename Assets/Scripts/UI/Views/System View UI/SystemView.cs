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
    public GameObject EconomicPlanetSummaryPanel;
    public GameObject DemographicPlanetSummaryPanel;
    //public GameObject SystemChainOfCommand;
    //public GameObject houseShieldsPanel;
    public GameObject SystemResourcesPanel;
    public GameObject UninhabitedPlanetPanel;
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
        //houseShieldsPanel = GameObject.Find("House Shields Panel");
        lowIntelLevelPlanetData = GameObject.Find("Low Intel Level Text").GetComponent<Text>();
        noIntelLevelPlanetData = GameObject.Find("No Intel Level Text").GetComponent<Text>();
        noStellarObjectText = GameObject.Find("No Stellar Object Text").GetComponent<Text>();
        lowIntelLevelPlanetData.enabled = false;
        SystemResourcesPanel.SetActive(false); // initially off
        //SystemChainOfCommand.SetActive(false);
    }

    void Update()
    {
       // DrawHouseShields();
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.System)
        {
            ShowSystemView();

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

        if (!DrawPanelSummary && gScriptRef.systemZoomComplete && Mathf.Approximately(gScriptRef.TargetCameraPosition.x, Camera.main.transform.position.x) && Mathf.Approximately(gScriptRef.TargetCameraPosition.y, Camera.main.transform.position.y)
            && Mathf.Approximately(gScriptRef.TargetCameraPosition.z, Camera.main.transform.position.z))
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

    //void DrawHouseShields()
    //{
    //    if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.System)
    //    {
    //        GameObject selectedStar = gScreenRef.GetSelectedStar();
    //        StarData selectedStarData = selectedStar.GetComponent<Star>().starData;

    //        houseShieldsPanel.SetActive(true);
    //        Image[] listOfShields = new Image[5];

    //        listOfShields[0] = houseShieldsPanel.transform.Find("Crest Planet 1").GetComponent<Image>();
    //        listOfShields[1] = houseShieldsPanel.transform.Find("Crest Planet 2").GetComponent<Image>();
    //        listOfShields[2] = houseShieldsPanel.transform.Find("Crest Planet 3").GetComponent<Image>();
    //        listOfShields[3] = houseShieldsPanel.transform.Find("Crest Planet 4").GetComponent<Image>();
    //        listOfShields[4] = houseShieldsPanel.transform.Find("Crest Planet 5").GetComponent<Image>();

    //        for (int x = 0; x < 5; x++)
    //        {
    //            if (selectedStarData.PlanetSpots[x] != null)
    //            {

    //                if (selectedStarData.PlanetSpots[x].IsInhabited)
    //                {
    //                    listOfShields[x].sprite = graphicsDataRef.HouseCrestList.Find(p => p.name == "CREST000"); // need to add code to read the Holding info of the planet
    //                    listOfShields[x].color = new Color(1f, 1f, 1f, 1f); // faded out, but still there to force the correct spacing
    //                }
    //                else
    //                {
    //                    listOfShields[x].sprite = graphicsDataRef.HouseCrestList.Find(p => p.name == "CREST000");
    //                    listOfShields[x].color = new Color(1f, 1f, 1f, 0f); // faded out, but still there to force the correct spacing
    //                }
    //            }
    //            else
    //            {
    //                listOfShields[x].sprite = graphicsDataRef.HouseCrestList.Find(p => p.name == "CREST000");
    //                listOfShields[x].color = new Color(1f, 1f, 1f, 0f); // faded out, but still there to force the correct spacing
    //            }
    //        }
    //    }
    //    else
    //    {
    //        houseShieldsPanel.SetActive(false);
    //    }
    //}

    void DrawSystemInformation()
    {     
        GameObject selectedStar = gScreenRef.GetSelectedStar();
        StarData selectedStarData = selectedStar.GetComponent<Star>().starData;
        SystemResourcesPanel.SetActive(true);
        //SystemChainOfCommand.SetActive(true);
    }

    void DrawPlanetSummaryPanels()
    {
        // test
        GameObject selectedStar = gScreenRef.GetSelectedStar();
        GameObject selectedPanelType = new GameObject();
        StarData selectedStarData = selectedStar.GetComponent<Star>().starData;
        
        switch (uiManagerRef.PrimaryViewMode) // determine which panel to draw
        {
            case ViewManager.ePrimaryView.Economic:
                selectedPanelType = EconomicPlanetSummaryPanel;
                break;
            case ViewManager.ePrimaryView.Political:
                selectedPanelType = DemographicPlanetSummaryPanel;
                break;
            case ViewManager.ePrimaryView.Military:
                selectedPanelType = DemographicPlanetSummaryPanel;
                break;
            case ViewManager.ePrimaryView.Demographic:
                selectedPanelType = DemographicPlanetSummaryPanel;
                break;
            default:
                break;
        }

        int planetCount = 0;
        
            for (int x = 0; x < 5; x++)
            {
                if (selectedStarData.PlanetSpots[x] != null)
                {
                    Vector3 boxLocation;
                    PlanetData planetData = selectedStarData.PlanetSpots[x]; // ref for planet's data
                    GameObject pPanel;
                    Vector3 nameVector = Camera.main.WorldToScreenPoint(gScreenRef.listSystemPlanetsCreated.Find(p => p.name == selectedStarData.PlanetSpots[x].Name).transform.position); // gets the screen point of the planet's transform position                                                                                                                                                                                        // set the planet data box position relative to the planet's world location
                    boxLocation = new Vector3(nameVector.x, nameVector.y - 125, 0); // where the text box is located
                    if (planetData.IsInhabited)
                    {
                        pPanel = Instantiate(selectedPanelType, boxLocation, Quaternion.identity) as GameObject; // draw the panel
                        switch (uiManagerRef.PrimaryViewMode) // determine which panel to draw
                        {
                            case ViewManager.ePrimaryView.Economic:
                                pPanel.GetComponent<PlanetEconomicDataBox>().PopulateDataBox(planetData.ID); // populate the panel
                                break;
                            case ViewManager.ePrimaryView.Political:
                                //pPanel.GetComponent<PlanetPoliticalDataBox>().PopulateDataBox(planetData.ID); // populate the panel
                                break;
                            case ViewManager.ePrimaryView.Military:
                                //pPanel.GetComponent<PlanetEconomicDataBox>().PopulateDataBox(planetData.ID); // populate the panel
                                break;
                            case ViewManager.ePrimaryView.Demographic:
                                //pPanel.GetComponent<PlanetDemographicDataBox>().PopulateDataBox(planetData.ID); // populate the panel
                                break;
                            default:
                                break;
                        }                   
                    }
                    else
                    {
                        pPanel = Instantiate(UninhabitedPlanetPanel, boxLocation, Quaternion.identity) as GameObject; // draw the panel
                        pPanel.GetComponent<PlanetUnownedDataBox>().PopulateDataBox(planetData.ID); // populate the panel
                    }

                    pPanel.transform.SetParent(canvasRef.transform);
                    pPanel.transform.localScale = new Vector3(.9f, .9f, .9f);
                    pPanel.transform.localPosition = new Vector3(boxLocation.x - (Screen.width / 2), boxLocation.y - (Screen.height / 2));
                                     
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
                    pPanel.transform.localScale = new Vector3(.9f * usedRatio, .9f * usedRatio, 1);

                    // increase the count                  
                    planetCount += 1;
                }
            }
                    
        DrawPanelSummary = true;
    }

    void ResetDrawStates()
    {
        
        lowIntelLevelPlanetData.enabled = false;
        noIntelLevelPlanetData.enabled = false;
        noStellarObjectText.enabled = false;
        SystemResourcesPanel.SetActive(false);
        //SystemChainOfCommand.SetActive(false);
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
            alphaValue += 5f;
            yield return null;
        }
    }

    IEnumerator Wait1Second()
    {
        yield return new WaitForSeconds(1.0f);       
    }

}
