using UnityEngine;
using UnityEngine.UI;
using Managers;
using TMPro;

class ProjectButton : MonoBehaviour
{
    private string buttonID;
    private string iconName;
    private GraphicAssets gAssets;
    public TextMeshProUGUI ButtonText;
    public Image ButtonImage;
    private UIManager uiManagerRef;
    public ProjectScrollView ScrollView;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void Update()
    {
        if (uiManagerRef.ModalIsActive)
            gameObject.GetComponent<Button>().interactable = false;
        else
            gameObject.GetComponent<Button>().interactable = true;
    }

    public void SetName(string name)
    {
        ButtonText.text = name;
    }

    public void SetID(string ID)
    {
        buttonID = ID;
    }

    public void SetIcon(string Name)
    {
        iconName = Name;
        if (gAssets.ProjectIconList.Find(p => p.name.ToUpper() == iconName.ToUpper()))
            ButtonImage.sprite = gAssets.ProjectIconList.Find(p => p.name.ToUpper() == iconName.ToUpper());
        else
            ButtonImage.sprite = gAssets.ProjectIconList.Find(p => p.name.ToUpper() == "RENAMEPLANET");
    }

    public void Button_Click()
    {
        ScrollView.ButtonClicked(buttonID);
    }
}
