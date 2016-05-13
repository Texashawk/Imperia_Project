using UnityEngine;
using System.Collections.Generic;
using StellarObjects;
using CivObjects;
using TMPro;
using HelperFunctions;
using CameraScripts;

namespace Managers
{
    class CalloutManager : MonoBehaviour
    {
        // the prefabs for each callout type
        public GameObject ExplorationCallout;
        public GameObject MilitaryCallout;
        public GameObject PoliticalCallout;

        private List<GameObject> listCalloutsCreated = new List<GameObject>(); // list of all callouts created (lines, range circles, etc)
        private GameData gameDataRef; // game data reference
        private TradeManager tManagerRef; // trade manager data reference;
        private GalaxyData galaxyDataRef;  // stellar data reference
        private UIManager uiManagerRef; // UI Manager reference
        private bool calloutsGenerated = false;
        private Canvas galaxyPlanetInfoCanvas; // where the trade info is drawn (not the 3D objects)
        private GraphicAssets graphicAssets;

        void Awake()
        {
            gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); // get global game data (date, location, version, etc)
            galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // get global game data (date, location, version, etc)
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
            galaxyPlanetInfoCanvas = GameObject.Find("Galaxy Planet Info Canvas").GetComponent<Canvas>();
            graphicAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
            tManagerRef = GameObject.Find("GameManager").GetComponent<TradeManager>();
        }

        void Update()
        {
            // check whether the mode is active and if so, show what needs to be shown
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
            {
                UpdateCalloutView();
            }
            else
                ClearView(); // destroy all objects so that they can be rebuilt on new view      

            //if (uiManagerRef.RequestPoliticalViewGraphicRefresh)
            //    uiManagerRef.RequestPoliticalViewGraphicRefresh = false;
        }

        void UpdateCalloutView()
        {
            if (!calloutsGenerated) // if callouts are not yet generated, create them
                GenerateCallouts();

            // show if in galaxy mode, otherwise hide
            if (uiManagerRef.ViewLevel == ViewManager.eViewLevel.Galaxy)
            {
                ShowCallouts();
                UpdateCalloutLocations();
            }
            else
            {
                HideCallouts();
            }
        }

        void HideCallouts()
        {
            foreach (GameObject callout in listCalloutsCreated)
            {
                callout.SetActive(false);
            }
        }

        void ShowCallouts()
        {
            foreach (GameObject callout in listCalloutsCreated)
            {
                switch (uiManagerRef.PrimaryViewMode)
                {
                    case ViewManager.ePrimaryView.Economic:
                        if (callout.tag == "Economic Callout") // filter eventually depending on the tag and the view mode, use switch
                            callout.SetActive(true);
                        else
                            callout.SetActive(false);
                        break;
                    case ViewManager.ePrimaryView.Political:
                        if (callout.tag == "Exploration Callout") // filter eventually depending on the tag and the view mode, use switch
                            callout.SetActive(true);
                        else
                            callout.SetActive(false);
                        break;
                    case ViewManager.ePrimaryView.Military:
                        if (callout.tag == "Military Callout") // filter eventually depending on the tag and the view mode, use switch
                            callout.SetActive(true);
                        else
                            callout.SetActive(false);
                        break;
                    case ViewManager.ePrimaryView.Demographic:
                        callout.SetActive(false); // no pops one yet
                        break;
                    default:
                        break;
                }
                
            }
        }

        void ClearView()
        {
            DestroyCallouts();
            if (listCalloutsCreated.Count > 0)
            {
                foreach (GameObject callout in listCalloutsCreated)
                    Destroy(callout); // remove the circles
            }

            listCalloutsCreated.Clear();
            calloutsGenerated = false;
        }

        void DestroyCallouts()
        {
            if (listCalloutsCreated.Count > 0)
            {
                foreach (GameObject tBlock in listCalloutsCreated)
                {
                    Destroy(tBlock); // remove the text blocks
                }
            }

            listCalloutsCreated.Clear();
            calloutsGenerated = false;
        }

