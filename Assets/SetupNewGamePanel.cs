using UnityEngine;
using Constants;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SetupNewGamePanel : MonoBehaviour
{

    private Button exitButton;
    private Button nextStepButton;
    private StateManager stateRef;
    private GameSetupFile setupFile;
    private TextMeshProUGUI empireSizeNumber;
    private ToggleGroup quadrantSize;
    private Slider empireSize;
    private InputField empireName;
    private GameData gDataRef;
    private IEnumerable<Toggle> activeToggle; 

    // Use this for initialization
    void Start ()
    {
        exitButton = transform.Find("Exit Button").GetComponent<Button>();
        stateRef = GameObject.Find("GameManager").GetComponent<StateManager>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();     
        nextStepButton = GameObject.Find("Move to Next Step Button").GetComponent<Button>();       
        quadrantSize = transform.Find("Game Size Sub Panel/Quadrant Toggle Group").GetComponent<ToggleGroup>();
        empireName = transform.Find("Empire Information Sub Panel/Name Input/InputField").GetComponent<InputField>();
        empireSize = transform.Find("Empire Information Sub Panel/Empire Size/Slider").GetComponent<Slider>();      
        empireSize.wholeNumbers = true;
        empireSizeNumber = transform.Find("Empire Information Sub Panel/Empire Size/Slider/Size").GetComponent<TextMeshProUGUI>();
        setupFile = new GameSetupFile();

        // add delegates for events
        exitButton.onClick.AddListener(delegate { ExitGame(); }); // initiate new game setup screen
        nextStepButton.onClick.AddListener(delegate { MoveToNextStep(); }); // move to the galaxy generation sequence
        empireSize.onValueChanged.AddListener(delegate { UpdateSliderValue(); }); // update the slider value when it changes
    }

    void OnGUI()
    {
        activeToggle = quadrantSize.ActiveToggles();

        foreach (Toggle tog in activeToggle)
        {
            if (tog.isOn)
            {
                if (tog.name == "Small Quadrant Select")
                {
                    setupFile.QuadrantSizeSelect = GameSetupFile.eQuadrantSize.Small;
                    empireSize.minValue = Constant.MinEmpireSizeInSmallQuadrant;
                    empireSize.maxValue = Constant.MaxEmpireSizeInSmallQuadrant;
                }

                if (tog.name == "Medium Quadrant Select")
                {
                    setupFile.QuadrantSizeSelect = GameSetupFile.eQuadrantSize.Medium;
                    empireSize.minValue = Constant.MinEmpireSizeInMediumQuadrant;
                    empireSize.maxValue = Constant.MaxEmpireSizeInMediumQuadrant;
                }

                if (tog.name == "Large Quadrant Select")
                {
                    setupFile.QuadrantSizeSelect = GameSetupFile.eQuadrantSize.Large;
                    empireSize.minValue = Constant.MinEmpireSizeInLargeQuadrant;
                    empireSize.maxValue = Constant.MaxEmpireSizeInLargeQuadrant;
                }
            }
        }           
    }

    void UpdateSliderValue()
    {
        empireSizeNumber.text = empireSize.value.ToString("N0");
    }

    void MoveToNextStep()
    {
        setupFile.EmpireWidth = (int)(empireSize.value / 2f);
        setupFile.EmpireName = empireName.text;
        gDataRef.GameSetup = setupFile;
        stateRef.activeState.Switch(); // the active state should be BeginState
    }

    void ExitGame()
    {
        Application.Quit();
    }
}
