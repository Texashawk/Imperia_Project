using UnityEngine;
using UnityEngine.UI;
using Managers;

public class MilitaryModeButton : MonoBehaviour {

    UIManager uiManagerRef;
    
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
        uiManagerRef.SetPrimaryModeToMilitary();       
    }
}
