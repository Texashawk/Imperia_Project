using UnityEngine;
using UnityEngine.UI;
using StellarObjects;
using Screens.Galaxy;
using Managers;
using TMPro;

public class PlanetGPPWindow : MonoBehaviour {

    // references
    private GameData gDataRef;
    private UIManager uiManagerRef;
    private GalaxyView gScreenRef;
    private PlanetData pData;
    private PlanetView planetViewRef;

    // external objects
    public GameObject GPPTotal;
    public GameObject GPPBase;
    public GameObject GPPRetail;
    public GameObject GPPExport;
    public GameObject GPPImport;

    // internal object
    private TextMeshProUGUI gppTotal;
    private TextMeshProUGUI gppBase;
    private TextMeshProUGUI gppRetail;
    private TextMeshProUGUI gppExport;
    private TextMeshProUGUI gppImport;
    Button viceroyChat;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
        gScreenRef = GameObject.Find("GameEngine").GetComponent<GalaxyView>();
        planetViewRef = GameObject.Find("UI Engine").GetComponent<PlanetView>();
        gppTotal = GPPTotal.GetComponent<TextMeshProUGUI>();
        gppBase = GPPBase.GetComponent<TextMeshProUGUI>();
        gppRetail = GPPRetail.GetComponent<TextMeshProUGUI>();
        gppExport = GPPExport.GetComponent<TextMeshProUGUI>();
        gppImport = GPPImport.GetComponent<TextMeshProUGUI>();
        viceroyChat = gameObject.transform.Find("Title_Bar/Hover_Icon_Talk").GetComponent<Button>();
        viceroyChat.onClick.AddListener(delegate { planetViewRef.ToggleViceroyMode(name); });
    }

    void Update()
    {
        // check for updated planet data
        if (uiManagerRef.selectedPlanet != null)
        {
            // pData = uiManagerRef.selectedPlanet;
            if (pData == null)
            {
                pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                UpdateGPPInfo();
            }

            else if (pData != null)
            {
                if (pData != gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData)
                {
                    pData = gScreenRef.GetSelectedPlanet().GetComponent<Planet>().planetData;
                    UpdateGPPInfo();
                }
            }
            // UpdateGPPInfo();
        }
    }

    void UpdateGPPInfo()
    {
        gppBase.text = "$" + pData.BaseGrossPlanetaryProduct.ToString("N0");
        gppRetail.text = "$" + pData.RetailRevenue.ToString("N0");
        gppExport.text = "$" + pData.ExportRevenue.ToString("N0");
        gppImport.text = "$" + pData.YearlyImportExpenses.ToString("N0") + "/ $" + pData.YearlyImportBudget.ToString("N0");
        gppImport.color = Color.red;
        gppTotal.text = "$" + pData.GrossPlanetaryProduct.ToString("N0");

    }
}
