using UnityEngine;
using Managers;
using Projects;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using CharacterObjects;
using TMPro;
using HelperFunctions;

public class ProjectScreenScript : MonoBehaviour, IHasBeenDeleted, IAdminUpdated, IContributorUpdated

{
    public enum eRangeFilter : int
    {
        Planet,
        System,
        Province,
        Empire
    }

    // setup references
    UIManager uiManagerRef;
    GameData gDataRef;
    Project activeProject = new Project();
    public GameObject CharacterCard; // the large character card
    public GameObject LockSlot; // showing what you can not drag
    public GameObject OpenSlot; // slot where you can drag characters
    public string ProjectID { get; set; }    
    public string LocationID { get; set; }
    private string projLocation;
    private string projPlanetLocationID;
    private string projSysLocationID;
    private string projProvLocationID;

    // lists of cards
    private List<GameObject> CharacterCards = new List<GameObject>();
    private List<GameObject> ContributorSlots = new List<GameObject>();

    // list of characters
    private List<Character> CharactersInProject = new List<Character>();

    // filter variables
    private eRangeFilter rangeFilter = eRangeFilter.Planet;
    private string[] houseIDsFiltered = new string[10];

    // UI elements
    Button cancelButton;
    Button planetRangeButton;
    Button systemRangeButton;
    Button provinceRangeButton;
    Button empireRangeButton;
    Button executeButton;
    Button admToggleButton;
    Button wealthToggleButton;
    Button skillSortButton;
    Button admSortButton;
    Button wealthSortButton;
    GameObject dropDownContent;
    public GameObject tab1;
    public GameObject tab2;
    public GameObject tab3;

    TextMeshProUGUI energyRequired;
    TextMeshProUGUI basicRequired;
    TextMeshProUGUI heavyRequired;
    TextMeshProUGUI rareRequired;
    TextMeshProUGUI projectLocation;
    TextMeshProUGUI prestige;
    TextMeshProUGUI ADMRequired;
    TextMeshProUGUI projectName;

    TextMeshProUGUI admContributionLabel;
    TextMeshProUGUI moneyContributionLabel;
    TextMeshProUGUI turnEstimateLabel;
    TextMeshProUGUI turnEstimateUnitLabel;

    // canvases
    GridLayoutGroup characterCardTable;
    GridLayoutGroup contributorCardTable;

    // variables during creation of Project
    private List<Character> currentParticipatingCharacters = new List<Character>();
    private float currentADMPledged = 0f;
    private float currentMoneyPercentagePledged = 0f;
    private int turnEstimate = 0;
    private bool materialsAvailableForProject = true;

    // UI variables
    private int currentPage = 0;
    private int maxPages = 1;
    private int unlockedContributorSlots = 0;
    private Color selectedTabColor = new Color(0f, .35f, .71f, .8f);
    private Color baseTabColor = new Color(0f, .24f, .37f, .5f);

    // filter variables
    private bool ADMNotZero = false;
    private bool WealthNotZero = false;
    private bool SortBySkill = false;
    private bool SortByADM = false;
    private bool SortByWealth = false;

