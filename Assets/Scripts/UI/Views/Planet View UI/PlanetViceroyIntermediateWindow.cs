﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using CharacterObjects;
using Tooltips;
using ConversationAI;
using TMPro;

public class PlanetViceroyIntermediateWindow : MonoBehaviour, IPointerClickHandler {

    // references
    GameData gDataRef;
    UIManager uiManagerRef;
    GraphicAssets gAssetData;
    GalaxyView gScreenRef;
    PlanetView planetViewRef;
    PlanetData pData;

    // public game objects
    //public GameObject Name;
    //public GameObject Portrait;
    //public GameObject AdminCounter;
    //public GameObject RevenueCounter;
    //public GameObject ShareCounter;
    //public GameObject ChatWindow;
    //public GameObject Relationship;
    public GameObject ActionWindowTitle;


    // private components
    //TextMeshProUGUI viceroyName;
    //TextMeshProUGUI adminCounter;
    //TextMeshProUGUI revenueCounter;
    //TextMeshProUGUI shareCounter;
    //TextMeshProUGUI chatWindow;
    TextMeshProUGUI actionWindowTitle;
    //Image relationship;
    //Image viceroyImage;

    List<string> ChatLog = new List<string>(); // will probably need to be with the character object

    Character viceroy;

    // Use this for initialization
    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        gAssetData = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        planetViewRef = GameObject.Find("UI Engine").GetComponent<PlanetView>();
        actionWindowTitle = ActionWindowTitle.GetComponent<TextMeshProUGUI>();
        //adminCounter = AdminCounter.GetComponent<TextMeshProUGUI>();
        //viceroyName = Name.GetComponent<TextMeshProUGUI>();
        //revenueCounter = RevenueCounter.GetComponent<TextMeshProUGUI>();
        //shareCounter = ShareCounter.GetComponent<TextMeshProUGUI>();
        //viceroyImage = Portrait.GetComponent<Image>();
        //relationship = Relationship.GetComponent<Image>();
        //chatWindow = ChatWindow.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        // check for updated planet data
        if (uiManagerRef.selectedPlanet != null)
        {
            pData = uiManagerRef.selectedPlanet;
        }        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            planetViewRef.ToggleViceroyMode(name); // callback to tell the planet UI which window called the window
        }
    }

    public void ShowViceroyIntermediateInfo(string windowName)
    {
        // build points
        //viceroyName.text = viceroy.Name;
        //adminCounter.text = viceroy.PlanetAssigned.TotalAdmin.ToString("N0");
        //shareCounter.text = pData.Owner.ViceroyBaseTaxCut.ToString("P0");
        //revenueCounter.text = (pData.GrossPlanetaryProduct * pData.Owner.ViceroyBaseTaxCut).ToString("N0") + "bn";
        //viceroyImage.sprite = gAssetData.CharacterList.Find(p => p.name == viceroy.PictureID);
        //Portrait.transform.GetComponent<CharacterTooltip>().InitializeTooltipData(viceroy, -(Portrait.GetComponent<RectTransform>().rect.width / 5f)); // set up the tooltip
        //Portrait.transform.GetComponent<CharacterScreenActivation>().InitializeData(viceroy);
        //string introText = ConversationEngine.GenerateInitialDialogue(viceroy);
        //relationship.sprite = gAssetData.RelationshipIconList.Find(p => p.name == viceroy.Relationships[pData.Owner.LeaderID].RelationshipIcon);
        actionWindowTitle.text = windowName + " Actions";
        //chatWindow.text = viceroy.Name + "_" + "\n" + introText;
    }
}