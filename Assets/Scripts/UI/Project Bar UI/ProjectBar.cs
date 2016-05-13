using UnityEngine;
using UnityEngine.UI;
using Managers;
using TMPro;

class ProjectBar : MonoBehaviour
{
    UIManager uiManagerRef;
    RectTransform projectBarRect;
    TextMeshProUGUI locationText;
    Image barImage;

    void Awake()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        locationText = transform.Find("Target Location").GetComponent<TextMeshProUGUI>();
        projectBarRect = gameObject.GetComponent<RectTransform>();
        barImage = gameObject.GetComponent<Image>();
    }

    void Update()
    {
        if (uiManagerRef.selectedPlanet != null)
            locationText.text = uiManagerRef.selectedPlanet.Name.ToUpper();
        else if (uiManagerRef.selectedSystem != null)
            locationText.text = uiManagerRef.selectedSystem.Name.ToUpper();
        else
            locationText.text = "NONE";
    }

}
