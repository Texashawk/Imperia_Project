using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StellarObjects;
using CameraScripts;
using HelperFunctions;
using TMPro;

public class ProvinceSelect : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    GalaxyData gDataRef;
    GalaxyCameraScript galCameraRef;
    Color originalColor;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // set the reference
        galCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalColor = GetComponentInChildren<TextMeshProUGUI>().color;
        GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {        
        GetComponentInChildren<TextMeshProUGUI>().color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -1) // for right mouse click
        {
            if (tag == "Province")
            {
                galCameraRef.provinceTarget = DataRetrivalFunctions.GetProvince(transform.name);
                galCameraRef.systemZoomActive = false;
                galCameraRef.planetZoomActive = false;
                galCameraRef.provinceZoomActive = true;
            }
        }
    }
}
