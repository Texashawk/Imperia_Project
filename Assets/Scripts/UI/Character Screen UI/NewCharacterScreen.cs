using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Managers;
using HelperFunctions;
using CharacterObjects;
using ConversationAI;
using TMPro;

public class NewCharacterScreen : MonoBehaviour, IPointerClickHandler {

    public enum eActionPanelMode : int
    {
        Assignment,
        Personal,
        Psychic,
        Hostile,
        IntelOps,
        None
    }

    // external data references
    UIManager uiManagerRef;
    GameData gDataRef;
    GraphicAssets gAssetRef;
    Character cData;

    // public GameObject links
    public GameObject CharacterName;
    public GameObject CharacterPower;
    public GameObject CharacterAdminSkill;
    public GameObject CharacterLove;
    public GameObject CharacterFear;
    public GameObject CharacterWealth;
    public GameObject CharacterImage;
    public GameObject CharacterRelationship;
    public GameObject CharacterRank;
    public GameObject CharacterADM;
    public GameObject AORText;
    public GameObject AORLocationText;
    public GameObject CharacterIntelText;
    public GameObject Caution;
    public GameObject Passion;
    public GameObject Drive;
    public GameObject Piety;
    public GameObject Humanity;
    public GameObject Honor;
    public GameObject Charm;
    public GameObject Intelligence;
    public GameObject Discretion;
    public GameObject ChatWindow;

    // buttons
    public GameObject RelationshipBuildingActionButton;
    public GameObject UsingCharacterActionButton;
    public GameObject HostileActionButton;

    // panels
    public GameObject ActionPanel;
    public GameObject SubActionPanel;

    // indicators
    public GameObject RelationActionIndicator;
    public GameObject UsingCharacterActionIndicator;
    public GameObject HostileActionActionIndicator;
    public GameObject ActionButtonList;

    // private GUI variables
    TextMeshProUGUI characterName;
    TextMeshProUGUI characterPower;
    TextMeshProUGUI characterAdmin;
    TextMeshProUGUI characterLove;
    TextMeshProUGUI characterFear;
    TextMeshProUGUI characterWealth;
    TextMeshProUGUI characterADM;
    TextMeshProUGUI aorText;
    TextMeshProUGUI aorLocationText;
    TextMeshProUGUI characterIntelText;
    TextMeshProUGUI caution;
    TextMeshProUGUI piety;
    TextMeshProUGUI passion;
    TextMeshProUGUI drive;
    TextMeshProUGUI humanity;
    TextMeshProUGUI honor;
    TextMeshProUGUI charm;
    TextMeshProUGUI intelligence;
    TextMeshProUGUI discretion;
    TextMeshProUGUI chatWindow;
    Button relationshipBuildingActionButton;
    Button usingCharacterActionButton;
    Button hostileActionButton;
    Image characterImage;
    Image characterRelationship;
    Image characterRank;
    ActionScrollView actionButtonList;

    // constants
    const int ActiveActionButtonWidth = 320;
    const int InactiveActionButtonWidth = 310;

    // internal vars
    eActionPanelMode actionPanelMode = eActionPanelMode.None; // set the initial panel mode to nothing (since closed)
    bool subModeActive = false;
    

	void Awake ()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        gAssetRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        characterName = CharacterName.GetComponent<TextMeshProUGUI>();
        characterPower = CharacterPower.GetComponent<TextMeshProUGUI>();
        characterAdmin = CharacterAdminSkill.GetComponent<TextMeshProUGUI>();
        characterLove = CharacterLove.GetComponent<TextMeshProUGUI>();
        characterFear = CharacterFear.GetComponent<TextMeshProUGUI>();
        characterADM = CharacterADM.GetComponent<TextMeshProUGUI>();
        characterWealth = CharacterWealth.GetComponent<TextMeshProUGUI>();
        characterIntelText = CharacterIntelText.GetComponent<TextMeshProUGUI>();
        aorText = AORText.GetComponent<TextMeshProUGUI>();
        aorLocationText = AORLocationText.GetComponent<TextMeshProUGUI>();
        caution = Caution.GetComponent<TextMeshProUGUI>();
        piety = Piety.GetComponent<TextMeshProUGUI>();
        passion = Passion.GetComponent<TextMeshProUGUI>();
        drive = Drive.GetComponent<TextMeshProUGUI>();
        humanity = Humanity.GetComponent<TextMeshProUGUI>();
        honor = Honor.GetComponent<TextMeshProUGUI>();
        charm = Charm.GetComponent<TextMeshProUGUI>();
        intelligence = Intelligence.GetComponent<TextMeshProUGUI>();
        discretion = Discretion.GetComponent<TextMeshProUGUI>();
        characterImage = CharacterImage.GetComponent<Image>();
        characterRelationship = CharacterRelationship.GetComponent<Image>();
        characterRank = CharacterRank.GetComponent<Image>();
        chatWindow = ChatWindow.GetComponent<TextMeshProUGUI>();
        relationshipBuildingActionButton = RelationshipBuildingActionButton.GetComponent<Button>();
        usingCharacterActionButton = UsingCharacterActionButton.GetComponent<Button>();
        hostileActionButton = HostileActionButton.GetComponent<Button>();

