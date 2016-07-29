using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using CameraScripts;


public class IconHelpTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPrefab; // the prefab to use
    private GameObject tooltipObject; // the actual tooltip instance
    private bool displayToolTip = false;
    private bool toolTipCreated = false;
    public bool exitRequested = false;
    private GraphicAssets graphicDataRef;
    private GameData gDataRef;
    private Canvas tooltipCanvas;
    private float alphaValue = 0f;
    private Color fadeColor;
    private float tooltipDelay = 1.0f;

    void Awake()
    {
        tooltipCanvas = GameObject.Find("Modal Box Canvas").GetComponent<Canvas>();
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
            Destroy(tooltipObject);
        }

        alphaValue = 0f;
        //tooltipDelay -= Time.deltaTime;
        //if (tooltipDelay <= 0.0f)
        //{
            displayToolTip = true;
            exitRequested = false;          
        //}
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTipCreated = false;
        displayToolTip = false;
        exitRequested = true;
        tooltipDelay = 1.0f;
    }

    private void FadeToolTip(bool fadeIn)
    {
        Color originalColor = tooltipObject.GetComponent<Image>().color;
        Color originalTextColor = tooltipObject.transform.Find("Title Text").GetComponent<TextMeshProUGUI>().color;
        Color originalBodyColor = tooltipObject.transform.Find("Body Text").GetComponent<TextMeshProUGUI>().color;

        fadeColor = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue / 255f);
        
        if (fadeIn)
            StartCoroutine(FadeInAlpha(230f));
        else
            StartCoroutine(FadeOutAlpha(0f));

        tooltipObject.GetComponent<Image>().color = fadeColor;

        // white colored data
        //tooltipObject.transform.Find("Title Text").GetComponent<TextMeshProUGUI>().color = fadeColor;
        //tooltipObject.transform.Find("Body Text").GetComponent<TextMeshProUGUI>().color = fadeColor;
    }

    private void DrawToolTip()
    {

        if (!toolTipCreated)
        {
            string name = "";
            string desc = "";

            Vector3 tooltipLocation = new Vector3(transform.position.x, transform.position.y - 3f, transform.position.z);

            tooltipObject = Instantiate(tooltipPrefab, tooltipLocation, Quaternion.identity) as GameObject;
            tooltipObject.transform.SetParent(tooltipCanvas.transform, true); // assign to canvas
            tooltipObject.transform.localScale = new Vector3(.9f, .9f, .9f);

            if (gDataRef.IconHelpList.Exists(p => p.IconName == transform.GetComponent<Image>().sprite.name))
                name = gDataRef.IconHelpList.Find(p => p.IconName == transform.GetComponent<Image>().sprite.name).IconLabel;
            else
                name = transform.GetComponent<Image>().sprite.name;

            if (gDataRef.IconHelpList.Exists(p => p.IconName == transform.GetComponent<Image>().sprite.name))
                desc = gDataRef.IconHelpList.Find(p => p.IconName == transform.GetComponent<Image>().sprite.name).IconDescription;
            else
                desc = "NO HELP ON FILE";

            tooltipObject.transform.Find("Title Text").GetComponent<TextMeshProUGUI>().text = name;
            tooltipObject.transform.Find("Body Text").GetComponent<TextMeshProUGUI>().text = desc;

            // dynamically resize the tooltip by finding the size of the body text
            float textHeight = tooltipObject.transform.Find("Body Text").GetComponent<TextMeshProUGUI>().preferredHeight;
            tooltipObject.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight + 30f);

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

