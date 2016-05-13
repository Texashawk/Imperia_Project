using UnityEngine;
using UnityEngine.UI;
using Managers;

public class EconomicModeButton : MonoBehaviour
{

    UIManager uiManagerRef;
    // Use this for initialization

    void Start()
    {
        var button = transform.GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void Update()
    {
        if (uiManagerRef.ModalIsActive)
            gameObject.GetComponent<Button>().interactable = false;
        else
            gameObject.GetComponent<Button>().interactable = true;
    }

    void OnClick()
    {
        uiManagerRef.SetPrimaryModeToEconomic();
    }
}
