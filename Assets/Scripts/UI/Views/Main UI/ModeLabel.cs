using UnityEngine;
using Managers;
using TMPro;

public class ModeLabel : MonoBehaviour {

    UIManager uiManagerRef;
    TextMeshProUGUI modeLabel;
	
	void Awake()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        modeLabel = transform.GetComponent<TextMeshProUGUI>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        modeLabel.text = uiManagerRef.PrimaryViewMode.ToString() + " Command Mode";
	}
}