        void GenerateAllCallouts(string calloutType, Vector3 textLocation, GameObject star)
        {
            GameObject calloutObject = null; // the empty container for which callout is used
            GameObject callout = null;
            string civNames = "";

            switch (calloutType)
            {
                case "Exploration":
                    calloutObject = ExplorationCallout;
                    break;
                case "Military":
                    calloutObject = MilitaryCallout;
                    break;
                case "Political":
                    calloutObject = PoliticalCallout;
                    break;
                default:
                    calloutObject = PoliticalCallout;
                    break;
            }

            if (calloutObject != null)
                callout = Instantiate(calloutObject, textLocation, Quaternion.identity) as GameObject;
            else
                return;

            callout.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the callout to the panel
            callout.GetComponent<CalloutSystemData>().starData = star.GetComponent<Star>().starData; // attach the star data to the object
            callout.GetComponent<CalloutSystemData>().starDataName = callout.GetComponent<CalloutSystemData>().starData.Name; // attach the star data to the object

            // set the base stats
           
            //callout.transform.Rotate(GalaxyCameraScript.cameraTilt - 10, 0, 0); // test
            // add the owning civs if present      
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<StarData> civSystems = new List<StarData>(); // assign to a temp list each system in a civ
                StarData sData;
                civSystems = DataRetrivalFunctions.GetCivSystemList(civ);

                if (civSystems.Exists(p => p.ID == star.GetComponent<Star>().starData.ID))     
                    sData = civSystems.Find(p => p.ID == star.GetComponent<Star>().starData.ID);
                   
                else             
                    sData = star.GetComponent<Star>().starData;                 

                switch (calloutType)
                {
                    case "Exploration":
                        InitializeExplorationCallout(callout, sData.PlanetList.Count, sData);
                        break;
                    default:
                        InitializeExplorationCallout(callout, sData.PlanetList.Count, sData);  // default for now always
                        break;
                }

                callout.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2) + 5, textLocation.y - (Screen.height / 2), -5); // reset after making a parent to canvas relative coordinates (pivot in center)
                callout.name = star.GetComponent<Star>().starData.ID;

                // assign to the star data block                  
                callout.GetComponent<CalloutSystemData>().ownerName = civNames;
                callout.GetComponent<CalloutSystemData>().starTransform = star.transform;
   
                listCalloutsCreated.Add(callout);
            }

            
        }

        private void InitializeExplorationCallout(GameObject callout, int planetCount, StarData star)
        {
            TextMeshProUGUI[] calloutComponents = callout.GetComponentsInChildren<TextMeshProUGUI>(); // get each component
            Color civColor = Color.white;
            int index = 0;
            
            for (int x = 0; x < planetCount; x++)
            {
                if (star.PlanetList[x].Owner != null)
                {
                    civColor = star.PlanetList[x].Owner.Color;
                }
                else
                {
                    civColor = Color.white;
                }

                calloutComponents[index].text = star.PlanetList[x].Name.ToUpper(); // get the first text component assigned
                calloutComponents[index].color = civColor;

                calloutComponents[index + 1].text = star.PlanetList[x].ScanLevel.ToString("P0"); // get the second text component assigned
                calloutComponents[index + 1].color = StringConversions.GetTextValueColor((int)(star.PlanetList[x].ScanLevel * 100f));

                if (star.PlanetList[x].AtmosphereScanLevel >= 1f)
                {
                    calloutComponents[index + 2].text = star.PlanetList[x].AdjustedBio.ToString("N0"); // get the second text component assigned
                    calloutComponents[index + 2].color = StringConversions.GetTextValueColor(star.PlanetList[x].AdjustedBio);
                }
                else
                {
                    calloutComponents[index + 2].text = "??";
                    calloutComponents[index + 2].color = Color.gray;
                }

                if (star.PlanetList[x].SurfaceScanLevel >= .5f)
                {
                    calloutComponents[index + 3].text = star.PlanetList[x].BasicMaterials.ToString("N0"); // get the third text component assigned
                    calloutComponents[index + 3].color = StringConversions.GetTextValueColor(star.PlanetList[x].BasicMaterials);
                }
                else
                {
                    calloutComponents[index + 3].text = "??";
                    calloutComponents[index + 3].color = Color.gray;
                }

                if (star.PlanetList[x].InteriorScanLevel >= .75f)
                {
                    calloutComponents[index + 4].text = star.PlanetList[x].HeavyMaterials.ToString("N0"); // get the third text component assigned
                    calloutComponents[index + 4].color = StringConversions.GetTextValueColor(star.PlanetList[x].HeavyMaterials);
                }
                else
                {
                    calloutComponents[index + 4].text = "??";
                    calloutComponents[index + 4].color = Color.grey;
                }

                if ((star.PlanetList[x].InteriorScanLevel >= 1f))
                {
                    calloutComponents[index + 5].text = star.PlanetList[x].RareMaterials.ToString("N0"); // get the final text component assigned
                    calloutComponents[index + 5].color = StringConversions.GetTextValueColor(star.PlanetList[x].RareMaterials);
                }
                else
                {
                    calloutComponents[index + 5].text = "??";
                    calloutComponents[index + 5].color = Color.grey;
                }

                index += 6;
            }
        }

        private void GenerateCallouts()
        {
            Vector3 textLocation;

            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.GetComponent<Star>().tag != "Companion Star" && star.GetComponent<Star>().starData.IntelLevel > eStellarIntelLevel.Medium && star.GetComponent<Star>().starData.PlanetList.Count > 0) // if not a companion star and the intel is high enough, and if there are any planets!
                    {
                        var nameVector = Camera.main.WorldToScreenPoint(star.transform.position); // gets the screen point of the star's transform position
                        textLocation = new Vector3(nameVector.x, nameVector.y, 0); // where the text box is located                      

                        // create all the callouts at once
                        GenerateAllCallouts("Exploration", textLocation, star);
                    }
                }
            }
            calloutsGenerated = true;
        }

        private void UpdateCalloutLocations()
        {
            Vector3 textLocation;
            CalloutSystemData sysData;

            foreach (GameObject callout in listCalloutsCreated)
            {
                if (callout.activeSelf)
                {
                    sysData = callout.GetComponent<CalloutSystemData>();            

                    // reset location of data line blocks
                    Vector3 nameVector;
                    float fov = Camera.main.fieldOfView;
                    nameVector = Camera.main.WorldToScreenPoint(sysData.starTransform.position);
                    textLocation = nameVector;
                    // callout.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2) - 3 + ((45f / uiManagerRef.cameraFOV) * 23), textLocation.y - (Screen.height / 2) - 35 - ((45f / uiManagerRef.cameraFOV) * 10), 5); // reset after making a parent to canvas relative coordinates (pivot in center)
                    callout.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2) - 5, textLocation.y - (Screen.height / 2) - 10, 5); // reset after making a parent to canvas relative coordinates (pivot in center)
                    // callout.transform.localScale = new Vector3(GalaxyCameraScript.maxZoomLevel / fov, GalaxyCameraScript.maxZoomLevel / fov, GalaxyCameraScript.maxZoomLevel / fov) / 1.75f;
                    callout.transform.localScale = new Vector3(.65f, .65f, .65f);

                }
            }
        }

    }
}
