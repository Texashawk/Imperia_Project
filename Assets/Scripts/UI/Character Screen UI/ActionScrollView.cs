﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CharacterObjects;
using EconomicObjects;
using Actions;

public class ActionScrollView : MonoBehaviour
{

    public GameObject Button_Template;
    //private List<Character> ActionList = new List<Character>();
    private GameData gameDataRef;
    private List<GameObject> buttonList = new List<GameObject>();
    public bool listIsInitialized = false;
    private Character activeChar;
    private NewCharacterScreen.eActionPanelMode activeMode;

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
    }
    
    public void InitializeList(Character cData, NewCharacterScreen.eActionPanelMode mode)
    {
        activeChar = cData;
        listIsInitialized = false; 
        if (!listIsInitialized)
        {
            ClearList(); // clear out the list
            if (activeChar.Civ.Leader.ActionPoints > 0)
            {
                List<CharacterAction> actionList = new List<CharacterAction>();
                foreach (CharacterAction action in gameDataRef.CharacterActionList)
                {
                    if (action.IsActionValid(activeChar, cData.Civ)) // don't populate yourself!
                    {
                        if (action.Category.ToString() == mode.ToString())
                            AddButton(action, activeChar);
                    }
                }
            }
            listIsInitialized = true;
            //ActionList.Clear();
        }
    }

    private void AddButton(CharacterAction action, Character cData)
    {
        GameObject go = Instantiate(Button_Template) as GameObject;
        go.SetActive(true);
        go.name = action.ID;
        ActionButton TB = go.GetComponent<ActionButton>();
        TB.SetName(action.Name.ToUpper());
        TB.SetID(action.ID);
        TB.SetType(action.Category); // sets the type and the color
        //TB.SetCharacter(activeChar.ID); // attaches button to character
        //TB.SetPicture(cha.PictureID);
        go.transform.SetParent(Button_Template.transform.parent);
        go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        buttonList.Add(go);
    }

    public void ClearList()
    {
        foreach (GameObject go in buttonList)
        {
            Destroy(go);
        }
        listIsInitialized = false;
    }

    public void ButtonClicked(string str, Character cData)
    {
        Debug.Log(str + " button clicked, action is on character " + activeChar.Name);
        if (activeChar.Civ.Leader != null)
        {
            if (activeChar.Civ.Leader.ActionPoints > 0)
                activeChar.Civ.Leader.ActionPoints -= 1;

            if (activeChar.Civ.Leader.ActionPoints == 0)
                ClearList(); // if no more APs, remove all buttons
        }
        else
            return;
     
        // Action switch board
        ExecuteAction(str, cData);
        listIsInitialized = false;
    }

    private void ExecuteAction(string str, Character cData)
    {
        string response = "";

        // switch statement for all the actions
        switch (str.ToUpper())
        {
            case "A1":
                response = ActionFunctions.GivePraisingSpeech(cData);
                break;
            case "A2" :
                response = ActionFunctions.GivePublicReprimand(activeChar.Civ.Leader, cData);
                break;
            case "A3":
                response = ActionFunctions.IssueInsultToCharacter(activeChar.Civ.Leader, cData);
                break;
            case "A6":
                response = ActionFunctions.OrderToChangeExport(activeChar, activeChar.Civ.Leader, Trade.eTradeGood.Basic, Trade.eTradeGood.Energy); // will need to rewrite to add 'sub action panel'
                break;
            case "A7":
                response = ActionFunctions.IssueInsultToCharacter(activeChar.Civ.Leader, cData);
                break;
            default:
                break;
        }
    }
} 

