﻿using UnityEngine;
using Assets.Scripts.Interfaces;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.States
{
    public class BeginState : IStateBase
    {
        private StateManager manager;
        private Button newGameButton;
        private PointerEventData p;

        public BeginState(StateManager managerRef)   //constructor
        {
            manager = managerRef;
            Debug.Log("Constructing NewGame");
            newGameButton = GameObject.Find("New Game Button").GetComponent<Button>();
            newGameButton.onClick.AddListener(delegate { OnClick(); });
        }

        public void StateUpdate()
        {
            
        }

        public void ShowIt()
        {
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "build: " + manager.gameDataRef.gameVersion); // legacy code

            //if (GUI.Button(new Rect((Screen.width / 2) - (150 / 2), Screen.height - 200, 150, 100), "New Game Test"))
            //    Switch();
           
        }

        void OnClick()
        {
            Switch();
        }

        public void StateFixedUpdate()
        {

        }

        public void Switch()
        {
            Debug.Log("Setting up the galaxy");
            Application.LoadLevelAsync("NewGameScreen");
            manager.SwitchState(new NewGameState(manager));
        }
    }
}
