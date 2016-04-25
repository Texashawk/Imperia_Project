using UnityEngine;
using EconomicObjects;
using TMPro;
using HelperFunctions;
using System.Collections;

public class TradeFleetModel : MonoBehaviour
{

    public Trade tradeFleet = new Trade(); // the trade data
    private TextMeshProUGUI tradeLabel;
    private TextMeshProUGUI turnLabel;
    public Vector3 Destination;
    private bool tradeDataLoaded = false;
    private int turnsRemaining = 0;
    private float distancePerTurn = 500;
    public bool hasArrived = false;
    public Material lineMaterial;
    private LineRenderer lr;

    // Update is called once per frame

    void Awake()
    {
        tradeLabel = transform.FindChild("Canvas/Trade Label").GetComponent<TextMeshProUGUI>();
        turnLabel = transform.FindChild("Canvas/Turn Label").GetComponent<TextMeshProUGUI>();
        gameObject.AddComponent<LineRenderer>(); // attach a new line renderer
        lr = gameObject.GetComponent<LineRenderer>(); // and then create a reference to it
       
    }
    void Update()
    {
        if (tradeFleet != null && !tradeDataLoaded)
        {
            LoadTradeData(); // load data for the object from the attached trade object
            UpdateDestinationLine();
        }

        turnsRemaining = Mathf.CeilToInt(Formulas.MeasureDistanceBetweenLocations(transform.position, Destination) / distancePerTurn); // calculate the turns remaining
        // check for reaching target
        if (turnsRemaining == 0)
        {
            hasArrived = true;
        }

        tradeLabel.text = tradeFleet.AmountRequested.ToString("N1") + " " + tradeFleet.TradeGood.ToString().ToUpper() + " FROM "
            + DataRetrivalFunctions.GetPlanet(tradeFleet.ExportingPlanetID).Name.ToUpper() + " TO " + DataRetrivalFunctions.GetPlanet(tradeFleet.ImportingPlanetID).Name.ToUpper();
        turnLabel.text = turnsRemaining.ToString("N0") + " MOs";
        
    }

    void UpdateDestinationLine()
    {
        lr.SetPosition(0,new Vector3(transform.position.x, transform.position.y, transform.position.z - 10));
        lr.SetPosition(1, Destination);
        lr.material = lineMaterial;
        lr.SetColors(Color.white,Color.red);
        lr.SetWidth(8f, 5f);
    }

    void LoadTradeData()
    {
        if (!tradeDataLoaded && tradeFleet != null)
        {
            Destination = DataRetrivalFunctions.GetPlanet(tradeFleet.ImportingPlanetID).System.WorldLocation;
            tradeDataLoaded = true;
        }
    }

    public IEnumerator MoveFleet(float distance)
    {
        distancePerTurn = distance;
        float step = distance / .33f * Time.deltaTime;
        float distanceMoved = 0;

        if (!hasArrived)
        {
            while (distanceMoved < distance)
            {
                if (transform != null)
                {
                    transform.position = Vector3.MoveTowards(transform.position, Destination, step);
                    distanceMoved += step;
                    UpdateDestinationLine();
                    if ((Mathf.Approximately(transform.position.x, Destination.x) && Mathf.Approximately(transform.position.y, Destination.y)))
                    {
                        hasArrived = true;
                        yield break;
                    }
                    else
                        yield return 0;
                }
            }
        }
        else
            yield break;
    }
}
    
    

