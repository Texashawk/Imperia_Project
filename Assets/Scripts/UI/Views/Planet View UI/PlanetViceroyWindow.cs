using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using CharacterObjects;
using Tooltips;
using TMPro;

public class PlanetViceroyWindow : MonoBehaviour {

    // references
    GameData gDataRef;
    UIManager uiManagerRef;
    GraphicAssets gAssetData;
    GalaxyView gScreenRef;
    PlanetData pData;

    // public game objects
    public GameObject Name;
    public GameObject Portrait;
    public GameObject AdminCounter;
    public GameObject RevenueCounter;
    public GameObject ShareCounter;
    public GameObject ChatWindow;
    

    // private components
    TextMeshProUGUI viceroyName;
    TextMeshProUGUI adminCounter;
    TextMeshProUGUI revenueCounter;
    TextMeshProUGUI shareCounter;
    TextMeshProUGUI chatWindow;
    Image viceroyImage;

    List<string> ChatLog = new List<string>(); // will probably need to be with the character object

    Character viceroy;
   
    // Use this for initialization
    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        gAssetData = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        adminCounter = AdminCounter.GetComponent<TextMeshProUGUI>();
        viceroyName = Name.GetComponent<TextMeshProUGUI>();
        revenueCounter = RevenueCounter.GetComponent<TextMeshProUGUI>();
        shareCounter = ShareCounter.GetComponent<TextMeshProUGUI>();
        viceroyImage = Portrait.GetComponent<Image>();
        chatWindow = ChatWindow.GetComponent<TextMeshProUGUI>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        // check for updated planet data
        if (uiManagerRef.selectedPlanet != null)
        {
            if (viceroy == null)
            {
                viceroy = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData.Viceroy;
                ShowViceroyInfo();
            }

            if (viceroy != null)
            {
                if (viceroy != gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData.Viceroy)
                {
                    viceroy = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData.Viceroy;
                    ShowViceroyInfo();
                }
            }         
        }      
    }

    void ShowViceroyInfo()
    {
        // build points
        viceroyName.text = viceroy.Name;
        adminCounter.text = viceroy.PlanetAssigned.TotalAdmin.ToString("N0");
        revenueCounter.text = (viceroy.PlanetAssigned.GrossPlanetaryProduct / 25f).ToString("N0") + "bn"; // update
        shareCounter.text = "25%";
        viceroyImage.sprite = gAssetData.CharacterList.Find(p => p.name == viceroy.PictureID);
        Portrait.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(viceroy, -(Portrait.GetComponent<RectTransform>().rect.width / 4f)); // set up the tooltip
        Portrait.transform.GetComponent<CharacterScreenActivation>().InitializeData(viceroy);
        chatWindow.text = viceroy.Name + "_" + "\n" + "Welcome, Emperor, what can I do for you ?";
    }
}