	// Use this for initialization
	void Awake ()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        projectName = transform.Find("Container/Title_Bar/Title").GetComponent<TextMeshProUGUI>();
        characterCardTable = transform.Find("Character_Selection/Character_List/Characterlist_Container_Grid").GetComponent<GridLayoutGroup>();
        contributorCardTable = transform.Find("Container/Project/Contributors/Contributors_Container/Small_Contributors_Grid").GetComponent<GridLayoutGroup>();
        energyRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Energy_Container/Counter_Mat_Energy").GetComponent<TextMeshProUGUI>();
        basicRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Basic_Container/Counter_Mat_Basic").GetComponent<TextMeshProUGUI>();
        heavyRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Heavy_Container/Counter_Mat_Heavy").GetComponent<TextMeshProUGUI>();
        rareRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Rare_Container/Counter_Mat_Rare").GetComponent<TextMeshProUGUI>();
        ADMRequired = transform.Find("Container/Project/ADM_Required/Rating_Container/ADM_Counter").GetComponent<TextMeshProUGUI>();
        prestige = transform.Find("Container/Project/Prestige/Rating_Container/Prestige_Counter").GetComponent<TextMeshProUGUI>();
        projectLocation = transform.Find("Container/Title_Bar/Location/Location_Name").GetComponent<TextMeshProUGUI>();
        admContributionLabel = transform.Find("Container/Project/They_must_Contribute/ADM_Basic_Container/Counter_ADM_Basic").GetComponent<TextMeshProUGUI>();
        moneyContributionLabel = transform.Find("Container/Project/They_must_Contribute/Money_Basic_Container/Counter_Money_Basic").GetComponent<TextMeshProUGUI>();
        turnEstimateLabel = transform.Find("Container/Project/Est_Time/Turn_Container/Counter_Turns_Basic").GetComponent<TextMeshProUGUI>();
        turnEstimateUnitLabel = transform.Find("Container/Project/Est_Time/Turn_Container/Counter_Turns_Label").GetComponent<TextMeshProUGUI>();
        cancelButton = transform.Find("Container/Project/BTN_CANCEL").GetComponent<Button>();
        executeButton = transform.Find("Container/Project/BTN_START").GetComponent<Button>();
        planetRangeButton = transform.Find("Character_Selection/Filter_Bar/Filters_Container_Grid/Planet Range").GetComponent<Button>();
        systemRangeButton = transform.Find("Character_Selection/Filter_Bar/Filters_Container_Grid/System Range").GetComponent<Button>();
        provinceRangeButton = transform.Find("Character_Selection/Filter_Bar/Filters_Container_Grid/Province Range").GetComponent<Button>();
        empireRangeButton = transform.Find("Character_Selection/Filter_Bar/Filters_Container_Grid/Empire Range").GetComponent<Button>();
        dropDownContent = GameObject.Find("Character_Selection/Filter_Bar/Filters_Container_Grid/Range/Dropdown_Content");
        admToggleButton = transform.Find("Character_Selection/Filter_Bar/Sort_Container_Grid/ADM_Above_Zero").GetComponent<Button>();
        wealthToggleButton = transform.Find("Character_Selection/Filter_Bar/Sort_Container_Grid/Wealth_Above_Zero").GetComponent<Button>();
        skillSortButton = transform.Find("Character_Selection/Filter_Bar/Sort_Container_Grid/Sort_By_Skill").GetComponent<Button>();
        admSortButton = transform.Find("Character_Selection/Filter_Bar/Sort_Container_Grid/Sort_By_ADM").GetComponent<Button>();
        wealthSortButton = transform.Find("Character_Selection/Filter_Bar/Sort_Container_Grid/Sort_By_Money").GetComponent<Button>();


