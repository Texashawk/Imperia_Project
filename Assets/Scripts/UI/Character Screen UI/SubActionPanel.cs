using UnityEngine;
using Actions;
using CharacterObjects;
using UnityEngine.UI;
using TMPro;

public class SubActionPanel : MonoBehaviour {

    // references
    private GameData gameDataRef;
    private GraphicAssets gAssetRef;
    private CharacterAction cAction = new CharacterAction();

    // public GameObjects
    public GameObject ActionTitle;
    public GameObject EmperorImage;
    public GameObject CharacterImage;

    // private UI references
    private TextMeshProUGUI actionTitle;
    private Image emperor;
    private Image character;
    private Character cData;

    // internal status variables
    private bool actionPanelDraw = false;

    void Awake ()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        gAssetRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        emperor = EmperorImage.GetComponent<Image>();
        character = CharacterImage.GetComponent<Image>();
        actionTitle = ActionTitle.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        //DrawPanel();      
    }
	
	void Update ()
    {
	    if (actionPanelDraw)
        {
            DrawPanel();
            actionPanelDraw = false;
        }
	}

    public void InitializePanel(string actionID)
    {
        cAction = gameDataRef.CharacterActionList.Find(p => p.ID == actionID);
        actionPanelDraw = true;
        cData = gameDataRef.SelectedCharacter;
    }

    private void DrawPanel()
    {
        actionTitle.text = cAction.Name;
        emperor.sprite = gAssetRef.CharacterList.Find(p => p.name == cData.Civ.Leader.PictureID);
        character.sprite = gAssetRef.CharacterList.Find(p => p.name == cData.PictureID);
    }
}
