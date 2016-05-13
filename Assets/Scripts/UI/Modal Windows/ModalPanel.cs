using UnityEngine;
using UnityEngine.UI;
using Managers;
using UnityEngine.Events;

public class ModalPanel : MonoBehaviour {

    public Text questionString;
    public Image iconImage;
    public Button yesButton;
    public Button noButton;
    public Button cancelButton;
    public GameObject modalPanelObject;
    private static GameData gGameData;
    private static UIManager uiManagerRef;

    private static ModalPanel modalPanel;

    public static ModalPanel Instance()
    {
        if (!modalPanel)
        {
            gGameData = GameObject.Find("GameManager").GetComponent<GameData>();
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
            modalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if (!modalPanel)
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
        }     
        return modalPanel;
    }

    // Yes/No/Cancel: A string, a yes event, a no event and a cancel event
    public void Choice (string question, UnityAction yesEvent, UnityAction noEvent, UnityAction cancelEvent)
    {
        modalPanelObject.SetActive(true); // open the panel
        uiManagerRef.ModalIsActive = true;

        // set up the event calls for each button
        yesButton.onClick.RemoveAllListeners(); // remove all listeners from the event window
        yesButton.onClick.AddListener(yesEvent);
        yesButton.onClick.AddListener(ClosePanel);

        noButton.onClick.RemoveAllListeners(); // remove all listeners from the event window
        noButton.onClick.AddListener(noEvent);
        noButton.onClick.AddListener(ClosePanel);

        cancelButton.onClick.RemoveAllListeners(); // remove all listeners from the event window
        cancelButton.onClick.AddListener(cancelEvent);
        cancelButton.onClick.AddListener(ClosePanel);

        this.questionString.text = question; // set the text string to the question

        this.iconImage.gameObject.SetActive(true);

        // turn on the buttons
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
    }

    void ClosePanel()
    {
        uiManagerRef.ModalIsActive = false;
        modalPanelObject.SetActive(false); // sets the panel to false
    }
}
