using UnityEngine;
using System.Collections.Generic;
using StellarObjects;
using CivObjects;
using Managers;
using CameraScripts;
using HelperFunctions;
using UnityEngine.UI;
using TMPro;
using Constants;

public class SovereignityGalaxyView : MonoBehaviour {

    public List<GameObject> listAstrographicViewObjectsCreated = new List<GameObject>(); // list of all UI elements created (lines, range circles, etc)
    private List<GameObject> listAstrographicTextObjectsCreated = new List<GameObject>(); // list of text labels (so can move dynamically onGUI)
    public GameObject sovDataBlock; // the data block used in this view
    private GameData gameDataRef; // game data reference
    private GalaxyData galDataRef;  // stellar data reference
    private GalaxyCameraScript gCameraRef; // main camera reference
    private UIManager uiManagerRef; // UI Manager reference
    private bool rangeCirclesGenerated;
    private Canvas galaxyPlanetInfoCanvas; // where the trade info is drawn (not the 3D objects)
    private GraphicAssets graphicAssets;
    private bool sovInfoDrawn = false;

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
        galDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // get global game data (date, location, version, etc)
        gCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // get global camera script
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        galaxyPlanetInfoCanvas = GameObject.Find("Galaxy Planet Info Canvas").GetComponent<Canvas>();
        graphicAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }
                                                                               
    // Update is called once per frame
    void Update()
    {
        // check whether the mode is active and if so, show what needs to be shown
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
        {
            UpdateSovereignityView();
            UpdateSovDataBlocks();
        }
        else
            ClearView(); // destroy all objects so that they can be rebuilt on new view                  
    }

    void UpdateSovereignityView()
    {
        if (!rangeCirclesGenerated || gameDataRef.RequestGraphicRefresh)
        {
            GenerateCivilizationRangeCircles();         
        }

        if (!sovInfoDrawn || gameDataRef.RequestGraphicRefresh)
        {
            GenerateSovDataBlocks();
        }

        ShowCivilizationRangeCircles();

        if (uiManagerRef.SecondaryViewMode == ViewManager.eSecondaryView.Sovereignity)
            ShowSovDataBlocks();
        else
        {
            DimCivilizationRangeCircles();
            ShowSovDataBlocks();
        }
                   
        if (uiManagerRef.PrimaryViewMode != ViewManager.ePrimaryView.Political) // show as an underlay for trade
        {
            ShowCivilizationRangeCircles();
            DimCivilizationRangeCircles();             
        }
        
        if (uiManagerRef.PrimaryViewMode == ViewManager.ePrimaryView.Economic)
            HideSovDataBlocks(); // they get in the way of the trade blocks
    }

    void ClearView()
    {
        if (listAstrographicViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listAstrographicViewObjectsCreated)
                Destroy(rCircle); // remove the circles
        }

        if (listAstrographicTextObjectsCreated.Count > 0)
        {
            foreach (GameObject rText in listAstrographicTextObjectsCreated)
                Destroy(rText); // remove the circles
        }

        listAstrographicViewObjectsCreated.Clear();
        listAstrographicTextObjectsCreated.Clear();
        rangeCirclesGenerated = false;
        sovInfoDrawn = false;
    }

    void GenerateCivilizationRangeCircles()
    {
        ClearView(); // clear out all existing objects in view

        foreach (Civilization civ in gameDataRef.CivList)
        {
            if (DataRetrivalFunctions.GetCivHomeSystem(civ).IntelValue > 6 || gameDataRef.DebugMode)
            {
                // first get each system that has a holding of a Empire
                foreach (StarData sData in civ.SystemList)
                {
                    Vector3 cirLocation = new Vector3(sData.WorldLocation.x, sData.WorldLocation.y, -10); // pull the star location of the capital system and put at a certain depth
                    float civRange = 0;

                    // determine scale of circle
                    civRange = sData.CivPopulation(civ) * 6; // using the population value for now
                    
                    // generate the range circle
                    GameObject rangeCircle = Instantiate(Resources.Load("Galaxy View Assets/Empire Range Circle", typeof(GameObject)), cirLocation, Quaternion.Euler(new Vector3(90, 0, 0))) as GameObject;
                    rangeCircle.transform.localScale = new Vector3(civRange, 1, civRange); // expand the circle
                    listAstrographicViewObjectsCreated.Add(rangeCircle);
                    rangeCircle.GetComponent<MeshRenderer>().material.color = new Color(civ.Color.r, civ.Color.g, civ.Color.b, .25f); // set range circle to show civ color
                }
            }
        }

        rangeCirclesGenerated = true;
    }

    void ShowCivilizationRangeCircles()
    {
        if (listAstrographicViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listAstrographicViewObjectsCreated)
            {
                rCircle.SetActive(true);
                Color civColor = rCircle.GetComponent<MeshRenderer>().material.color;
                rCircle.GetComponent<MeshRenderer>().material.color = new Color(civColor.r, civColor.g, civColor.b, .25f); // restore color                
            }
        }
    }

    void HideCivilizationRangeCircles()
    {
        if (listAstrographicViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listAstrographicViewObjectsCreated)
            {
                rCircle.SetActive(false);
            }
        }
    }

    void DimCivilizationRangeCircles()
    {
        if (listAstrographicViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listAstrographicViewObjectsCreated)
            {
                Color civColor = rCircle.GetComponent<MeshRenderer>().material.color;
                rCircle.GetComponent<MeshRenderer>().material.color = new Color(civColor.r, civColor.g, civColor.b, .08f); // set range circle to show civ color
            }
        }
    }

    void HideSovDataBlocks()
    {
        foreach (GameObject star in listAstrographicTextObjectsCreated)
        {
            star.SetActive(false);
        }
    }

    void ShowSovDataBlocks()
    {
        foreach (GameObject star in listAstrographicTextObjectsCreated)
        {
            star.SetActive(true);
        }
    }

    void UpdateSovDataBlocks()
    {
        Vector3 textLocation;
        TradeViewSystemData sysData;

        foreach (GameObject tradeBlock in listAstrographicTextObjectsCreated)
        {
            if (tradeBlock.activeSelf)
            {
                Color textColor = new Color();
                sysData = tradeBlock.GetComponent<TradeViewSystemData>();
                StarData starData = sysData.starData;

                if (!gameDataRef.DebugMode)
                {
                    tradeBlock.GetComponentInChildren<TextMeshProUGUI>().text = starData.Name.ToUpper();
                    if (starData.IntelValue > Constant.MediumIntelLevelMax)
                        textColor = sysData.ownerColor;
                    else
                        textColor = Color.grey;
                }
                else
                {
                    if (starData.OwningCiv != null)
                    {
                        tradeBlock.GetComponentInChildren<TextMeshProUGUI>().text = starData.Name.ToUpper() + "(" + starData.WorldLocation.x.ToString("N0") + "," + starData.WorldLocation.y.ToString("N0") + ")" + starData.OwningCiv.Name + starData.OwningCiv.PlanetMinTolerance;// stellarObject.civNames;
                        textColor = sysData.ownerColor;
                    }
                }

                tradeBlock.GetComponentInChildren<TextMeshProUGUI>().color = textColor; // change text to color of the owning civ

                // reset location of data line blocks
                Vector3 nameVector;

                nameVector = Camera.main.WorldToScreenPoint(sysData.starTransform.position);
                textLocation = nameVector;
                tradeBlock.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)

            }
        }
    }

    void GenerateSovDataBlocks()
    {
        Vector3 textLocation;
        Vector3 textLocation2;

        foreach (GameObject star in galDataRef.GalaxyStarList)
        {
            if (star != null)
            {
                if (star.GetComponent<Star>().tag != "Companion Star") // if not a companion star
                {
                    string civNames = "";
                    bool civOwnerFound = false;
                    Color civColor = Color.grey;

                    var nameVector = Camera.main.WorldToScreenPoint(star.transform.position); // gets the screen point of the star's transform position
                    textLocation = new Vector3(nameVector.x, nameVector.y, 0); // where the text box is located
                    textLocation2 = new Vector3(nameVector.x, nameVector.y, 0); // where the lower text box is located

                    // create the text objects
                    GameObject sovInfoBlock = Instantiate(sovDataBlock, textLocation, Quaternion.identity) as GameObject;
                    sovInfoBlock.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the blocks to the panel
                    sovInfoBlock.GetComponent<TradeViewSystemData>().starData = star.GetComponent<Star>().starData; // attach the star data to the object
                    sovInfoBlock.GetComponent<TradeViewSystemData>().starDataName = sovInfoBlock.GetComponent<TradeViewSystemData>().starData.Name; // attach the star data to the object
                    sovInfoBlock.transform.Find("Province Name").GetComponent<TextMeshProUGUI>().text = "";

                    // set the base stats                  
                    sovInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().enabled = true;
                    sovInfoBlock.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // scale systems with low intel down
                    //sovInfoBlock.transform.Rotate(GalaxyCameraScript.cameraTilt, 0, 0); // test

                    // add the owning civs if present      
                    string civNumber = "-1"; // for the icons

                    foreach (Civilization civ in gameDataRef.CivList)
                    {
                        List<StarData> civSystems = new List<StarData>(); // assign to a temp list each system in a civ
                        civSystems = DataRetrivalFunctions.GetCivSystemList(civ);

                        if (!civOwnerFound)
                        {
                            foreach (StarData sys in civSystems) // check to see if this star belongs to any civ that is currently running the loop
                            {
                                if (sys.ID == star.GetComponent<Star>().starData.ID) // if IDs match, assign a trade block
                                {
                                    civColor = civ.Color;
                                    civNumber = civ.ID.Substring(3); // get the number from the ID of the civilization                                    
                                    sovInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().sprite = graphicAssets.EmpireCrestList.Find(p => p.name == "CREST" + civNumber); // attach the crest
                                    if (sys.Province != null)   
                                        sovInfoBlock.transform.Find("Province Name").GetComponent<TextMeshProUGUI>().text = sys.Province.Name.ToUpper() + " PROVINCE";

                                    if (sys.IntelLevel <= eStellarIntelLevel.Medium)
                                    {
                                        sovInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().enabled = false;
                                        sovInfoBlock.transform.localScale = new Vector3(.85f, .85f, 1); // do not scale
                                        sovInfoBlock.transform.Find("Province Name").GetComponent<TextMeshProUGUI>().text = "";
                                    }

                                    civOwnerFound = true;
                                    break;
                                }
                            }
                        }
                        else
                            break;
                    }

                    if (!civOwnerFound) // if there was not a civ found, don't attach any trade information, just show the basic callout
                    {
                        sovInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().enabled = false; // don't show icons of stars that don't have an owner!
                        sovInfoBlock.transform.localScale = new Vector3(.85f, .85f, 1); // do not scale
                    }

                    // assign the color of the civ, set the local position and name of the block
                    sovInfoBlock.GetComponentInChildren<TextMeshProUGUI>().color = civColor;
                    sovInfoBlock.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)
                    sovInfoBlock.name = star.GetComponent<Star>().starData.ID;

                    // assign to the star data block                  
                    sovInfoBlock.GetComponent<TradeViewSystemData>().ownerName = civNames;
                    sovInfoBlock.GetComponent<TradeViewSystemData>().starTransform = star.transform;
                    sovInfoBlock.GetComponent<TradeViewSystemData>().ownerColor = civColor;

                    //starDataBlock.textObject = tradeInfoBlock;
                    listAstrographicTextObjectsCreated.Add(sovInfoBlock);
                }
            }
        }

        sovInfoDrawn = true;
    }
}
