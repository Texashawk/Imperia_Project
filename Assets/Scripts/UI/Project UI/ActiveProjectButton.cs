using UnityEngine;
using UnityEngine.UI;
using Managers;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

class ActiveProjectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string buttonID;
    private string iconName;
    private GraphicAssets gAssets;
    private TextMeshProUGUI ButtonText;
    private Image ButtonImage;
    private GameObject hoverContent;
    private TextMeshProUGUI hoverText;
    private UIManager uiManagerRef;
    private GridLayoutGroup buttonGrid;
    private Image hoverMaxObject;
    private Button thisButton;
    private float currentButtonWidth = 0f;
    private float maxButtonWidth = 420f;
    private float minButtonWidth = 0f;
    public bool IsHovering = false;
    public bool IsExpanded = false;

    private const float ExpandRate = 30f;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        thisButton = gameObject.GetComponent<Button>();
        hoverContent = GameObject.Find("Active_Hover_Content");
        hoverMaxObject = transform.Find("Active_Hover_Content/Hover_Maximize").GetComponent<Image>();
        hoverText = transform.Find("Active_Hover_Content/Hover_Text_Status").GetComponent<TextMeshProUGUI>();
        ButtonImage = transform.Find("BTN_Small_Create").GetComponent<Image>();
        ButtonText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        hoverContent.SetActive(false);
        thisButton.onClick.AddListener(delegate { ButtonClicked(buttonID); }); // button is clicked, so activate the Project Screen               
    }

    public void Start()
    {
        buttonGrid = GameObject.Find("Active_Project_Container_Grid").GetComponent<GridLayoutGroup>();
        minButtonWidth = buttonGrid.cellSize.x;

    }

    void Update()
    {
        if (uiManagerRef.ModalIsActive)
            gameObject.GetComponent<Button>().interactable = false;
        else
            gameObject.GetComponent<Button>().interactable = true;

        if (!IsHovering)
            hoverContent.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eData)
    {
        IsHovering = true;
        if (!uiManagerRef.ModalIsActive)
            ButtonExpand();
    }

    public void OnPointerExit(PointerEventData eData)
    {
        IsHovering = false;
        ButtonRetract();
    }

    public void SetName(string name)
    {
        ButtonText.text = name;
    }

    public void SetID(string ID)
    {
        buttonID = ID;
    }

    public void SetDescription(string desc)
    {
        hoverText.text = desc;
    }

    public void SetIcon(string Name)
    {
        iconName = Name;
        if (gAssets.ProjectIconList.Find(p => p.name.ToUpper() == iconName.ToUpper()))
            ButtonImage.sprite = gAssets.ProjectIconList.Find(p => p.name.ToUpper() == iconName.ToUpper());
        else
            ButtonImage.sprite = gAssets.ProjectIconList.Find(p => p.name.ToUpper() == "RENAMEPLANET");
    }

    private void ButtonClicked(string projectID)
    {
        //string locationID = "";

        //if (uiManagerRef.selectedPlanet != null)
        //    locationID = uiManagerRef.selectedPlanet.ID;
        //else if (uiManagerRef.selectedSystem != null)
        //    locationID = uiManagerRef.selectedSystem.ID;

        //IsExpanded = false; // complete the expansion so ended
        //IsHovering = false;
        //gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minButtonWidth); // immediately retract button
        //uiManagerRef.ActivateProjectScreen(projectID, locationID); // launch the project screen
    }

    private IEnumerator ReduceButtonWidth()
    {
        while (currentButtonWidth > minButtonWidth && !IsHovering)
        {
            currentButtonWidth -= ExpandRate;
            if (currentButtonWidth < minButtonWidth)
                currentButtonWidth = minButtonWidth;
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentButtonWidth); // will change to constant later;
            yield return null;
        }

        IsExpanded = false; // complete the expansion so ended
    }

    private IEnumerator ExpandButtonWidth()
    {
        while (currentButtonWidth < maxButtonWidth && IsHovering)
        {
            currentButtonWidth += ExpandRate;
            if (currentButtonWidth > maxButtonWidth)
                currentButtonWidth = maxButtonWidth;
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentButtonWidth); // will change to constant later;
            yield return null;
        }
        hoverContent.SetActive(true);
    }

    private void ButtonExpand()
    {
        IsExpanded = true;
        StartCoroutine(ExpandButtonWidth());
    }

    private void ButtonRetract()
    {
        hoverContent.SetActive(false);
        StartCoroutine(ReduceButtonWidth());
    }
}
