using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using CameraScripts;

public class ActionTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPrefab; // the prefab to use
    private GameObject tooltipObject; // the actual tooltip instance
    private bool displayToolTip = false;
    private bool toolTipCreated = false;
    public bool exitRequested = false;
    private GraphicAssets graphicDataRef;
    private GameData gDataRef;
    private Canvas charCanvas;
    private float alphaValue = 0f;
    private Color fadeColor;
    private float offsetAmount = 2.0f; // sets the distance away from the center of the tooltipped object

    void Awake()
    {
        charCanvas = GameObject.Find("Character Window Canvas").GetComponent<Canvas>();
        graphicDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
    }

    void OnGUI()
    {
        if (Input.GetButtonDown("Right Mouse Button") || Input.GetButtonDown("Left Mouse Button")) // right mouse button kills the tooltip
        {
            alphaValue = 0f;
            exitRequested = true;
        }

        if (exitRequested)
        {
            displayToolTip = false;
            if (alphaValue <= 0f)
            {
                Destroy(tooltipObject);
            }
            if (tooltipObject != null)
            {
                FadeToolTip(false);
            }
        }

        if (displayToolTip)
        {
            DrawToolTip();
            if (tooltipObject != null)
            {
                FadeToolTip(true);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject != null)
        {
            DestroyImmediate(tooltipObject);
        }
  
        alphaValue = 0f;
        displayToolTip = true;
        exitRequested = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTipCreated = false;
        displayToolTip = false;
        exitRequested = true;
    }

    private void FadeToolTip(bool fadeIn)
    {
        fadeColor = new Color(1, 1, 1, alphaValue / 255f);
        if (fadeIn)
            StartCoroutine(FadeInAlpha(255f));
        else
            StartCoroutine(FadeOutAlpha(0f));

        tooltipObject.GetComponent<Image>().color = fadeColor;

        // white colored data
        tooltipObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().color = fadeColor;
        tooltipObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().color = fadeColor;
    }

    private void DrawToolTip()
    {

        if (!toolTipCreated)
        {
            string name = "";
            string desc = "";
            string type = "";

            Vector3 tooltipLocation = new Vector3(transform.position.x, transform.position.y - 3f, transform.position.z);

            tooltipObject = Instantiate(tooltipPrefab, tooltipLocation, Quaternion.identity) as GameObject;
            tooltipObject.transform.SetParent(charCanvas.transform,true); // assign to canvas

            tooltipObject.transform.localScale = new Vector3(.7f, .7f, .7f);
            //tooltipObject.transform.localPosition = tooltipLocation;
          
            name = gDataRef.CharacterActionList.Find(p => p.ID == transform.name).Name;
            desc = "'" + gDataRef.CharacterActionList.Find(p => p.ID == transform.name).Description + "'";
            type = gDataRef.CharacterActionList.Find(p => p.ID == transform.name).Category.ToString();

            tooltipObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
            tooltipObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = desc;
            tooltipObject.transform.Find("Action Type").GetComponent<TextMeshProUGUI>().text = type;

            gDataRef.activeTooltip = tooltipObject; // assign the active tooltip
            toolTipCreated = true;
        }
    }

    IEnumerator FadeInAlpha(float targetAlpha)
    {
        while (alphaValue < targetAlpha)
        {
            alphaValue += 5f;
            yield return null;
        }
    }

    IEnumerator FadeOutAlpha(float targetAlpha)
    {
        while (alphaValue > targetAlpha)
        {
            alphaValue -= 5f;
            if (alphaValue < 0f)
                alphaValue = 0f;
            yield return null;
        }
    }
}