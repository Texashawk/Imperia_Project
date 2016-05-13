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
                    galaxyMapSubModeInfo.text = "Diplomatic";
                    break;

                case ViewManager.eSecondaryView.Financial:
                    galaxyMapSubModeInfo.text = "Financial";
                    break;

                case ViewManager.eSecondaryView.Sovereignity:
                    galaxyMapSubModeInfo.text = "Sovereignity";
                    break;

                case ViewManager.eSecondaryView.Intel:
                    galaxyMapSubModeInfo.text = "Intel";
                    break;

                case ViewManager.eSecondaryView.Morale:
                    galaxyMapSubModeInfo.text = "Morale";
                    break;

                case ViewManager.eSecondaryView.Military:
                    galaxyMapSubModeInfo.text = "Military";
                    break;

                case ViewManager.eSecondaryView.Trade:
                    galaxyMapSubModeInfo.text = "Trade";
                    break;

                default:
                    galaxyMapSubModeInfo.text = "No";
                    break;
            }

            galaxyMapSubModeInfo.text += " focus";
        }

        void CheckPrimaryViewMode()
        {
            switch (uiManagerRef.PrimaryViewMode)
            {
                case ViewManager.ePrimaryView.Economic:
                    galaxyMapModeInfo.text = "Economic Command Mode";
                    break;
                case ViewManager.ePrimaryView.Political:
                    galaxyMapModeInfo.text = "Political Command Mode";
                    break;
                case ViewManager.ePrimaryView.Military:
                    galaxyMapModeInfo.text = "Military Command Mode";
                    break;
                case ViewManager.ePrimaryView.Demographic:
                    galaxyMapModeInfo.text = "Demographic Command Mode";
                    break;
                default:
                    break;
            }           
        }
    }
}
