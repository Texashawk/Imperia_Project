using UnityEngine;
using Managers;
using Projects;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using CharacterObjects;
using TMPro;

public class ProjectScreenScript : MonoBehaviour, IPointerClickHandler

{
    // setup references
    UIManager uiManagerRef;
    GameData gDataRef;
    Project activeProject;
    public string ProjectID { get; set; }    
    public string LocationID { get; set; }
    private string projLocation;

    // UI elements
    Button cancelButton;
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

    // variables during creation of Project
    private List<Character> currentParticipatingCharacters = new List<Character>();
    private float currentADMPledged = 0f;
    private float currentMoneyPercentagePledged = 0f;
    private float turnEstimate = 0f;


	// Use this for initialization
	void Awake ()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        projectName = transform.Find("Container/Title_Bar/Title").GetComponent<TextMeshProUGUI>();
        energyRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Energy_Container/Counter_Mat_Energy").GetComponent<TextMeshProUGUI>();
        basicRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Basic_Container/Counter_Mat_Basic").GetComponent<TextMeshProUGUI>();
        heavyRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Heavy_Container/Counter_Mat_Heavy").GetComponent<TextMeshProUGUI>();
        rareRequired = transform.Find("Container/Project/Your_Contribution/Resources_Container/Rare_Container/Counter_Mat_Rare").GetComponent<TextMeshProUGUI>();
        ADMRequired = transform.Find("Container/Project/ADM_Required/Rating_Container/ADM_Counter").GetComponent<TextMeshProUGUI>();
        prestige = transform.Find("Container/Project/Prestige/Rating_Container/Prestige_Counter").GetComponent<TextMeshProUGUI>();
        projectLocation = transform.Find("Container/Title_Bar/Project_A/Location").GetComponent<TextMeshProUGUI>();
        admContributionLabel = transform.Find("Container/Project/They_must_Contribute/ADM_Basic_Container/Counter_ADM_Basic").GetComponent<TextMeshProUGUI>();
        moneyContributionLabel = transform.Find("Container/Project/They_must_Contribute/Money_Basic_Container/Counter_Money_Basic").GetComponent<TextMeshProUGUI>();
        turnEstimateLabel = transform.Find("Container/Project/Est_Time/Turns_Basic_Container/Counter_Turns_Basic").GetComponent<TextMeshProUGUI>();
        turnEstimateUnitLabel = transform.Find("Container/Project/Est_Time/Turns_Basic_Container/Counter_Turns_Label").GetComponent<TextMeshProUGUI>();
        cancelButton = transform.Find("Container/Project/BTN_CANCEL").GetComponent<Button>();

        // delegates
        cancelButton.onClick.AddListener(delegate { ExitScreen(); }); // button is clicked, so activate the Project Screen
    }

    void Start()
    {
        activeProject = gDataRef.ProjectDataList.Find(p => p.ID == ProjectID); // assigns the working project
        if (activeProject == null)
            Destroy(gameObject); // don't create 
        else
            InitializeScreen();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (turnEstimate == 0)
        {
            turnEstimateLabel.text = "";
            turnEstimateUnitLabel.text = "Never";
        }
	}

    void CalculateTurnsToComplete()
    {
        if (currentADMPledged == 0)
        {
            turnEstimate = 0;
        }
    }

    void CalculateTotalADMContribution()
    {

    }

    void CalculateTotalMoneyContribution()
    {

    }

    void ExitScreen()
    {
        uiManagerRef.ModalIsActive = false; // reset modal set
        Destroy(gameObject);
    }

    void InitializeScreen()
    {
        if (LocationID.StartsWith("STA"))
            projLocation = HelperFunctions.DataRetrivalFunctions.GetSystem(LocationID).Name;
        else if (LocationID.StartsWith("PRO"))
            projLocation = HelperFunctions.DataRetrivalFunctions.GetProvince(LocationID).Name;
        else
            projLocation = HelperFunctions.DataRetrivalFunctions.GetPlanet(LocationID).Name;

        projectName.text = activeProject.Name + ": " + projLocation; // test
        projectLocation.text = projLocation;
        energyRequired.text = activeProject.BaseEnergyReq.ToString("N0");
        basicRequired.text = activeProject.BaseBasicReq.ToString("N0");
        heavyRequired.text = activeProject.BaseHeavyReq.ToString("N0");
        rareRequired.text = activeProject.BaseRareReq.ToString("N0");
        ADMRequired.text = activeProject.BaseADMReq.ToString("N0");
        prestige.text = activeProject.BasePrestige.ToString("N0");
        admContributionLabel.text = currentADMPledged.ToString("N0");
        moneyContributionLabel.text = currentMoneyPercentagePledged.ToString("N0") + "%";
        

    }

    public void OnPointerClick(PointerEventData pEvent)
    {
        if (pEvent.button == PointerEventData.InputButton.Right)
        {
            uiManagerRef.ModalIsActive = false; // reset modal set
            Destroy(gameObject);
        }
            
    }
}
