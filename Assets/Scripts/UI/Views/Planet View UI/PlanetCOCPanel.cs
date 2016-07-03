using UnityEngine;
using UnityEngine.UI;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using TMPro;

public class PlanetCOCPanel : MonoBehaviour {

    /// references
    GameData gDataRef;
    UIManager uiManagerRef;
    GalaxyView gScreenRef;
    PlanetData pData;

    // public game objects


    // private components


    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
    }

    void Update()
    {
        // check for updated planet data

        if (uiManagerRef.selectedPlanet != null)
        {
            if (pData == null)
            {
                pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                UpdateCOCPanelInfo();
            }

            else if (pData != null)
            {
                if (pData != gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData)
                {
                    pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                    UpdateCOCPanelInfo();
                }
            }
        }
    }

    void UpdateCOCPanelInfo()
    {
        // build points
        
    }
}
