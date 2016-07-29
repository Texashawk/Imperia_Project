using UnityEngine;
using UnityEngine.UI;
using Actions;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;


public class ActionButton : MonoBehaviour, IPointerClickHandler
{
    private string buttonID;
    private CharacterAction.eType actionType;
    private string characterID;
    private GraphicAssets gAssets;
    public TextMeshProUGUI ButtonText;
    public Image background;
    public Image ButtonImage;
    public NewCharacterScreen NewCharacterScreen;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }

    public void SetName(string name)
    {      
        ButtonText.text = name;
    }

    public void SetID(string ID)
    {
        buttonID = ID;        
    }

    public void SetType(CharacterAction.eType type)
    {
        actionType = type;
        switch (type)
        {
            case CharacterAction.eType.Assignment:
                background.color = new Color(.91f, .65f, 0, .4f);
                break;
            case CharacterAction.eType.Personal:
                background.color = new Color(.09f, .53f, .19f, .4f);
                break;
            case CharacterAction.eType.Psychic:
                background.color = Color.cyan;
                break;
            case CharacterAction.eType.Hostile:
                background.color = new Color(.9f, 0, 0, .5f);
                break;
            case CharacterAction.eType.IntelOps:
                background.color = Color.cyan;
                break;
            default:
                background.color = Color.white;
                break;
        }
    }

    public void SetPicture(string picID)
    {
        ButtonImage.sprite = gAssets.CharacterList.Find(p => p.name == picID);
    }

    public void OnPointerClick(PointerEventData pData)
    {
        NewCharacterScreen.ActionButtonClicked(buttonID);
    }
}
