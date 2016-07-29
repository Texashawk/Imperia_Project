using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using CharacterObjects;
using Managers;


public class CharacterScreenActivation : MonoBehaviour, IPointerClickHandler
{
    GameData gDataRef;
    UIManager uiManagerRef;
    CharacterScreen cScreen;
    Character cData;
   

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        cScreen = GameObject.Find("Character Window Canvas").GetComponent<CharacterScreen>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            ActivateWindow();
    }

    public void ActivateWindow()
    {
        //gDataRef.CharacterWindowActive = true;
        gDataRef.SelectedCharacter = cData;
        uiManagerRef.ModalIsActive = true;
        uiManagerRef.ActivateCharacterScreen(cData.ID);
        //cScreen.CharacterDataLoaded = false;
        //cScreen.InitializeData(cData);
    }

    public void InitializeData(Character characterData)
    {
        cData = characterData;
    }
   
}
