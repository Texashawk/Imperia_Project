using UnityEngine;
using HelperFunctions;
using Projects;
using CharacterObjects;
using UnityEngine.UI;
using TMPro;
using Tooltips;

public class CharacterCard : MonoBehaviour {

    private Image portrait;
    private Image house;
    private Image relationship;
    private TextMeshProUGUI charName;
    private TextMeshProUGUI ADM;
    public string CharID;
    private TextMeshProUGUI adminSkill;
    private TextMeshProUGUI funding;
    private GraphicAssets gAssetData;
    public bool isContributor = false;
    public bool isAdminstrator = false;
    

    void Awake()
    {
        portrait = transform.Find("Portrait_Container/Portrait").GetComponent<Image>();
        house = transform.Find("Portrait_Container/Icon_House").GetComponent<Image>();
        relationship = transform.Find("Portrait_Container/Icon_Relationship").GetComponent<Image>();
        charName = transform.Find("Name").GetComponent<TextMeshProUGUI>();
        ADM = transform.Find("Stats_Container/Counter_ADM").GetComponent<TextMeshProUGUI>();
        adminSkill = transform.Find("Stats_Container/Counter_ADS").GetComponent<TextMeshProUGUI>();
        funding = transform.Find("Stats_Container/Funding_Container/Counter_Funding").GetComponent<TextMeshProUGUI>();
        gAssetData = GameObject.Find("GameManager").GetComponent<GraphicAssets>();

    }

	// Use this for initialization
	void Start ()
    {
	
	}
	
	public void InitializeCard(Character cData, Project pData)
    {
        CharID = cData.ID;
        charName.text = cData.Name;
        adminSkill.text = cData.Administration.ToString("N0");
        ADM.text = DataRetrivalFunctions.DetermineAdminAvailable(cData).ToString("N0");
        portrait.sprite = gAssetData.CharacterList.Find(p => p.name == cData.PictureID);
        funding.text = DataRetrivalFunctions.DetermineContributionToProject(cData, pData).ToString("P0");
        transform.GetComponent<CharacterTooltip>().InitializeTooltipData(cData, -13f); // set up the tooltip
        transform.GetComponent<CharacterScreenActivation>().InitializeData(cData); // set up the window
    }

    public void UpdateCard(string cID, Project pData)
    {
        Character cData = DataRetrivalFunctions.GetCharacter(cID);
        ADM.text = DataRetrivalFunctions.DetermineAdminAvailable(cData).ToString("N0");    
        funding.text = DataRetrivalFunctions.DetermineContributionToProject(cData, pData).ToString("P0");
    }
}