        // delegates
        cancelButton.onClick.AddListener(delegate { ExitScreen(); }); // button is clicked, so activate the Project Screen
        executeButton.onClick.AddListener(delegate { AddProject(); }); // button is clicked, so activate the Project Screen
        planetRangeButton.onClick.AddListener(delegate { SetRangeToPlanet(); }); // sort buttons
        systemRangeButton.onClick.AddListener(delegate { SetRangeToSystem(); }); // sort buttons
        provinceRangeButton.onClick.AddListener(delegate { SetRangeToProvince(); }); // sort buttons
        empireRangeButton.onClick.AddListener(delegate { SetRangeToEmpire(); }); // sort buttons
        admToggleButton.onClick.AddListener(delegate { SetADMThreshold(); });
        wealthToggleButton.onClick.AddListener(delegate { SetWealthThreshold(); });
        skillSortButton.onClick.AddListener(delegate { SetSkillSortToggle(); }); // sort buttons
        admSortButton.onClick.AddListener(delegate { SetADMSortToggle(); });
        wealthSortButton.onClick.AddListener(delegate { SetWealthSortToggle(); });
    }

    void Start()
    {
        activeProject = gDataRef.ProjectDataList.Find(p => p.ID == ProjectID); // assigns the working project
        if (activeProject == null)
            Destroy(gameObject); // don't create 
        else
        {
            InitializeScreen();
            CheckForEmpireResources();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        uiManagerRef.ModalIsActive = true; // to prevent issues with the background

        if (turnEstimate == 0)
        {
            turnEstimateLabel.text = "";
            turnEstimateUnitLabel.text = "Never";
        }
        else
        {
            turnEstimateLabel.text = turnEstimate.ToString("N0");
            turnEstimateUnitLabel.text = "Turns";
        }

        admContributionLabel.text = currentADMPledged.ToString("N0");
        moneyContributionLabel.text = currentMoneyPercentagePledged.ToString("P0") + " OF " + activeProject.BaseCostReq.ToString("N0") + "bn";

        CalculateTurnsToComplete();
        CheckForMaxFunding();

        if (gDataRef.CivList[0].PlayerEmperor.ActionPoints < Constants.Constant.APsRequiredForProject)
        {
            transform.Find("Container/Project/BTN_START/Container/Title").GetComponent<Text>().text = "INSUFFICIENT APs";
            executeButton.interactable = false;
        }

        if (!materialsAvailableForProject)
        {
            transform.Find("Container/Project/BTN_START/Container/Title").GetComponent<Text>().text = "INSUFFICIENT MATERIALS";
            executeButton.interactable = false;
        }

    }

    public void SetToPage1()
    {
        currentPage = 0;
        tab1.GetComponent<Image>().color = selectedTabColor;
        tab2.GetComponent<Image>().color = baseTabColor;
        tab3.GetComponent<Image>().color = baseTabColor;
        RedrawCharacterCards();
    }

    public void SetToPage2()
    {
        currentPage = 1;
        tab2.GetComponent<Image>().color = selectedTabColor;
        tab1.GetComponent<Image>().color = baseTabColor;
        tab3.GetComponent<Image>().color = baseTabColor;
        RedrawCharacterCards();
    }

    public void SetToPage3()
    {
        currentPage = 2;
        tab3.GetComponent<Image>().color = selectedTabColor;
        tab2.GetComponent<Image>().color = baseTabColor;
        tab1.GetComponent<Image>().color = baseTabColor;
        RedrawCharacterCards();
    }

    private void CheckForMaxFunding()
    {
        if (currentMoneyPercentagePledged >= 1f)
        {
            transform.Find("Container/Project/BTN_START/Container/Title").GetComponent<Text>().text = "EXECUTE";
            executeButton.interactable = true;
        }
        else
        {
            transform.Find("Container/Project/BTN_START/Container/Title").GetComponent<Text>().text = "INSUFFICIENT FUNDS";
            executeButton.interactable = false;
        }

    }

    private void CheckForEmpireResources()
    {
        materialsAvailableForProject = true; // initially set as true

        if (DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).EnergyStored < activeProject.BaseEnergyReq)
            materialsAvailableForProject = false;

        if (DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).BasicStored < activeProject.BaseBasicReq)
            materialsAvailableForProject = false;

        if (DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).HeavyStored < activeProject.BaseHeavyReq)
            materialsAvailableForProject = false;

        if (DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).RareStored < activeProject.BaseHeavyReq)
            materialsAvailableForProject = false;

    }

    private void CalculateTurnsToComplete()
    {
        if (currentADMPledged == 0)
        {
            turnEstimate = 0;
        }
        else
        {
            turnEstimate = Mathf.FloorToInt((int)activeProject.BaseADMReq / (int)currentADMPledged);
        }
    
    }

    private void AddCharacterCard(Character cData)
    {
        GameObject go = Instantiate(CharacterCard);
        go.SetActive(true);
        go.name = cData.ID;
        CharacterCard CC = go.GetComponent<CharacterCard>();
        CC.InitializeCard(cData, activeProject);     
        CharacterCards.Add(go);
    }

    private void UpdateAllCharacterCards()
    {
        foreach (GameObject cData in CharacterCards)
        {
            cData.GetComponent<CharacterCard>().UpdateCard(cData.name, activeProject);
        }
    }

    private void UpdateContributorSlots()
    {
        foreach (GameObject slot in ContributorSlots)
        {
            Destroy(slot);
        }

        for (int x = 0; x < unlockedContributorSlots; x++)
        {
            GameObject us = Instantiate(OpenSlot);
            us.SetActive(true);
            us.transform.SetParent(contributorCardTable.transform);
            us.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
            us.transform.localPosition = new Vector3(us.transform.localPosition.x, us.transform.localPosition.y, 0); // to set on flat plane
            ContributorSlots.Add(us);
        }

        for (int x = 0; x < 10 - unlockedContributorSlots; x++)
        {
            GameObject ls = Instantiate(LockSlot);
            ls.SetActive(true);
            ls.transform.SetParent(contributorCardTable.transform);
            ls.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
            ls.transform.localPosition = new Vector3(ls.transform.localPosition.x, ls.transform.localPosition.y, 0); // to set on flat plane
            ContributorSlots.Add(ls);
        }
    }

    void ExitScreen()
    {
        foreach (GameObject cc in CharacterCards)
        {
            DataRetrivalFunctions.GetCharacter(cc.GetComponent<CharacterCard>().CharID).HasActiveProject = false;
            cc.GetComponent<CharacterCard>().isAdminstrator = false;
            cc.GetComponent<CharacterCard>().isContributor = false;
            Destroy(cc);
        }
               
        uiManagerRef.ModalIsActive = false; // reset modal set
        Destroy(gameObject);
    }

    void AddProject()
    {
        if (currentMoneyPercentagePledged >= 1f)
        {
            foreach (string cID in activeProject.CharacterIDsInProject)
            {
                // code to remove wealth here (must account for total contributions > 100%)
                DataRetrivalFunctions.GetCharacter(cID).HasActiveProject = true;
                float percentToRemove = DataRetrivalFunctions.DetermineContributionToProject(DataRetrivalFunctions.GetCharacter(cID), activeProject);
                DataRetrivalFunctions.GetCharacter(cID).Wealth -= DataRetrivalFunctions.GetCharacter(cID).Wealth * percentToRemove;
                activeProject.ADMAllocatedPerTurn += DataRetrivalFunctions.DetermineAdminAvailable(DataRetrivalFunctions.GetCharacter(cID));
            }
            activeProject.UniqueID = "PROJ" + Random.Range(0, 9999999).ToString("N0");
            activeProject.ActivateProject = false;
            activeProject.TotalADMAccumulation = 0;
            gDataRef.CivList[0].ActiveProjects.Add(activeProject);
            gDataRef.CivList[0].PlayerEmperor.ActionPoints -= Constants.Constant.APsRequiredForProject;
            
            DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).EnergyStored -= activeProject.BaseEnergyReq;
            DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).BasicStored -= activeProject.BaseBasicReq;
            DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).HeavyStored -= activeProject.BaseHeavyReq;
            DataRetrivalFunctions.GetPlanet(gDataRef.CivList[0].CapitalPlanetID).RareStored -= activeProject.BaseRareReq;

            ExitScreen();
        }
    }

    void SetRangeToPlanet()
    {
        rangeFilter = eRangeFilter.Planet;
        planetRangeButton.image.color = Color.green;
        systemRangeButton.image.color = Color.white;
        provinceRangeButton.image.color = Color.white;
        empireRangeButton.image.color = Color.white;
        RedrawCharacterCards();
    }

    void SetRangeToSystem()
    {
        rangeFilter = eRangeFilter.System;
        planetRangeButton.image.color = Color.white;
        systemRangeButton.image.color = Color.green;
        provinceRangeButton.image.color = Color.white;
        empireRangeButton.image.color = Color.white;
        RedrawCharacterCards();
    }

    void SetRangeToProvince()
    {
        rangeFilter = eRangeFilter.Province;
        planetRangeButton.image.color = Color.white;
        systemRangeButton.image.color = Color.white;
        provinceRangeButton.image.color = Color.green;
        empireRangeButton.image.color = Color.white;
        RedrawCharacterCards();
    }

    void SetRangeToEmpire()
    {
        rangeFilter = eRangeFilter.Empire;
        planetRangeButton.image.color = Color.white;
        systemRangeButton.image.color = Color.white;
        provinceRangeButton.image.color = Color.white;
        empireRangeButton.image.color = Color.green;
        RedrawCharacterCards();
    }

    void SetSkillSortToggle()
    {

        if (SortBySkill == true)
        {
            SortBySkill = false;
            skillSortButton.image.color = Color.white;
        }
        else
        {
            SortBySkill = true;
            skillSortButton.image.color = Color.green;
        }

        RedrawCharacterCards();
    }

    void SetADMSortToggle()
    {

        if (SortByADM == true)
        {
            SortByADM = false;
            admSortButton.image.color = Color.white;
        }
        else
        {
            SortByADM = true;
            admSortButton.image.color = Color.green;
        }

        RedrawCharacterCards();
    }

    void SetWealthSortToggle()
    {

        if (SortByWealth == true)
        {
            SortByWealth = false;
            wealthSortButton.image.color = Color.white;
        }
        else
        {
            SortByWealth = true;
            wealthSortButton.image.color = Color.green;
        }

        RedrawCharacterCards();
    }

    void SetADMThreshold()
    {
        if (ADMNotZero == true)
        {
            ADMNotZero = false;
            admToggleButton.image.color = Color.white;
        }
        else
        {
            ADMNotZero = true;
            admToggleButton.image.color = Color.green;
        }

        RedrawCharacterCards();
    }

    void SetWealthThreshold()
    {
        if (WealthNotZero == true)
        {
            WealthNotZero = false;
            wealthToggleButton.image.color = Color.white;
        }
        else
        {
            WealthNotZero = true;
            wealthToggleButton.image.color = Color.green;
        }

        RedrawCharacterCards();
    }

    void RedrawCharacterCards()
    {
        ClearCharacterTable();
        PopulateCharacterCardList();
        DrawCharacterCards();
    }

    void InitializeScreen()
    {
        if (LocationID.StartsWith("STA"))
        {
            projLocation = DataRetrivalFunctions.GetSystem(LocationID).Name;
            projPlanetLocationID = DataRetrivalFunctions.GetSystem(LocationID).SystemCapitalID;
            projSysLocationID = DataRetrivalFunctions.GetSystem(LocationID).ID;
            projProvLocationID = DataRetrivalFunctions.GetSystem(LocationID).AssignedProvinceID;
        }

        else if (LocationID.StartsWith("PRO"))
        {
            projLocation = DataRetrivalFunctions.GetProvince(LocationID).Name;
            projPlanetLocationID = DataRetrivalFunctions.GetProvince(LocationID).CapitalPlanetID;
            projSysLocationID = DataRetrivalFunctions.GetPlanet(DataRetrivalFunctions.GetProvince(LocationID).CapitalPlanetID).SystemID;
            projProvLocationID = DataRetrivalFunctions.GetProvince(LocationID).ID;
        }

        else
        {
            projLocation = DataRetrivalFunctions.GetPlanet(LocationID).Name;
            projPlanetLocationID = LocationID;
            projSysLocationID = DataRetrivalFunctions.GetPlanet(LocationID).SystemID;
            projProvLocationID = DataRetrivalFunctions.GetSystem(projSysLocationID).AssignedProvinceID;
        }

        projectName.text = activeProject.Name + ": " + projLocation; // test
        projectLocation.text = projLocation;
        energyRequired.text = activeProject.BaseEnergyReq.ToString("N0");
        basicRequired.text = activeProject.BaseBasicReq.ToString("N0");
        heavyRequired.text = activeProject.BaseHeavyReq.ToString("N0");
        rareRequired.text = activeProject.BaseRareReq.ToString("N0");
        ADMRequired.text = activeProject.BaseADMReq.ToString("N0");
        prestige.text = activeProject.BasePrestige.ToString("N0");
        admContributionLabel.text = currentADMPledged.ToString("N0");
        moneyContributionLabel.text = currentMoneyPercentagePledged.ToString("P0");

        PopulateCharacterCardList(); // populate the 'card table' with the initial filters
        UpdateContributorSlots();
        DrawCharacterCards();       
    }

    private void ClearCharacterTable()
    {
        // clear the table virtually
        foreach (GameObject cc in CharacterCards)
            if (!cc.GetComponent<CharacterCard>().isAdminstrator && !cc.GetComponent<CharacterCard>().isContributor)
                Destroy(cc);
    }

    private void DrawCharacterCards()
    {
        // set all the tabs inactive to start
        tab1.SetActive(false);
        tab2.SetActive(false);
        tab3.SetActive(false);

        maxPages = 0; // reset
        
        maxPages = Mathf.CeilToInt(CharacterCards.Count / 12f);
        if (maxPages > 0)
            tab1.SetActive(true);
        if (maxPages > 1)
            tab2.SetActive(true);
        if (maxPages > 2)
            tab3.SetActive(true);

        if (currentPage > (maxPages - 1)) // to prevent overflow and stay on a page that doesn't exist
            currentPage = (maxPages - 1);

        for (int x = currentPage * 12; x < (currentPage + 1) * 12; x++)
        {
            if (x < CharacterCards.Count)
            {
                GameObject go = CharacterCards[x];
                go.transform.SetParent(characterCardTable.transform);
                go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
                go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0); // to set on flat plane
            }
            else
                return;          
        }
    }

    private void PopulateCharacterCardList()
    {
        List<Character> tempCharacterList = new List<Character>();
        CharacterCards.Clear(); // clear out the deck

        foreach (Character cData in gDataRef.CharacterList)
        {
            if (cData.CivID == gDataRef.CivList[0].PlayerEmperor.CivID && cData.Role != Character.eRole.Emperor) // not you!
            {
                if (cData.HasActiveProject == false)
                {
                    switch (rangeFilter)  // check the range filter
                    {
                        case eRangeFilter.Planet:
                            if (cData.PlanetLocationID != null)
                            {
                                if (cData.PlanetLocationID == projPlanetLocationID)
                                    tempCharacterList.Add(cData);
                            }
                            break;
                        case eRangeFilter.System:
                            if (cData.PlanetLocationID != null)
                            {
                                if (cData.SystemLocationID == projSysLocationID)
                                    tempCharacterList.Add(cData);
                            }
                            break;
                        case eRangeFilter.Province:
                            if (cData.PlanetLocationID != null)
                            {
                                if (cData.ProvinceLocationID == projProvLocationID)
                                    tempCharacterList.Add(cData);
                            }
                            break;
                        case eRangeFilter.Empire:
                            tempCharacterList.Add(cData);
                            break;
                        default:
                            break;
                    }
                }
                else
                    Debug.Log("Character " + cData.Name + " is already on a project! Can't add...");
                
            }

        }

        
        

        // more filters will go here
        foreach (Character checkChar in tempCharacterList.ToArray())
        {
            if (ADMNotZero)
            {
                if (DataRetrivalFunctions.DetermineAdminAvailable(checkChar) == 0)
                {
                    tempCharacterList.Remove(checkChar);
                    tempCharacterList.TrimExcess();
                }
            }
        }

        foreach(Character checkChar in tempCharacterList.ToArray())
        { 
            if (WealthNotZero)
            {
                if (DataRetrivalFunctions.DetermineContributionToProject(checkChar,activeProject) == 0)
                {
                    tempCharacterList.Remove(checkChar);
                    tempCharacterList.TrimExcess();
                }
            }
        }

        // sorters are here
        if (SortBySkill)
        {
            tempCharacterList.Sort(delegate (Character i1, Character i2)
            {
                return i2.Administration.CompareTo(i1.Administration);
            });
        }

        if (SortByADM)
        {
            tempCharacterList.Sort(delegate (Character i1, Character i2)
            {
                return DataRetrivalFunctions.DetermineAdminAvailable(i2).CompareTo(DataRetrivalFunctions.DetermineAdminAvailable(i1));
            });
        }

        if (SortByWealth)
        {
            tempCharacterList.Sort(delegate (Character i1, Character i2)
            {
                return DataRetrivalFunctions.DetermineContributionToProject(i2, activeProject).CompareTo(DataRetrivalFunctions.DetermineContributionToProject(i1, activeProject));
            });
        }


        // after all filters have been added, populate
        foreach (Character cData in tempCharacterList)
        {
            AddCharacterCard(cData);

        }
    }

    public void HasBeenDeleted(string type, string ID)
    {
        // this is where you will check which name (charID) has been deleted
       

        if (type == "Administrator")
        {
            unlockedContributorSlots = 0; // resets
            currentADMPledged = 0; // resets
            currentMoneyPercentagePledged = 0; // resets
            activeProject.AdministratorID = null;
            
            foreach (string cID in activeProject.CharacterIDsInProject)
            {
                DataRetrivalFunctions.GetCharacter(cID).HasActiveProject = false;
            }
            activeProject.CharacterIDsInProject.Clear(); // resets
            UpdateContributorSlots();
        }

        if (type == "Contributor")
        {
            currentADMPledged -= DataRetrivalFunctions.DetermineAdminAvailable(DataRetrivalFunctions.GetCharacter(ID));
            activeProject.CharacterIDsInProject.Remove(ID);
            DataRetrivalFunctions.GetCharacter(ID).HasActiveProject = false;
            currentMoneyPercentagePledged -= DataRetrivalFunctions.DetermineContributionToProject(DataRetrivalFunctions.GetCharacter(ID), activeProject); // temp      
        }

        ClearCharacterTable();
        PopulateCharacterCardList();
        DrawCharacterCards();
        UpdateAllCharacterCards();
    }

    public void UpdateContributor(string ID)
    {
        activeProject.CharacterIDsInProject.Add(ID);
        DataRetrivalFunctions.GetCharacter(ID).HasActiveProject = true;
        currentADMPledged += DataRetrivalFunctions.DetermineAdminAvailable(DataRetrivalFunctions.GetCharacter(ID));
        currentMoneyPercentagePledged += DataRetrivalFunctions.DetermineContributionToProject(DataRetrivalFunctions.GetCharacter(ID),activeProject); // static for now, will need to write a function to determine this
        if (gDataRef.CivList[0].PlayerEmperor.ActionPoints > Constants.Constant.APsRequiredForProject)
            CheckForMaxFunding();
        UpdateAllCharacterCards();
    }

    public void UpdateAdministrator(string ID)
    {
        activeProject.AdministratorID = ID;
        activeProject.CharacterIDsInProject.Add(ID);
        unlockedContributorSlots = DataRetrivalFunctions.GetCharacter(ID).Administration;
        currentADMPledged += DataRetrivalFunctions.DetermineAdminAvailable(DataRetrivalFunctions.GetCharacter(ID));
        currentMoneyPercentagePledged += 0f; // static for now, will need to write a function to determine this
        UpdateContributorSlots();
        UpdateAllCharacterCards();
        if (gDataRef.CivList[0].PlayerEmperor.ActionPoints > Constants.Constant.APsRequiredForProject)
            CheckForMaxFunding();
    }
}

namespace UnityEngine.EventSystems
{
    public interface IHasBeenDeleted : IEventSystemHandler
    {
        void HasBeenDeleted(string type, string ID);
    }

    public interface IAdminUpdated : IEventSystemHandler
    {
        void UpdateAdministrator(string ID);
    }

    public interface IContributorUpdated : IEventSystemHandler
    {
        void UpdateContributor(string ID);
    }
}
