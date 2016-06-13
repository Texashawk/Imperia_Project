using UnityEngine;
using System.Collections.Generic;
using PlanetObjects;
using CivObjects;
using CharacterObjects;
using EconomicObjects;
using Tooltips;
using Projects;
using Actions;

public class GameData : MonoBehaviour
{

    // ui mode enums
    public enum eSubMode : int
    {
        None,
        Intel,
        War,
        Finance,
        Diplomacy,
        Emperor,
        Science,
        NextTurn
    };

    public eSubMode uiSubMode = eSubMode.None;
    public bool CharacterWindowActive = false;
    public string CharacterTooltipIDActive;
    public GameObject activeTooltip = null;
    public bool StarSelected = false; // checks to see whether there is a star selected
    // public bool modalIsActive = false; // checks for modal window active
    public Character SelectedCharacter = null; // selected character ID

    public bool DebugMode = false; // allows to see all values during testing (remove for play builds)
    public bool RequestGraphicRefresh = false; // this determines whether a redraw is done

    // game screen backgrounds
    public Texture2D mainGameScreen;
    public Texture2D createWorldGameScreen;
    public Texture2D createEmperorScreen;

    // universal game data
    public string gameVersionNumber = "unknown";
    public string gameVersionSuffix = "a";
    public string gameVersion = "";
    public float GameDate { get; set; }
    public int GameMonth { get; set; }
    public int GameNumber { get; set; } // this is a random number for the game number for saves, logging, etc

    // gamewide object lists
    public List<Civilization> CivList = new List<Civilization>();
    public List<Pops> PopList = new List<Pops>(); // univeral population list in game
    public List<Challenge> ChallengeList = new List<Challenge>(); // tracks current challenges between different characters
    public List<TradeFleet> ActiveTradeFleets = new List<TradeFleet>();
    public List<Character> CharacterList = new List<Character>();
    public List<GameObject> ActiveTradeFleetObjects = new List<GameObject>();
    public List<Project> ProjectDataList = new List<Project>();
    public List<CharacterAction> CharacterActionList = new List<CharacterAction>();
    public List<CharacterTrait> CharacterTraitList = new List<CharacterTrait>();
    public List<House> HouseList = new List<House>(); // all houses generated in the game

    // galaxy setup variables
    public int GalaxySizeHeight { get; set; }
    public int GalaxySizeWidth { get; set; }
    public int MinSystems { get; set; }
    public int MaxSystems { get; set; }
    public int TotalSystems { get; set; }

    // empire setup variables
    public string PlayerEmpireName { get; set; }
    public GameSetupFile GameSetup { get; set; }

    // game setup variables
    public int NumberOfAICivs { get; set; }
    public bool CivsGenerated = false;

    // Use this for initialization
    void Awake()
    {
        CivList = new List<Civilization>(); // initialize lists
        GameDate = 3050.0f;

        GameMonth = 0;
        GalaxySizeHeight = 20000;
        GalaxySizeWidth = 20000;
        MinSystems = 90;
        GameNumber = Random.Range(0, 100000);
        MaxSystems = 115;
        TotalSystems = 150;
        NumberOfAICivs = 1; // static number to test UnityEngine.Random.Range(4, 7); // test
        PlayerEmpireName = "Celestial Empire";
        gameVersion = gameVersionNumber + gameVersionSuffix;
        //DataManager.InitializeData(); // intialize data all in one place (test)
    }

    void Start()
    {
        DataManager.InitializeData(); // intialize data all in one place (test)
    }

    public void UpdateGameDate()
    {
        GameDate += .1f;
        GameMonth += 1;
        if (GameMonth == 9)
        {
            GameMonth = 0;
        }
    }

    public void AddNewCivilization(Civilization civ)
    {
        CivList.Add(civ);
    }

} 
