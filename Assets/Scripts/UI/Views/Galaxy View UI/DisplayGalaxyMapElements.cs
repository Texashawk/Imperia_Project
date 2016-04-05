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
        
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
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
        if (uiManagerRef.ViewLevel != ViewManager.eViewLevel.Galaxy)  // take away things that don't belong in the system view
        {
            rosette.SetActive(false);
           
        }

        else
        {
            rosette.SetActive(true);
           
        }
    }
}
