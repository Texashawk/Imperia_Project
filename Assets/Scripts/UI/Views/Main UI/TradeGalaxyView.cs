using UnityEngine;
using System.Collections.Generic;
using StellarObjects;
using CivObjects;
using Managers;
using CameraScripts;
using HelperFunctions;
using EconomicObjects;
using Constants;
using TMPro;
using UnityEngine.UI;

public class TradeGalaxyView : MonoBehaviour
{
    public GameObject tradeModeDataObject; // the trade mode prefab that is used in this mode
    public List<GameObject> listTradeViewObjectsCreated = new List<GameObject>(); // list of all UI elements created (lines, range circles, etc)
    private List<GameObject> listTradeTextBlocksCreated = new List<GameObject>(); // list of text labels (so can move dynamically onGUI)
    //public List<GameObject> listActiveTradeFleets = new List<GameObject>(); // list of trade fleet objects that are moving
    private GameData gameDataRef; // game data reference
    public bool RefreshTradeView = false;
    private GalaxyData galaxyDataRef;  // stellar data reference
    private GalaxyCameraScript gCameraRef; // main camera reference
    private UIManager uiManagerRef; // UI Manager reference
    private TradeManager tManagerRef; // Trade Manager reference
    public GameObject TradeHubCircle; // the trade hub circle object to use
    private bool tradeHubsGenerated;
    private bool tradeInfoDrawn = false;
    private Canvas galaxyPlanetInfoCanvas; // where the trade info is drawn (not the 3D objects)
    private GraphicAssets graphicAssets;

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
        galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // get global game data (date, location, version, etc)
        gCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // get global camera script
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        galaxyPlanetInfoCanvas = GameObject.Find("Galaxy Planet Info Canvas").GetComponent<Canvas>();
        graphicAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        tManagerRef = GameObject.Find("GameManager").GetComponent<TradeManager>();
    }

    // Update is called once per frame
    void Update()
    {

        // check whether the mode is active and if so, show what needs to be shown
        if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
        {
            UpdateTradeView();
            UpdateTradeDataBlocks();
        }
        else
        {
            if (tradeHubsGenerated)
                HideTradeHubRanges();
            if (tradeInfoDrawn)
                HideTradeDataBlocks();
            SetTradeFleetVisibility(false);
        }
            //ClearView(); // destroy all objects so that they can be rebuilt on new view

        if (uiManagerRef.RequestTradeViewGraphicRefresh)
            uiManagerRef.RequestTradeViewGraphicRefresh = false; // reset the refresh need                  
    }

    void UpdateTradeView()
    {
        if (!tradeHubsGenerated || uiManagerRef.RequestTradeViewGraphicRefresh) // if circles are not yet generated, create them
            GenerateTradeRadiusCircles();

        if (!tradeInfoDrawn || uiManagerRef.RequestTradeViewGraphicRefresh) // if data blocks are not yet generated, create them
            GenerateTradeDataBlocks();

        // show if in economic mode, trade view, otherwise hide
        if (uiManagerRef.PrimaryViewMode == ViewManager.ePrimaryView.Economic && uiManagerRef.SecondaryViewMode == ViewManager.eSecondaryView.Trade)
        {
            ShowTradeHubRanges();
            ShowTradeDataBlocks();
            SetTradeFleetVisibility(true);
        }
        else
        {
            HideTradeHubRanges();
            HideTradeDataBlocks();
            SetTradeFleetVisibility(false);
        }
    }

    void SetTradeFleetVisibility(bool isVisible)
    {
        if (gameDataRef.ActiveTradeFleetObjects.Count > 0)
        {
            foreach (GameObject rFleet in gameDataRef.ActiveTradeFleetObjects)
            {
                if (rFleet != null)
                {
                    if (!isVisible)
                        rFleet.SetActive(false);
                    else
                        rFleet.SetActive(true);
                }
            }
        }
    }

    void ClearView()
    {
        DestroyTradeDataBlocks();
        if (listTradeViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listTradeViewObjectsCreated)
                Destroy(rCircle); // remove the circles
        }

        listTradeViewObjectsCreated.Clear();
        tradeHubsGenerated = false;   
    }

    void DestroyTradeDataBlocks()
    {
        if (listTradeTextBlocksCreated.Count > 0)
        {
            foreach (GameObject tBlock in listTradeTextBlocksCreated)
            {
                Destroy(tBlock); // remove the text blocks
            }
            listTradeTextBlocksCreated.Clear();
            tradeInfoDrawn = false;
        }          
    }

    void GenerateTradeRadiusCircles()
    {
        ClearView(); // clear out all existing objects in view

        foreach (Civilization civ in gameDataRef.CivList)
        {
            if (DataRetrivalFunctions.GetCivHomeSystem(civ).IntelValue > Constant.MediumIntelLevelMax || gameDataRef.DebugMode)
            {
                // first get each system that has a holding of a Empire
                foreach (StarData sData in civ.SystemList)
                {
                    if (sData.LargestTradeHub != PlanetData.eTradeHubType.NotHub)
                    {
                        Vector3 cirLocation = new Vector3(sData.WorldLocation.x, sData.WorldLocation.y, sData.WorldLocation.z + 20); // pull the coordinate location of the system
                        string hubSize = "";
                        string hubName = "";
                        string tradeGroupName = "NONE";
                        Color tradeGroupColor = Color.white;
                        float TotalMerchantSkill = 0;

                        foreach (PlanetData pData in sData.PlanetList)
                        {
                            TotalMerchantSkill += pData.TotalMerchants * pData.AverageMerchantSkill;
                        }

                        // determine scale of circle (trade range)
                        float hubRange = 0;
                        if (sData.LargestTradeHub == PlanetData.eTradeHubType.SecondaryTradeHub)
                        {
                            hubRange = sData.GetRangeOfHub * 2;
                            hubSize = "SECONDARY";
                            hubName = sData.Name.ToUpper() + " " + hubSize + " HUB"; // set the text of the circle
                        }
                        else if (sData.LargestTradeHub == PlanetData.eTradeHubType.ProvinceTradeHub)
                        {
                            hubRange = sData.GetRangeOfHub * 2;
                            hubSize = "PROVINCE";
                            hubName = sData.Province.Name.ToUpper() + " " + hubSize + " HUB"; // set the text of the circle
                        }
                        else
                        {
                            hubRange = sData.GetRangeOfHub * 2;
                            hubSize = "IMPERIAL";
                            hubName = sData.OwningCiv.Name.ToUpper() + " " + hubSize + " HUB"; // set the text of the circle
                        }

                        if (tManagerRef.ActiveTradeGroups.Exists(p => p.SystemIDList.Exists(x => x == sData.ID))) // if the system belongs to any of the trade groups
                        {
                            foreach (TradeGroup tradeGroup in tManagerRef.ActiveTradeGroups)
                            {
                                if (tradeGroup.SystemIDList.Contains(sData.ID))
                                {
                                    tradeGroupName = tradeGroup.Name;
                                    tradeGroupColor = tradeGroup.GroupColor;
                                    if (tradeGroup.ConnectedToCivHub && sData.LargestTradeHub == PlanetData.eTradeHubType.ProvinceTradeHub)
                                    {
                                        tradeGroupName += " (CIV CONNECTED)";
                                    }
                                }
                            }
                        }


                        //GameObject tradeHubCircle = Instantiate(Resources.Load("Galaxy View Assets/Trade Range Circle", typeof(GameObject)), cirLocation, Quaternion.Euler(new Vector3(90, 0, 0))) as GameObject;
                        //tradeHubCircle.transform.localScale = new Vector3(hubRange, 1, hubRange); // expand the circle
                        //TextMesh[] tradeHubComponents = tradeHubCircle.GetComponentsInChildren<TextMesh>(); // get each component
                        //tradeHubComponents[0].text = hubName; // get the first text component assigned
                        ////tradeHubComponents[1].text = tradeGroupName; // get the second text component assigned
                        ////tradeHubComponents[3].text = TotalMerchantSkill.ToString("N0"); 
                        ////tradeHubComponents[1].color = tradeGroupColor;
                        //listTradeViewObjectsCreated.Add(tradeHubCircle);
                        //tradeHubCircle.GetComponent<MeshRenderer>().material.color = new Color(0f, 153/255f, 0f); // set range circle to show green color

                        GameObject tradeHubCircle = Instantiate(TradeHubCircle, cirLocation, Quaternion.Euler(new Vector3(180, 0, 0))) as GameObject;
                        CurvedText[] tradeHubComponents = tradeHubCircle.GetComponentsInChildren<CurvedText>(); // get each component
                        tradeHubComponents[0].text = tradeGroupName; // get the first text component assigned
                        //tradeHubComponents[1].text = tradeGroupName; // get the second text component assigned
                        //tradeHubComponents[3].text = TotalMerchantSkill.ToString("N0");
                        tradeHubComponents[1].color = tradeGroupColor;
                        tradeHubCircle.transform.localScale = new Vector3(hubRange / 10f, hubRange / 10f, 0); // expand the circle
                        tradeHubCircle.GetComponentInChildren<SpriteRenderer>().color = new Color(tradeGroupColor.r, tradeGroupColor.g, tradeGroupColor.b, .25f); // set the transparency on the circle to half
                        listTradeViewObjectsCreated.Add(tradeHubCircle);
                    }
                }
            }
        }

        tradeHubsGenerated = true;
    }

    void ShowTradeHubRanges()
    {
        if (listTradeViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listTradeViewObjectsCreated)
            {
                rCircle.SetActive(true);
                if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
                {
                    // something something dark side
                }
            }
        }
    }

    void HideTradeHubRanges()
    {
        if (listTradeViewObjectsCreated.Count > 0)
        {
            foreach (GameObject rCircle in listTradeViewObjectsCreated)
            {
                rCircle.SetActive(false);
            }
        }     
    }

    void HideTradeDataBlocks()
    {
        foreach (GameObject star in listTradeTextBlocksCreated)
        {
            star.SetActive(false);
        }      
    }

    void ShowTradeDataBlocks()
    {
        foreach (GameObject star in listTradeTextBlocksCreated)
        {
            star.SetActive(true);
        }
    }

    void UpdateTradeDataBlocks()
    {
        Vector3 textLocation;
        TradeViewSystemData sysData;

        foreach (GameObject tradeBlock in listTradeTextBlocksCreated)
        {
            if (tradeBlock.activeSelf)
            {
                Color textColor = new Color();
                sysData = tradeBlock.GetComponent<TradeViewSystemData>();
                StarData starData = sysData.starData;

                if (!gameDataRef.DebugMode)
                {
                    int fleetsAvailable = 0;
                    tradeBlock.GetComponentInChildren<TextMeshProUGUI>().text = starData.Name.ToUpper();
                   
                    foreach (PlanetData pData in starData.PlanetList)
                    {
                        if (pData.TradeStatus != PlanetData.eTradeStatus.ImportOnly || pData.TradeStatus != PlanetData.eTradeStatus.NoTrade)
                        {
                            fleetsAvailable += (int)Mathf.Floor(pData.MerchantsAvailableForExport / 20);
                        }
                    }

                    if (starData.SystemIsTradeHub && starData.IntelLevel > eStellarIntelLevel.Medium)
                    {
                        tradeBlock.transform.Find("Fleets Available").GetComponent<TextMeshProUGUI>().text = fleetsAvailable.ToString("N0");
                        tradeBlock.transform.Find("Fleets Available").GetComponent<TextMeshProUGUI>().enabled = true;
                    }
                    else
                        tradeBlock.transform.Find("Fleets Available").GetComponent<TextMeshProUGUI>().enabled = false;
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

    void GenerateTradeDataBlocks()
    {
        Vector3 textLocation;
        Vector3 textLocation2;

        if (listTradeTextBlocksCreated.Count > 0)
        {
            DestroyTradeDataBlocks();
        }

        foreach (GameObject star in galaxyDataRef.GalaxyStarList)
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
                    GameObject tradeInfoBlock = Instantiate(tradeModeDataObject, textLocation, Quaternion.identity) as GameObject;                                                                                     
                    tradeInfoBlock.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the blocks to the panel
                    tradeInfoBlock.GetComponent<TradeViewSystemData>().starData = star.GetComponent<Star>().starData; // attach the star data to the object
                    tradeInfoBlock.GetComponent<TradeViewSystemData>().starDataName = tradeInfoBlock.GetComponent<TradeViewSystemData>().starData.Name; // attach the star data to the object
                    Transform exportPanel = tradeInfoBlock.transform.FindChild("Trade Export Panel");
                    Transform importPanel = tradeInfoBlock.transform.FindChild("Trade Import Panel");

                    // set the base stats
                    tradeInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().enabled = true;
                    tradeInfoBlock.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // scale systems with low intel down
                    //tradeInfoBlock.transform.Rotate(GalaxyCameraScript.cameraTilt, 0, 0); // test

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
                                    tradeInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().sprite = graphicAssets.EmpireCrestList.Find(p => p.name == "CREST" + civNumber); // attach the crest                            
                                    DetermineImportExports(importPanel, exportPanel, sys); // draw the appropriate exported goods

                                    if (sys.IntelLevel <= eStellarIntelLevel.Medium)
                                    {
                                        tradeInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().enabled = false;
                                        tradeInfoBlock.transform.localScale = new Vector3(.85f, .85f, 1); // do not scale
                                        exportPanel.gameObject.SetActive(false);
                                        importPanel.gameObject.SetActive(false);
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
                        tradeInfoBlock.transform.Find("Empire Icon").GetComponent<Image>().enabled = false; // don't show icons of stars that don't have an owner!
                        tradeInfoBlock.transform.localScale = new Vector3(.85f, .85f, 1); // do not scale
                        importPanel.gameObject.SetActive(false);
                        exportPanel.gameObject.SetActive(false);                      
                    }

                    // assign the color of the civ, set the local position and name of the block
                    tradeInfoBlock.GetComponentInChildren<TextMeshProUGUI>().color = civColor;
                    tradeInfoBlock.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)
                    tradeInfoBlock.name = star.GetComponent<Star>().starData.ID;

                    // assign to the star data block                  
                    tradeInfoBlock.GetComponent<TradeViewSystemData>().ownerName = civNames;
                    tradeInfoBlock.GetComponent<TradeViewSystemData>().starTransform = star.transform;
                    tradeInfoBlock.GetComponent<TradeViewSystemData>().ownerColor = civColor;

                    //starDataBlock.textObject = tradeInfoBlock;
                    listTradeTextBlocksCreated.Add(tradeInfoBlock);
                }
            }
        }
    
        tradeInfoDrawn = true;
    }

    private void DetermineImportExports(Transform importPanel, Transform exportPanel, StarData sys)
    {
        Image impFood = importPanel.Find("Food Export Image").GetComponent<Image>();
        Image impEnergy = importPanel.Find("Energy Export Image").GetComponent<Image>();
        Image impBasic = importPanel.Find("Basic Materials Export Image").GetComponent<Image>();
        Image impHeavy = importPanel.Find("Heavy Materials Export Image").GetComponent<Image>();
        Image impRare = importPanel.Find("Rare Materials Export Image").GetComponent<Image>();

        // reset the import need icons
        impFood.enabled = false;
        impEnergy.enabled = false;
        impBasic.enabled = false;
        impHeavy.enabled = false;
        impRare.enabled = false;

        Image expFood = exportPanel.Find("Food Export Image").GetComponent<Image>();
        Image expEnergy = exportPanel.Find("Energy Export Image").GetComponent<Image>();
        Image expBasic = exportPanel.Find("Basic Materials Export Image").GetComponent<Image>();
        Image expHeavy = exportPanel.Find("Heavy Materials Export Image").GetComponent<Image>();
        Image expRare = exportPanel.Find("Rare Materials Export Image").GetComponent<Image>();

        PlanetData systemHub;

        // check first for import wants, turn on icon if at least one planet in the system needs the resource
        foreach (PlanetData sysData in sys.PlanetList)
        {
            if (sysData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Food))
                impFood.enabled = true;

            if (sysData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Energy))
                impEnergy.enabled = true;

            if (sysData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Basic))
                impBasic.enabled = true;

            if (sysData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Heavy))
                impHeavy.enabled = true;

            if (sysData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Rare))
                impRare.enabled = true;
        }

        if (sys.SystemIsTradeHub)
        {
            systemHub = (DataRetrivalFunctions.GetPlanet(sys.SystemCapitalID));

            // check for exports first
            if (systemHub.FoodExportAvailable <= 0)
                expFood.enabled = false;
            
            if (systemHub.EnergyExportAvailable <= 0)
                expEnergy.enabled = false;
            
            if (systemHub.BasicExportAvailable <= 0)
                expBasic.enabled = false;
           
            if (systemHub.HeavyExportAvailable <= 0 )
                expHeavy.enabled = false;
           
            if (systemHub.RareExportAvailable <= 0)
                expRare.enabled = false;

            
        }
        else
            exportPanel.gameObject.SetActive(false); // there isn't a trade hub here, so shut off the trade

    }
}
