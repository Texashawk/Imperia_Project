using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

class ProjectButton : MonoBehaviour
    {
        private string buttonID;
        private string characterID;
        private string iconName;
        private GraphicAssets gAssets;
        public TextMeshProUGUI ButtonText;
        public Image ButtonImage;
        public ProjectScrollView ScrollView;

        public void Awake()
        {
            gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
            
        }

        public void SetName(string name)
        {
            ButtonText.text = name;
        }

        public void SetID(string ID)
        {
            buttonID = ID;
        }

        public void SetIcon(string Name)
        {
            iconName = Name;
            ButtonImage.sprite = gAssets.ProjectIconList.Find(p => p.name.ToUpper() == iconName.ToUpper());
        }

        public void Button_Click()
        {
            ScrollView.ButtonClicked(buttonID);
        }
    }
