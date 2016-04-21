using UnityEngine;
using UnityEngine.UI;
using Managers;
using TMPro;

namespace Assets.Scripts.UI.Views.Galaxy_View_UI
{
    class MapModeDisplay : MonoBehaviour
    {
        UIManager uiManagerRef;
        TextMeshProUGUI galaxyMapModeInfo;
        TextMeshProUGUI galaxyMapSubModeInfo;
        
        void Awake()
        {
            uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
            galaxyMapModeInfo = GameObject.Find("MapModeInfo").GetComponent<TextMeshProUGUI>();
            galaxyMapSubModeInfo = GameObject.Find("MapSubModeInfo").GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {        
            galaxyMapModeInfo.enabled = true;
            galaxyMapSubModeInfo.enabled = true;
            GetComponentInParent<Image>().enabled = true;
            CheckPrimaryViewMode();
            CheckSecondaryViewMode();                   
        }

        void CheckSecondaryViewMode()
        {
            switch (uiManagerRef.SecondaryViewMode)
            {
                case ViewManager.eSecondaryView.Diplomatic:
                    galaxyMapSubModeInfo.text = "DIPLOMATIC";
                    break;

                case ViewManager.eSecondaryView.Financial:
                    galaxyMapSubModeInfo.text = "FINANCIAL";
                    break;

                case ViewManager.eSecondaryView.Sovereignity:
                    galaxyMapSubModeInfo.text = "SOVEREIGNITY";
                    break;

                case ViewManager.eSecondaryView.Intel:
                    galaxyMapSubModeInfo.text = "INTEL";
                    break;

                case ViewManager.eSecondaryView.Morale:
                    galaxyMapSubModeInfo.text = "MORALE";
                    break;

                case ViewManager.eSecondaryView.Military:
                    galaxyMapSubModeInfo.text = "MILITARY";
                    break;

                case ViewManager.eSecondaryView.Trade:
                    galaxyMapSubModeInfo.text = "TRADE";
                    break;

                default:
                    galaxyMapSubModeInfo.text = "NO";
                    break;
            }

            galaxyMapSubModeInfo.text += " FOCUS";
        }

        void CheckPrimaryViewMode()
        {
            switch (uiManagerRef.PrimaryViewMode)
            {
                case ViewManager.ePrimaryView.Economic:
                    galaxyMapModeInfo.text = "ECONOMIC COMMAND MODE";
                    break;
                case ViewManager.ePrimaryView.Political:
                    galaxyMapModeInfo.text = "POLITICAL COMMAND MODE";
                    break;
                case ViewManager.ePrimaryView.Military:
                    galaxyMapModeInfo.text = "MILITARY COMMAND MODE";
                    break;
                case ViewManager.ePrimaryView.Pops:
                    galaxyMapModeInfo.text = "DEMOGRAPHIC COMMAND MODE";
                    break;
                default:
                    break;
            }           
        }
    }
}
