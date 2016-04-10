using StellarObjects;
using HelperFunctions;
using Constants;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EconomicObjects
{

    public class Trade // This represents the active and pending trades in the game
    {
        public enum eTradeStatus : int
        {
            Request, // in request status
            Accepted, // has been accepted by the exporting planet's vic
            Denied, // has been denied and will be removed in end of turn processing
            Active, // is active and a fleet has been created and is actively moving resources for this trade
            Complete // all runs of trade have been completed and trade will be removed in end of turn processing
        }

        public enum eTradeGood : int
        {
            Food,
            Energy,
            Basic,
            Heavy,
            Rare
        }

        public eTradeStatus Status { get; set; }
        public eTradeGood TradeGood { get; set; }
        public float AmountRequested { get; set; }
        public float OfferPerUnit { get; set; }
        public int RunsRequested { get; set; }
        public string TradeID { get; set; } // randomly generated ID to link trades to trade fleets

        private double _securityModifier;
        public double SecurityModifier
        {
            get { return _securityModifier; }
            set { } // need to write the derived code once security levels are added to planets
        }

        private decimal _energyNeeded;
        public decimal EnergyNeeded
        {
            get { return _energyNeeded; }
            set { } // need to write the derived code to determine energy used
        }

        public string ImportingPlanetID { get; set; } // importing and exporting planet IDs (pass IDs to minimize memory used)
        public string ExportingPlanetID { get; set; }

        private decimal _shippingCost;
        public decimal ShippingCost
        {
            get { return _shippingCost; }
            set
            {
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);

                float distanceBetweenPlanets = Formulas.MeasureDistanceBetweenSystems(importPlanet.System, exportPlanet.System);
                float energyRequired = (distanceBetweenPlanets * AmountRequested) * Constant.EnergyUsedPerLightYearCoeff;
                decimal energyCostPerUnit = (decimal)(energyRequired / AmountRequested);
                decimal shippingCost = energyCostPerUnit * (decimal)(Math.Exp(SecurityModifier) / 5) + 1;
                _shippingCost = shippingCost;
            }
        }
        private decimal _currentProfit;
        public decimal CurrentProfit
        {
            get { return _currentProfit; }
            set { } // write derived variable for determining profit of current trade proposal
        }
    }

    public class TradeGroup // this represents planets that are grouped together into a trade block, linked by trade hubs
    {
        public string Name { get; set; }
        public List<string> PlanetIDList = new List<string>();
        public List<string> SystemIDList = new List<string>();
        public Color GroupColor = new Color(); // color of the trade group
        public string TradeGroupHubID = "";
        public bool ConnectedToCivHub = false; // is the province hub connected to the civilization hub?
    }

    public class TradeProposal // this represents what each viceroy has valued each resource at, how much they want, what type of resource, and how much they are willing to pay for each one
    {
        //public enum eTradeResource : int
        //{
        //    Food,
        //    Energy,
        //    Basic,
        //    Heavy,
        //    Rare
        //}

        public Trade.eTradeGood TradeResource { get; set; } // what type of resource
        public float Importance { get; set; } // the assigned importance of this resource
        public float AmountDesired { get; set; } // how much the viceroy wants to get
        public float MaxCrownsToPay { get; set; } // max willing to pay for this proposal
        public bool NoValidTradePartners { get; set; } // are there any valid trade partners?
    }

    public class TradeFleet // This will have to be changed to reflect the new trade system, probably will have to be called TradeFleet
    {
        public enum eTradeFleetStatus : int
        {
            Active,
            Inactive_Support,
            Inactive_Resources,       
            Mothballed
        }

        public enum eTradeFleetType : int
        {
            Supply,
            Economic
        }

        public string Name { get; set;} // name of fleet - for flavor only
        public eTradeFleetStatus Status { get; set; }
        public eTradeFleetType Type { get; set; }
        public string TradeLinkedID { get; set; } // which trade is linked to this fleet
        public int RunsRemaining { get; set; } // how many times the trade fleet will make the current run
        public float Distance
        {
            get
            {
                float distance = 0f;
                distance = Formulas.MeasureDistanceBetweenSystems(ImportPlanet.System, ExportPlanet.System);
                if (distance < 10)
                    distance = 10;
                return distance;
            }
        }

        public string ImportPlanetID { get; set; } // importing planet
        public string ExportPlanetID { get; set; } // exporting planet

        // get planets
        public PlanetData ImportPlanet
        {
            get
            {
                return DataRetrivalFunctions.GetPlanet(ImportPlanetID);
            }
        }

        public PlanetData ExportPlanet
        {
            get
            {
                return DataRetrivalFunctions.GetPlanet(ExportPlanetID);
            }
        }

        public float Cost
        {
            get
            {
                return 0;
            }
        }

        public float EnergyOnBoard { get; set; }
        public float FoodOnBoard { get; set; }
        public float BasicOnBoard { get; set; }
        public float HeavyOnBoard { get; set; }
        public float RareOnBoard { get; set; }

        public float TotalSent
        {
            get
            {
                float totalSent = 0;
                totalSent = EnergyOnBoard + FoodOnBoard + BasicOnBoard + HeavyOnBoard + RareOnBoard;
                return totalSent;
            }
        }
    }
}
