using UnityEngine;
using TMPro;
using PlanetObjects;
using StellarObjects;
using Managers;

namespace Assets.Scripts.UI
{
    
    class SelectedStellarItemPanel
    {
        private TextMeshProUGUI systemDisplaySystemName;
        private TextMeshProUGUI systemDisplaySecondaryDataLine;
        private UIManager uiManagerRef;

        void Awake()
        {
            systemDisplaySystemName = GameObject.Find("System Name Text").GetComponent<TextMeshProUGUI>();
            systemDisplaySecondaryDataLine = GameObject.Find("Secondary Header Line").GetComponent<TextMeshProUGUI>();
            uiManagerRef = GameObject.Find("UIEngine").GetComponent<UIManager>();
        }

        void Update()
        {
            if (uiManagerRef.ViewMode == UIManager.eViewMode.System)
            {
                systemDisplaySystemName.text = GetSelectedProvince().Name.ToUpper() + " PROVINCE";
                systemDisplaySecondaryDataLine.text = GetSelectedProvince().OwningCiv.Name.ToUpper() + " SOVEREIGNITY";
            }         
        }
    }
}