        ActionPanel.SetActive(false); // initially turn off the action panel
        RelationActionIndicator.SetActive(false);
        UsingCharacterActionIndicator.SetActive(false);
        HostileActionActionIndicator.SetActive(false);

        // panels
        actionButtonList = ActionButtonList.GetComponent<ActionScrollView>();

        // button delegates
        relationshipBuildingActionButton.onClick.AddListener(delegate { ToggleActionPanel("relationshipsAction"); }); // button is clicked, so activate the Project Screen
        usingCharacterActionButton.onClick.AddListener(delegate { ToggleActionPanel("usingCharacterAction"); }); // button is clicked, so activate the Project Screen
        hostileActionButton.onClick.AddListener(delegate { ToggleActionPanel("hostileAction"); }); // sort buttons
    }

    void Start()
    {
        SetAllActionButtonsInactive();
        UpdateCharacterScreen();
    }

	void Update ()
    {
	    if (gDataRef.SelectedCharacter != cData)
        {
            UpdateCharacterScreen();
        }

        if (cData.Civ.Leader.ActionPoints < Constants.Constant.APsRequiredForAction)
        {
            ActionPanel.SetActive(false);
            SetAllActionButtonsInactive();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -2) // for right mouse click
        {
            if (!subModeActive)
            {
                Destroy(gameObject);
                uiManagerRef.ModalIsActive = false;
            }
            else
            {
                SubActionPanel.SetActive(false);
                subModeActive = false;
            }
        }
    }

    public void ActionButtonClicked(string buttonID)
    {
        cData.Civ.Leader.ActionPoints -= 1; // when clicked, you initiate the action and lose the points
        SubActionPanel.SetActive(true);
        SubActionPanel.GetComponent<SubActionPanel>().InitializePanel(buttonID);
        subModeActive = true;

    }

    void ToggleActionPanel(string actionType)
    {
        

        if (actionPanelMode == eActionPanelMode.None)
            ActionPanel.SetActive(true);

        switch (actionType)
        {
            case "relationshipsAction":
                if (actionPanelMode == eActionPanelMode.Personal)
                {
                    RelationActionIndicator.SetActive(false);
                    SetAllActionButtonsInactive();
                    ActionPanel.SetActive(false);                  
                    actionPanelMode = eActionPanelMode.None;
                }
                else
                {
                    ActionPanel.SetActive(true);
                    RelationActionIndicator.SetActive(true);
                    UsingCharacterActionIndicator.SetActive(false);
                    HostileActionActionIndicator.SetActive(false);
                    SetAllActionButtonsInactive();
                    RelationshipBuildingActionButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ActiveActionButtonWidth);
                    actionPanelMode = eActionPanelMode.Personal;
                    actionButtonList.InitializeList(cData, actionPanelMode);
                }
                break;
            case "usingCharacterAction":
                if (actionPanelMode == eActionPanelMode.Assignment)
                {
                    UsingCharacterActionIndicator.SetActive(false);
                    SetAllActionButtonsInactive();
                    ActionPanel.SetActive(false);
                    actionPanelMode = eActionPanelMode.None;
                }
                else
                {
                    ActionPanel.SetActive(true);
                    UsingCharacterActionIndicator.SetActive(true);
                    HostileActionActionIndicator.SetActive(false);
                    RelationActionIndicator.SetActive(false);
                    SetAllActionButtonsInactive();
                    UsingCharacterActionButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ActiveActionButtonWidth);
                    actionPanelMode = eActionPanelMode.Assignment;
                    actionButtonList.InitializeList(cData, actionPanelMode);
                }
                break;
            case "hostileAction":
                if (actionPanelMode == eActionPanelMode.Hostile)
                {
                    HostileActionActionIndicator.SetActive(false);
                    SetAllActionButtonsInactive();
                    ActionPanel.SetActive(false);
                    actionPanelMode = eActionPanelMode.None;
                }
                else
                {
                    ActionPanel.SetActive(true);
                    HostileActionActionIndicator.SetActive(true);
                    UsingCharacterActionIndicator.SetActive(false);
                    RelationActionIndicator.SetActive(false);
                    SetAllActionButtonsInactive();
                    HostileActionButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ActiveActionButtonWidth);
                    actionPanelMode = eActionPanelMode.Hostile;
                    actionButtonList.InitializeList(cData, actionPanelMode);
                }
                break;

            default:
                break;
        }
    }

    void SetAllActionButtonsInactive()
    {
        RelationshipBuildingActionButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, InactiveActionButtonWidth);
        UsingCharacterActionButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, InactiveActionButtonWidth);
        HostileActionButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, InactiveActionButtonWidth);
    }

    void UpdateAORLocation()
    {
        cData = gDataRef.SelectedCharacter;
        string aorPrimaryLocation = "";
        string aorSecondaryLocation = "";
        string aorTertiaryLocation = "";

        if (cData.Role == Character.eRole.Viceroy)
        {
            aorPrimaryLocation = cData.PlanetAssigned.Name;
            aorSecondaryLocation = cData.PlanetAssigned.System.Name + " | ";
            aorTertiaryLocation = cData.PlanetAssigned.System.Province.Name;
        }

        else if (cData.Role == Character.eRole.SystemGovernor)
        {
            aorPrimaryLocation = cData.SystemAssigned.Name + " System";
            aorSecondaryLocation = cData.SystemAssigned.Province.Name + " | ";
            aorTertiaryLocation = cData.Civ.Name;
        }

        else if (cData.Role == Character.eRole.ProvinceGovernor)
        {
            aorPrimaryLocation = cData.ProvinceAssigned.Name + " Province";
            aorSecondaryLocation = cData.Civ.Name;
            aorTertiaryLocation = "";
        }

        else
        {
            aorPrimaryLocation = "NO";
            aorSecondaryLocation = "Current Assignment";
        }

        aorText.text = aorPrimaryLocation;
        aorLocationText.text = aorSecondaryLocation + aorTertiaryLocation;
    }

    void UpdateCharacterBaseData()
    {
        cData = gDataRef.SelectedCharacter;
        string HouseInfo = " of the ";
        if (cData.AssignedHouse.Rank == House.eHouseRank.Great)
            HouseInfo += "Great House of ";

        else if (cData.AssignedHouse.Rank == House.eHouseRank.Minor)
            HouseInfo += "Minor House of ";

        else
            HouseInfo += "common house of ";
        characterImage.sprite = gAssetRef.CharacterList.Find(p => p.name == cData.PictureID);
        switch (cData.Role)
        {
            case Character.eRole.Emperor:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_Emperor");
                break;
            case Character.eRole.Leader:
                characterRank.enabled = false;
                break;
            case Character.eRole.Viceroy:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_Viceroy");
                break;
            case Character.eRole.ProvinceGovernor:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_PR_Governor");
                break;
            case Character.eRole.SystemGovernor:
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_SY_Governor");
                break;
            case Character.eRole.Admiral:
                characterRank.enabled = false;
                break;
            case Character.eRole.General:
                characterRank.enabled = false;
                break;
            case Character.eRole.SciencePrime:
                characterRank.enabled = false;
                break;
            case Character.eRole.FinancePrime:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_Eco_Prime");
                break;
            case Character.eRole.WarPrime:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_War_Prime");
                break;
            case Character.eRole.DomesticPrime:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_Pop_Prime");
                break;
            case Character.eRole.IntelPrime:
                characterRank.enabled = true;
                characterRank.sprite = gAssetRef.PlanetRankList.Find(p => p.name == "Icon_Rank_Politics_Prime");
                break;
            case Character.eRole.Inquisitor:
                characterRank.enabled = false;
                break;
            case Character.eRole.Pool:
                characterRank.enabled = false;
                break;
            default:
                characterRank.enabled = false;
                break;
        }

        UpdateAORLocation();
        characterRelationship.sprite = gAssetRef.RelationshipIconList.Find(p => p.name == cData.Relationships[cData.Civ.LeaderID].RelationshipIcon);
        HouseInfo += cData.AssignedHouse.Name;
        characterName.text = cData.Name + HouseInfo;
        characterPower.text = cData.Power.ToString("N0");
        characterAdmin.text = cData.Administration.ToString("N0");
        characterWealth.text = "$" + cData.Wealth.ToString("N0") + "bn";
        characterLove.text = cData.AssignedLove.ToString("P0");
        //characterFear.text = cData.AssignedFear.ToString("P0");
        characterIntelText.text = StringConversions.ConvertIntelValueToDescription(cData.IntelLevel);

        // character traits
        caution.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Caution, cData.IntelLevel);
        caution.color = StringConversions.GetTextValueColor((int)cData.Caution);
        passion.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Passion, cData.IntelLevel);
        passion.color = StringConversions.GetTextValueColor((int)cData.Passion);
        drive.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Drive, cData.IntelLevel);
        drive.color = StringConversions.GetTextValueColor((int)cData.Drive);
        piety.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Piety, cData.IntelLevel);
        piety.color = StringConversions.GetTextValueColor((int)cData.Piety);
        humanity.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Humanity, cData.IntelLevel);
        humanity.color = StringConversions.GetTextValueColor((int)cData.Humanity);
        honor.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Honor, cData.IntelLevel);
        honor.color = StringConversions.GetTextValueColor((int)cData.Honor);
        charm.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Charm, cData.IntelLevel);
        charm.color = StringConversions.GetTextValueColor((int)cData.Charm);
        intelligence.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Intelligence, cData.IntelLevel);
        intelligence.color = StringConversions.GetTextValueColor((int)cData.Intelligence);
        discretion.text = StringConversions.ConvertCharacterValueToDescription((int)cData.Discretion, cData.IntelLevel);
        discretion.color = StringConversions.GetTextValueColor((int)cData.Discretion);
    }

    void UpdateCharacterScreen()
    {

        UpdateCharacterBaseData();

        // chat window (will change)
        string introText = ConversationEngine.GenerateInitialDialogue(cData);
        chatWindow.text = cData.Name + "_" + "\n" + introText;
    }
}
