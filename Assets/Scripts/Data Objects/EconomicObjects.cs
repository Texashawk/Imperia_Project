using StellarObjects;
using HelperFunctions;
using Constants;
using System;
using System.Collections.Generic;
using UnityEngine;
using CivObjects;

namespace EconomicObjects
{

    public class Trade // This represents the active and pending trades in the game
    {
        public enum eTradeStatus : int
        {
            InReview, // in review status
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

        public float EnergyCostPerUnit
        {
            get
            {
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);
                float _energyCostPerUnit = 0f;

                float distanceBetweenPlanets = Formulas.MeasureDistanceBetweenSystems(importPlanet.System, exportPlanet.System);
                float energyRequired = (distanceBetweenPlanets * AmountRequested) * Constant.EnergyUsedPerLightYearCoeff;
                float energyCostPerUnit = (energyRequired / AmountRequested);
                _energyCostPerUnit = energyCostPerUnit;
                _energyCostPerUnit = Mathf.Clamp(_energyCostPerUnit, 0f, 9999f);
                return _energyCostPerUnit;
            }
        }
   
        public float ShippingCostPerUnit
        {
            get
            {
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);
                float _shippingCost = 0f;

                _shippingCost = EnergyCostPerUnit * (float)(Math.Exp(SecurityModifier) / 5) + 1;
                _shippingCost = Mathf.Clamp(_shippingCost, 0f, 9999f);
                return _shippingCost;
            }
        }

        public float TotalProfitForTrade
        {
            get
            {
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);
                float _totalProfit = 0f;

                _totalProfit = CurrentProfitPerUnit * AmountRequested;
                _totalProfit = Mathf.Clamp(_totalProfit, 0f, 9999f);
                return _totalProfit;
            }
        }

        public float TotalCostOfTrade
        {
            get
            {
                float _totalCost = 0f;
                _totalCost = (AmountRequested * ShippingCostPerUnit) + (AmountRequested * OfferPerUnit);
                return _totalCost;
            }
        }

        public float CurrentProfitPerUnit
        {
            get
            {
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                Civilization exportingCiv = exportPlanet.Owner;

                float costOfGood = 0f;
                float offerForGood = OfferPerUnit; // the offer for each unit
                float currentProfit = 0f;

                switch (TradeGood)
                {
                    case eTradeGood.Food:
                        costOfGood = exportingCiv.CurrentFoodPrice;
                        break;
                    case eTradeGood.Energy:
                        costOfGood = exportingCiv.CurrentEnergyPrice;
                        break;
                    case eTradeGood.Basic:
                        costOfGood = exportingCiv.CurrentBasicPrice;
                        break;
                    case eTradeGood.Heavy:
                        costOfGood = exportingCiv.CurrentHeavyPrice;
                        break;
                    case eTradeGood.Rare:
                        costOfGood = exportingCiv.CurrentRarePrice;
                        break;
                    default:
                        break;
                }

                currentProfit = offerForGood - (costOfGood + EnergyCostPerUnit);
                currentProfit = Mathf.Clamp(currentProfit, 0f, 9999f);
                return currentProfit;
            }       
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
        
        public Trade.eTradeGood TradeResource { get; set; } // what type of resource
        public float Importance { get; set; } // the assigned importance of this resource
        public float AmountRequested { get; set; } // how much the viceroy wants to get
        public float MaxCrownsToPay { get; set; } // max willing to pay for this proposal
        public bool NoValidTradePartners { get; set; } // are there any valid trade partners?
        public string ExportingPlanetID { get; set; }
        public string ImportingPlanetID { get; set; }

        private double _securityModifier;
        public double SecurityModifier
        {
            get { return _securityModifier; }
            set { } // need to write the derived code once security levels are added to planets
        }

        public float EnergyCostPerUnit
        {
            get
            {
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);
                float _energyCostPerUnit = 0f;

                float distanceBetweenPlanets = Formulas.MeasureDistanceBetweenSystems(importPlanet.System, exportPlanet.System);
                float energyRequired = (distanceBetweenPlanets * AmountRequested) * Constant.EnergyUsedPerLightYearCoeff;
                float energyCostPerUnit = (energyRequired / AmountRequested);
                _energyCostPerUnit = energyCostPerUnit;
                _energyCostPerUnit = Mathf.Clamp(_energyCostPerUnit, 0f, 9999f);
                return _energyCostPerUnit;
            }
        }

        public float ShippingCostPerUnit
        {
            get
            {
                PlanetData exportPlanet = DataRetrivalFunctions.GetPlanet(ExportingPlanetID);
                PlanetData importPlanet = DataRetrivalFunctions.GetPlanet(ImportingPlanetID);
                float _shippingCost = 0f;

                _shippingCost = EnergyCostPerUnit * (float)(Math.Exp(SecurityModifier) / 5) + 1;
                _shippingCost = Mathf.Clamp(_shippingCost, 0f, 9999f);
                return _shippingCost;
            }
        }
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
        public string ID { get; set; }
        public string LinkedTradeID { get; set; }
        public eTradeFleetStatus Status { get; set; }
        public eTradeFleetType Type { get; set; }
        public string DestinationSystemID
        {
            get
            {
                return ImportPlanet.SystemID;
            }
        }
        public Trade LinkedTrade = new Trade(); // which trade is linked to this fleet
        public int RunsRemaining { get; set; } // how many times the trade fleet will make the current run
        public Vector3 Location { get; set; } // where the fleet is in world space
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

        public string ImportPlanetID { get; set; } // importing planet, destination
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
