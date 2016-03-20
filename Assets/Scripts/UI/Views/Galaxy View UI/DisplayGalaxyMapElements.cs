using UnityEngine;
using System.Collections;
using CameraScripts;
using UnityEngine.UI;
using Managers;

public class DisplayGalaxyMapElements : MonoBehaviour
{

    private GameObject rosette;
    private GameObject UICamera;
    private GameObject mainCamera;
    private GameObject zoomValueText;
    private UIManager uiManagerRef;

    // Use this for initialization
    void Start()
    {
        rosette = GameObject.Find("CompassRose");
        UICamera = GameObject.Find("UI Camera");
        mainCamera = GameObject.Find("Main Camera");
        zoomValueText = GameObject.Find("CameraZoomValue");
        uiManagerRef = GameObject.Find("UI Engine").GetComponent<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        RemoveUIObjects();
    }

    void RemoveUIObjects()
    {
        if (uiManagerRef.ViewMode != ViewManager.eViewLevel.Galaxy)  // take away things that don't belong in the system view
        {
            rosette.SetActive(false);
            zoomValueText.SetActive(false);
        }

        else
        {
            rosette.SetActive(true);
            zoomValueText.SetActive(true);
        }
    }
}
