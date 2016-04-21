using UnityEngine;
using Assets.Scripts.Interfaces;
using Managers;

namespace Assets.Scripts.States
{
    public class SetupState : IStateBase
    {
        private StateManager manager;
        private GalaxyData gData;
        private GameData gameData;
        private TurnEngine tEngineData;
        private UIManager uiManagerRef;
        public GameObject[] starObjects;
        private bool initialTurnGenerationProcessing = false;
        bool turnInitialized = false;

        public SetupState(StateManager managerRef)
        {
            manager = managerRef;
            gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            gameData = GameObject.Find("GameManager").GetComponent<GameData>();
            tEngineData = GameObject.Find("GameManager").GetComponent<TurnEngine>();
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();

            Debug.Log("In SetupState");        
        }

        public void Update()
        {
            StateUpdate();          
        }

        public void StateUpdate()
        {
            

            if (gData.galaxyIsGenerated && gameData.CivsGenerated && !initialTurnGenerationProcessing) // && tEngineData.GameGenerationComplete) // if all parameters are ready to load
            {
                initialTurnGenerationProcessing = true;               
            }

            if (initialTurnGenerationProcessing)
            {
                if (!turnInitialized)
                {
                    tEngineData.InitializeFirstTurn();
                    turnInitialized = true;
                }
               
                if (tEngineData.GameGenerationComplete)
                {
                    Application.LoadLevel("GalaxyMap");
                    manager.SwitchState(new GalaxyMapState(manager));  // and change the state manager to the beginning state
                    initialTurnGenerationProcessing = false;
                }
            }
        }

        public void ShowIt()
        {
            // draw stats
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "build: " + manager.gameDataRef.gameVersion);
        }

        public void StateFixedUpdate()
        {

        }

        
       
    }
}