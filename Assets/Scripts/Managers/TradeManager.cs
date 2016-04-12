﻿using UnityEngine;
using StellarObjects;
using CivObjects;
using EconomicObjects;
using Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using HelperFunctions;


namespace Managers
{
    public class TradeManager : MonoBehaviour
    {
        // static data references
        private GameData gameDataRef;
        public List<Trade> ActiveTradesInGame = new List<Trade>();
        public List<TradeFleet> ActiveTradeFleetsInGame = new List<TradeFleet>();
        public List<TradeGroup> ActiveTradeGroups = new List<TradeGroup>();

        public void Awake()
        {
            gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        }

        // this will update trade fleets that are actively moving to their destinations (update positions, etc)
        public void UpdateActiveTradeFleets()
        {

        }

        // once trade fleets have been determined, this function will create and load them, and assign a merchant pop
        public void GenerateNewTradeFleets()
        {

        }

        // once a trade fleet has completed its mission, this function will deactivate them and release the merchant assigned
        public void DeactivateTradeFleets()
        {

        }

        public IEnumerator CreateTrades(Civilization civ) // creates trades that
        {
            Logging.Logger.LogThis("Checking trades for the " + civ.Name + ".");
            foreach (PlanetData checkedPlanet in civ.PlanetList)
            {
                PlanetData pData = new PlanetData();
                pData = checkedPlanet;
        
                if (pData.TradeHub != PlanetData.eTradeHubType.NotHub)
                {
                    int availableMerchants = pData.MerchantsAvailableForExport;
                    
                    TradeGroup activeTradeGroup = new TradeGroup();

                    if (ActiveTradeGroups.Exists(p => p.PlanetIDList.Find(q => q == pData.ID) == pData.ID))
                    {
                        activeTradeGroup = ActiveTradeGroups.Find(p => p.PlanetIDList.Find(q => q == pData.ID) == pData.ID);
                        Logging.Logger.LogThis(""); // line
                        Logging.Logger.LogThis("Looking within " + activeTradeGroup.Name + "....");

                        if (activeTradeGroup.ConnectedToCivHub)
                        {
                            pData = DataRetrivalFunctions.GetPlanet(civ.CapitalPlanetID);
                            Logging.Logger.LogThis("   Connected to civilization trade hub, so checking that trade hub as well...");
                            Logging.Logger.LogThis("Checking " + pData.TradeHub.ToString().ToLower() + ": " + pData.Name + ".");
                            StartCoroutine(CheckHubWithinTradeGroup(activeTradeGroup, pData, availableMerchants));
                            Logging.Logger.LogThis("Checking " + checkedPlanet.TradeHub.ToString().ToLower() + ": " + checkedPlanet.Name + ".");
                            StartCoroutine(CheckHubWithinTradeGroup(activeTradeGroup, checkedPlanet, availableMerchants));
                        }
                        else
                        {
                            Logging.Logger.LogThis("Checking " + pData.TradeHub.ToString().ToLower() + ": " + pData.Name + ".");
                            Logging.Logger.LogThis("   Viceroy trade tendency for hub " + pData.Name + " is " + pData.Viceroy.TraderTendency.ToString("N0") + "; caution is " + pData.Viceroy.Caution.ToString("N0"));
                            StartCoroutine(CheckHubWithinTradeGroup(activeTradeGroup, pData, availableMerchants));
                        }
                                             
                    }                  
                }
            }
            yield return 0;
        }

        private IEnumerator CheckHubWithinTradeGroup(TradeGroup activeTradeGroup, PlanetData pData, int availableMerchants)
        {
            foreach (string tDataID in activeTradeGroup.PlanetIDList)
            {
                if (tDataID != pData.ID) // planets don't trade to themselves
                {
                    PlanetData tData = DataRetrivalFunctions.GetPlanet(tDataID);
                    Logging.Logger.LogThis("Checking on valid trades for planet " + tData.Name + "....");
                    
                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Food) && pData.FoodExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal foodTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Food);
                        Logging.Logger.LogThis("Food request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("   Food requested: " + foodTradeProposal.AmountRequested.ToString("N0") + " units. Food allocated for export on " + pData.Name + ": " + pData.FoodExportAvailable.ToString("N0") + "(includes " +
                            pData.FoodStockpilePercentAvailable.ToString("P0") + " allocated from stockpiles)");

                        if (pData.FoodExportAvailable > foodTradeProposal.AmountRequested)
                        {
                            CreateNewTrade(foodTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("   Trade for food denied due to insufficient food surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Energy) && pData.EnergyExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal energyTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Energy);
                        Logging.Logger.LogThis("Energy request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("   Energy requested: " + energyTradeProposal.AmountRequested.ToString("N0") + " units. Energy allocated for export on " + pData.Name + ": " + pData.EnergyExportAvailable.ToString("N0") + "(includes " +
                            pData.EnergyStockpilePercentAvailable.ToString("P0") + " allocated from stockpiles)");

                        if (pData.EnergyExportAvailable > energyTradeProposal.AmountRequested)
                        {
                            CreateNewTrade(energyTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("   Trade for energy denied due to insufficient energy surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Basic) && pData.BasicExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal basicTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Basic);
                        Logging.Logger.LogThis("Basic material request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("   Basic materials requested: " + basicTradeProposal.AmountRequested.ToString("N0") + " units. Basic materials allocated for export on " + pData.Name + ": " + pData.BasicExportAvailable.ToString("N0") + "(includes " +
                            pData.BasicStockpilePercentAvailable.ToString("P0") + " allocated from stockpiles)");

                        if (pData.BasicExportAvailable > basicTradeProposal.AmountRequested)
                        {
                            CreateNewTrade(basicTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("   Trade for basic materials denied due to insufficient basic material surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Heavy) && pData.HeavyExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal heavyTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Heavy);
                        Logging.Logger.LogThis("Heavy material request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("   Heavy materials requested: " + heavyTradeProposal.AmountRequested.ToString("N0") + " units. Heavy materials allocated for export on " + pData.Name + ": " + pData.HeavyExportAvailable.ToString("N0") + "(includes " +
                            pData.HeavyStockpilePercentAvailable.ToString("P0") + " allocated from stockpiles)");

                        if (pData.HeavyExportAvailable > heavyTradeProposal.AmountRequested)
                        {
                            CreateNewTrade(heavyTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("   Trade for heavy materials denied due to insufficient heavy material surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Rare) && pData.RareExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal rareTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Rare);
                        Logging.Logger.LogThis("Rare material request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("   Rare materials requested: " + rareTradeProposal.AmountRequested.ToString("N0") + " units. Rare materials allocated for export on " + pData.Name + ": " + pData.RareExportAvailable.ToString("N0") + "(includes " +
                            pData.RareStockpilePercentAvailable.ToString("P0") + " allocated from stockpiles)");

                        if (pData.RareExportAvailable > rareTradeProposal.AmountRequested)
                        {
                            CreateNewTrade(rareTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("   Trade for rare materials denied due to insufficient rare material surplus.");
                        }
                    }
                }
            }

            yield return 0;
        }

        private IEnumerator DetermineNewTradeProfitability(Civilization civ)
        {
            Logging.Logger.LogThis(""); // blank line
            Logging.Logger.LogThis("Now determining trade profitability for the " + civ.Name + ".");
            
            foreach (PlanetData pData in civ.PlanetList)
            {
                if (pData.TradeHub != PlanetData.eTradeHubType.NotHub)
                {
                    List<Trade> pendingTrades = pData.ActiveTradesList.FindAll(p => p.Status == Trade.eTradeStatus.InReview);
                    float profitThreshold = 0f; // the minimum profit that the viceroy will take to make the deal

                    profitThreshold = UnityEngine.Random.Range(.5f, (150f + pData.Viceroy.GluttonyTendency - pData.Viceroy.Humanity)/8f) * (1 + (pData.Viceroy.TradeAptitude / 1000f));
                    if (profitThreshold < .5f)
                        profitThreshold = .5f; // minimum profit

                    Logging.Logger.LogThis("");
                    Logging.Logger.LogThis("Viceroy Gluttony: " + pData.Viceroy.GluttonyTendency.ToString("N0") + " Trade Aptitude: " + pData.Viceroy.TradeAptitude.ToString("N0") + " Humanity: " + pData.Viceroy.Humanity.ToString("N0"));
                    Logging.Logger.LogThis("Trade hub " + pData.Name + "'s viceroy will not consider trades with less than " + profitThreshold.ToString("N1") + " MCs per trade.");
                    foreach (Trade analyzedTrade in pendingTrades)
                    {
                        Logging.Logger.LogThis("   Viceroy is considering trade with " + DataRetrivalFunctions.GetPlanet(analyzedTrade.ImportingPlanetID).Name + " for " + analyzedTrade.TradeGood.ToString() + "."
                            + " The current profit per unit based on prices and energy needed for the trip is " + analyzedTrade.CurrentProfitPerUnit.ToString("N1") + " MCs, with total profit of " + analyzedTrade.TotalProfitForTrade.ToString("N1") + ".");
                        if (analyzedTrade.TotalProfitForTrade > profitThreshold)
                        {
                            Logging.Logger.LogThis("    .... After careful consideration, this trade is accepted, pending sufficient fleet availability!");
                            analyzedTrade.Status = Trade.eTradeStatus.Accepted; // the trade is now active!
                        }
                        else
                        {
                            Logging.Logger.LogThis("    .... After careful consideration, this trade is denied! Not enough profit to make the trip.");
                            analyzedTrade.Status = Trade.eTradeStatus.Denied;
                        }
                    }
                }
            }
            yield return 0;
        }

        private void CreateNewTrade(TradeProposal proposal, PlanetData tData, PlanetData pData)
        {
            Trade newTrade = new Trade();
            newTrade.AmountRequested = proposal.AmountRequested;
            newTrade.TradeGood = proposal.TradeResource;
            newTrade.ImportingPlanetID = tData.ID;
            newTrade.Status = Trade.eTradeStatus.InReview;
            newTrade.ExportingPlanetID = pData.ID;
            newTrade.OfferPerUnit = proposal.MaxCrownsToPay;
            newTrade.RunsRequested = 1; // how many times back and forth
            pData.ActiveTradesList.Add(newTrade);
            Logging.Logger.LogThis("   Trade request has been accepted and is now under review: " + newTrade.AmountRequested.ToString("N0") + " " + newTrade.TradeGood.ToString() + " units of " + newTrade.TradeGood.ToString().ToLower() + " for "
                + newTrade.OfferPerUnit.ToString("N1") + " MCs requested from " + pData.Name + " to " + tData.Name + ".");
        }

        public IEnumerator CreateTradeAgreements(Civilization civ)
        {          
            UpdateResourceBasePrices(civ);
            CheckImportanceOfGoods(civ);
            DetermineBaseStockpileHolds(civ);         
            yield return StartCoroutine(CheckForTrades(civ));
            yield return StartCoroutine(CreateTrades(civ));
            yield return StartCoroutine(DetermineNewTradeProfitability(civ));
        }

        // Step 1: Determine base prices of each good in the civ
        public void UpdateResourceBasePrices(Civilization civ)
        {
            float foodPriceTotal = 0f;
            float energyPriceTotal = 0f;
            float basicPriceTotal = 0f;
            float heavyPriceTotal = 0f;
            float rarePriceTotal = 0f;

            for (int x = 0; x < civ.Last6MonthsFoodPrices.Length; x++)
            {
                foodPriceTotal += civ.Last6MonthsFoodPrices[x];
                energyPriceTotal += civ.Last6MonthsEnergyPrices[x];
                basicPriceTotal += civ.Last6MonthsBasicPrices[x];
                heavyPriceTotal += civ.Last6MonthsHeavyPrices[x];
                rarePriceTotal += civ.Last6MonthsRarePrices[x];
            }

            civ.CurrentFoodPrice = foodPriceTotal / civ.Last6MonthsFoodPrices.Length;
            civ.CurrentEnergyPrice = energyPriceTotal / civ.Last6MonthsEnergyPrices.Length;
            civ.CurrentBasicPrice = basicPriceTotal / civ.Last6MonthsBasicPrices.Length;
            civ.CurrentHeavyPrice = heavyPriceTotal / civ.Last6MonthsHeavyPrices.Length;
            civ.CurrentRarePrice = rarePriceTotal / civ.Last6MonthsRarePrices.Length;
        }

        private void DetermineBaseStockpileHolds(Civilization civ)
        {
            foreach (PlanetData pData in civ.PlanetList)
            {
                if (pData.FoodImportance > 80f - pData.Viceroy.Caution)
                    pData.FoodStockpilePercentAvailable = 0;
                else if (pData.FoodImportance > 60f - pData.Viceroy.Caution)
                    pData.FoodStockpilePercentAvailable = UnityEngine.Random.Range(.02f, .1f); // 2-10%
                else
                    pData.FoodStockpilePercentAvailable = UnityEngine.Random.Range(.05f, .5f); // 5-50%

                if (pData.EnergyImportance > 80f - pData.Viceroy.Caution)
                    pData.EnergyStockpilePercentAvailable = 0;
                else if (pData.EnergyImportance > 60f - pData.Viceroy.Caution)
                    pData.EnergyStockpilePercentAvailable = UnityEngine.Random.Range(.02f, .1f); // 2-10%
                else
                    pData.EnergyStockpilePercentAvailable = UnityEngine.Random.Range(.05f, .3f); // 5-30%

                if (pData.BasicImportance > 80f - pData.Viceroy.Caution)
                    pData.BasicStockpilePercentAvailable = 0;
                else if (pData.BasicImportance > 60f - pData.Viceroy.Caution)
                    pData.BasicStockpilePercentAvailable = UnityEngine.Random.Range(.02f, .1f); // 2-10%
                else
                    pData.BasicStockpilePercentAvailable = UnityEngine.Random.Range(.05f, .3f); // 5-30%

                if (pData.HeavyImportance > 80f - pData.Viceroy.Caution)
                    pData.HeavyStockpilePercentAvailable = 0;
                else if (pData.HeavyImportance > 60f - pData.Viceroy.Caution)
                    pData.HeavyStockpilePercentAvailable = UnityEngine.Random.Range(.02f, .08f); // 2-8%
                else
                    pData.HeavyStockpilePercentAvailable = UnityEngine.Random.Range(.05f, .25f); // 5-25%

                if (pData.RareImportance > 80f - pData.Viceroy.Caution)
                    pData.RareStockpilePercentAvailable = 0;
                else if (pData.RareImportance > 60f - pData.Viceroy.Caution)
                    pData.RareStockpilePercentAvailable = UnityEngine.Random.Range(.02f, .05f); // 2-5%
                else
                    pData.RareStockpilePercentAvailable = UnityEngine.Random.Range(.05f, .2f);  // 5-20%

                // adjust stockpile holds by the viceroy's trader tendancy
                pData.FoodStockpilePercentAvailable = pData.FoodStockpilePercentAvailable * (1 + (pData.Viceroy.TraderTendency / 100f));
                pData.EnergyStockpilePercentAvailable = pData.EnergyStockpilePercentAvailable * (1 + (pData.Viceroy.TraderTendency / 100f));
                pData.BasicStockpilePercentAvailable = pData.BasicStockpilePercentAvailable * (1 + (pData.Viceroy.TraderTendency / 100f));
                pData.HeavyStockpilePercentAvailable = pData.HeavyStockpilePercentAvailable * 1 + (pData.Viceroy.TraderTendency / 100f);
                pData.RareStockpilePercentAvailable = pData.RareStockpilePercentAvailable * (1 + (pData.Viceroy.TraderTendency / 100f));
            }
        }

        private void CheckImportanceOfGoods(Civilization civ)
        {
            foreach (PlanetData pData in civ.PlanetList)
            {
                // Step 2: determine Importance of each good on each planet
                pData.FoodImportance = (((50f - (pData.FoodStored / pData.TotalFoodConsumed)) / 5f) - pData.FoodDifference) * Constant.FoodPriority;
                pData.EnergyImportance = (((50f - (pData.EnergyStored / pData.TotalEnergyConsumed)) / 5f) - pData.EnergyDifference) * Constant.EnergyPriority;
                pData.BasicImportance = (((50f - (pData.BasicStored / pData.TotalAlphaMaterialsConsumed)) / 5f) - pData.AlphaTotalDifference) * Constant.BasicPriority;
                pData.HeavyImportance = (((50f - (pData.HeavyStored / pData.TotalHeavyMaterialsConsumed)) / 5f) - pData.HeavyTotalDifference) * Constant.HeavyPriority;
                pData.RareImportance = (((50f - (pData.RareStored / pData.TotalRareMaterialsConsumed)) / 5f) - pData.RareTotalDifference) * Constant.RarePriority;
            }
        }

        private IEnumerator CheckForTrades(Civilization civ)
        {
            
            if (civ.ProvinceList.Count > 0)
            {
                foreach (Province provData in civ.ProvinceList)
                {
                    // now determine whether each planet is in the trade network
                    if (provData != null)
                        yield return StartCoroutine(GenerateTradeGroups(provData));
                }
            }
            yield return StartCoroutine(UpdateResourceStockBalances(civ));
            foreach (PlanetData pData in civ.PlanetList)
            {
                yield return StartCoroutine(GenerateTradeProposals(pData));
            }
            yield return 0;
        }

        // Step 2b: create Trade Proposals based on each resource's Importance to that Viceroy
        private IEnumerator GenerateTradeProposals(PlanetData pData)
        {
            float maxTotalImportBudget = 0f;
            float unallocatedImportBudget = 0f;
            float maxFoodImportBudget = 0f;
            float maxEnergyImportBudget = 0f;
            float maxBasicImportBudget = 0f;
            float maxHeavyImportBudget = 0f;
            float maxRareImportBudget = 0f;
            float foodUnitsDesired = 0f;
            float energyUnitsDesired = 0f;
            float basicUnitsDesired = 0f;
            float heavyUnitsDesired = 0f;
            float rareUnitsDesired = 0f;

            
            SortedList ResourcePriority = new SortedList(); // sorts in ascending order

            // prior to process, clear out the trade proposal list if needed
            pData.ActiveTradeProposalList.Clear();

            // log information used to analyze the trade algorithm
            Logging.Logger.LogThis("____________________________________________________________________");
            Logging.Logger.LogThis("TRADE ANALYSIS THREAD FOR PLANET " + pData.Name.ToUpper() + " OF THE CIVILIZATION " + pData.Owner.Name.ToUpper());
            Logging.Logger.LogThis("VICEROY HUMANITY: " + pData.Viceroy.Humanity.ToString("N0") + "  INTELLIGENCE: " + pData.Viceroy.Intelligence.ToString("N0") + "  CAUTION: " + pData.Viceroy.Caution.ToString("N0"));
            Logging.Logger.LogThis("Viceroy " + pData.Viceroy.Name + " on planet " + pData.Name + " is calculating Importances....");

            // log Importances of each resource
            Logging.Logger.LogThis("Food Importance: " + pData.FoodImportance.ToString("N1"));
            Logging.Logger.LogThis("Energy Importance: " + pData.EnergyImportance.ToString("N1"));
            Logging.Logger.LogThis("Basic Importance: " + pData.BasicImportance.ToString("N1"));
            Logging.Logger.LogThis("Heavy Importance: " + pData.HeavyImportance.ToString("N1"));
            Logging.Logger.LogThis("Rare Importance: " + pData.RareImportance.ToString("N1"));

            // first, determine the budget that can be allocated for these trades by taking the yearly import budget, subtracting the expenses, and dividing by the months left in the year
            maxTotalImportBudget = ((pData.YearlyImportBudget - pData.YearlyImportExpenses) / 10) * (11 - gameDataRef.GameMonth); // max allowed total for this month
            Logging.Logger.LogThis("Total Import Budget for this month is " + maxTotalImportBudget.ToString("N1"));

            // now determine the base budget for each item based on weighted importance
            float totalImportance = pData.FoodImportance + pData.EnergyImportance + pData.BasicImportance + pData.HeavyImportance + pData.RareImportance;

            if (totalImportance == 0f) // if there is no importance on any resource, exit since there will not be any trades generated
            {
                Logging.Logger.LogThis("No resources are determined to be needed by the viceroy this month, therefore no proposals will be considered.");
                Logging.Logger.LogThis("Trade Request analysis completed. Exiting " + pData.Name + "...");
                Logging.Logger.LogThis("_________________________________________________________________________");
                yield break;
            }

            maxFoodImportBudget = (maxTotalImportBudget * (pData.FoodImportance / totalImportance));
            Logging.Logger.LogThis("Viceroy allocates " + maxFoodImportBudget.ToString("N1") + " crowns towards food imports this month.");
            maxEnergyImportBudget = (maxTotalImportBudget * (pData.EnergyImportance / totalImportance));
            Logging.Logger.LogThis("Viceroy allocates " + maxEnergyImportBudget.ToString("N1") + " crowns towards energy imports this month.");
            maxBasicImportBudget = (maxTotalImportBudget * (pData.BasicImportance / totalImportance));
            Logging.Logger.LogThis("Viceroy allocates " + maxBasicImportBudget.ToString("N1") + " crowns towards basic imports this month.");
            maxHeavyImportBudget = (maxTotalImportBudget * (pData.HeavyImportance / totalImportance));
            Logging.Logger.LogThis("Viceroy allocates " + maxHeavyImportBudget.ToString("N1") + " crowns towards heavy imports this month.");
            maxRareImportBudget = (maxTotalImportBudget * (pData.RareImportance / totalImportance));
            Logging.Logger.LogThis("Viceroy allocates " + maxRareImportBudget.ToString("N1") + " crowns towards rare imports this month.");

            // determine what is left to spend if needed
            unallocatedImportBudget = maxTotalImportBudget - maxFoodImportBudget - maxEnergyImportBudget - maxBasicImportBudget - maxHeavyImportBudget - maxRareImportBudget;
            Logging.Logger.LogThis("After allocations, there is " + unallocatedImportBudget.ToString("N1") + " remaining this month to use for adjusting import bids, or if not used, to return to the yearly import budget.");

            // now determine the theoretical max amount of each resource based on current civ prices plus the transport costs
            foodUnitsDesired = (-1 * pData.FoodDifference) * (pData.Viceroy.Humanity / 50f); // adjust the food based on the humanity of the viceroy
            if (foodUnitsDesired < 0)
                foodUnitsDesired = 0;
            Logging.Logger.LogThis("With a monthly shortfall of " + (-1 * pData.FoodDifference).ToString("N1") + ", " + foodUnitsDesired.ToString("N1") + " food units are requested from the viceroy this month.");

            energyUnitsDesired = (-1 * pData.EnergyDifference);
            if (energyUnitsDesired < 0)
                energyUnitsDesired = 0;
            Logging.Logger.LogThis("With a monthly shortfall of " + (-1 * pData.EnergyDifference).ToString("N1") + ", " + energyUnitsDesired.ToString("N1") + " energy units are requested from the viceroy this month.");

            basicUnitsDesired = (-1 * pData.AlphaTotalDifference) * (50f / pData.Viceroy.Humanity) * (pData.Viceroy.Intelligence / 50f); // materials are based on low humanity and high intelligence
            if (basicUnitsDesired < 0)
                basicUnitsDesired = 0;
            Logging.Logger.LogThis("With a monthly shortfall of " + (-1 * pData.AlphaTotalDifference).ToString("N1") + ", " + basicUnitsDesired.ToString("N1") + " basic units are requested from the viceroy this month.");

            heavyUnitsDesired = (-1 * pData.HeavyTotalDifference) * (50f / pData.Viceroy.Humanity) * (pData.Viceroy.Intelligence / 50f);
            if (heavyUnitsDesired < 0)
                heavyUnitsDesired = 0;
            Logging.Logger.LogThis("With a monthly shortfall of " + (-1 * pData.HeavyTotalDifference).ToString("N1") + ", " + heavyUnitsDesired.ToString("N1") + " heavy units are requested from the viceroy this month.");

            rareUnitsDesired = (-1 * pData.RareTotalDifference) * (50f / pData.Viceroy.Humanity) * (pData.Viceroy.Intelligence / 50f);
            if (rareUnitsDesired < 0)
                rareUnitsDesired = 0;
            Logging.Logger.LogThis("With a monthly shortfall of " + (-1 * pData.RareTotalDifference).ToString("N1") + ", " + rareUnitsDesired.ToString("N1") + " rare units are requested from the viceroy this month.");

            // now, rank the Importance of each resource against each other
            RankResourcesByImportance(pData, ResourcePriority);

            // now in order of top 3 resource priority, and excluding low priority items, calculate what base expenditure will be for each resource
            DetermineTradeAgreementParameters(pData, ResourcePriority, maxFoodImportBudget, foodUnitsDesired, maxEnergyImportBudget, energyUnitsDesired, maxBasicImportBudget, basicUnitsDesired,
                maxRareImportBudget, heavyUnitsDesired, maxHeavyImportBudget, rareUnitsDesired, maxTotalImportBudget);

            // Step 3: Determine how many trade fleets each planet can support this month
            DetermineTradeFleetsAvailable(pData);

            Logging.Logger.LogThis("TRADE ANALYSIS COMPLETED FOR " + pData.Name + "...");
            Logging.Logger.LogThis("_________________________________________________________________________");
            Logging.Logger.LogThis(""); // blank space
            yield return 0;
        }

        // Step 4a: Determine Trade Groups that currently exist this turn
        private IEnumerator GenerateTradeGroups(Province provData)
        {
            if (provData.SystemList == null)
                yield return null;

            List<StarData> starsInProvince = provData.SystemList;
            List<StarData> starsWithHubs = new List<StarData>(); // list of systems with hubs

            // first, find each star in the province that has a hub and put them in the hub list
            foreach (StarData sData in starsInProvince)
            {
                sData.SystemIsTradeHub = false; // reset
                foreach (PlanetData pData in sData.PlanetList)
                {
                    pData.PlanetIsLinkedToTradeHub = false;
                    if (pData.TradeHub == PlanetData.eTradeHubType.CivTradeHub)
                    {
                        pData.PlanetIsLinkedToTradeHub = true; // flag as true (it's the empire!)
                        starsWithHubs.Add(sData);                       
                        break; // get out; hub found
                    }

                    if (pData.TradeHub == PlanetData.eTradeHubType.ProvinceTradeHub)
                    {
                        pData.PlanetIsLinkedToTradeHub = true; // flag as true (it's the core!)                        
                        starsWithHubs.Add(sData);                       
                        break; // get out; hub found
                    }

                    if (pData.TradeHub == PlanetData.eTradeHubType.SecondaryTradeHub)
                    {
                        pData.PlanetIsLinkedToTradeHub = true; // flag as true (it's the core!)
                        starsWithHubs.Add(sData);
                        break; // get out; hub found
                    }
                }
            }

            // now, create the first trade group, starting with the province trade hub
            foreach (StarData sData in starsWithHubs)
            {
                List<PlanetData> planetsInSystemOwned = new List<PlanetData>();

                if (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.ProvinceTradeHub) || (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.CivTradeHub)) || 
                        (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.SecondaryTradeHub)))
                {
                    TradeGroup newTG = new TradeGroup();
                    PlanetData hubPlanet = new PlanetData();
                    // if the trade hub is found, get the planet with the hub and create the group
                    if (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.CivTradeHub))
                    {
                        hubPlanet = sData.PlanetList.Find(p => p.TradeHub == PlanetData.eTradeHubType.CivTradeHub);
                        newTG.Name = sData.Name.ToUpper() + " TRADE GROUP";
                        newTG.SystemIDList.Add(sData.ID);
                        sData.SystemIsTradeHub = true;
                        newTG.TradeGroupHubID = hubPlanet.ID;
                        planetsInSystemOwned = sData.PlanetList.FindAll(p => p.Owner == hubPlanet.Owner);
                        // add each planet that belongs to the province hub owner
                        foreach (PlanetData pData in planetsInSystemOwned)
                        {
                            pData.PlanetIsLinkedToTradeHub = true;
                            newTG.PlanetIDList.Add(pData.ID);
                        }
                        
                    }

                    else if (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.ProvinceTradeHub))
                    {
                        hubPlanet = sData.PlanetList.Find(p => p.TradeHub == PlanetData.eTradeHubType.ProvinceTradeHub);
                        newTG.Name = sData.Name.ToUpper() + " TRADE GROUP";
                        sData.SystemIsTradeHub = true;
                        if (ProvinceHubLinkedToCivHub(sData, DataRetrivalFunctions.GetPlanet(sData.OwningCiv.CapitalPlanetID).System))
                            newTG.ConnectedToCivHub = true;
                        newTG.SystemIDList.Add(sData.ID);
                        newTG.TradeGroupHubID = hubPlanet.ID;
                        planetsInSystemOwned = sData.PlanetList.FindAll(p => p.Owner == hubPlanet.Owner);
                        // add each planet that belongs to the province hub owner
                        foreach (PlanetData pData in planetsInSystemOwned)
                        {
                            pData.PlanetIsLinkedToTradeHub = true;
                            newTG.PlanetIDList.Add(pData.ID);
                        }                    
                    }
                    
                    // test code, remove if not work
                    else if (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.SecondaryTradeHub))
                    {
                        hubPlanet = sData.PlanetList.Find(p => p.TradeHub == PlanetData.eTradeHubType.SecondaryTradeHub);
                        newTG.Name = sData.Name.ToUpper() + " TRADE GROUP";
                        newTG.SystemIDList.Add(sData.ID);
                        sData.SystemIsTradeHub = true;
                        newTG.TradeGroupHubID = hubPlanet.ID;
                        planetsInSystemOwned = sData.PlanetList.FindAll(p => p.Owner == hubPlanet.Owner);
                        // add each planet that belongs to the province hub owner
                        foreach (PlanetData pData in planetsInSystemOwned)
                        {
                            pData.PlanetIsLinkedToTradeHub = true;
                            newTG.PlanetIDList.Add(pData.ID);
                        }
                    }

                    // now check that system and check for how far a trade chain stretches, then create the trade group
                    CheckPlanetsAttachedToHub(newTG, sData, starsInProvince, planetsInSystemOwned, hubPlanet, starsWithHubs);
                }
            }
            yield return null;
        }
        
        private void CheckPlanetsAttachedToHub(TradeGroup newTG, StarData sData, List<StarData> starsInProvince, List<PlanetData> planetsInSystemOwned, PlanetData hubPlanet, List<StarData>starsWithHubs)
        {
            bool ChainIsActive = true; // recursive function looking for a chained trade group

            // now look for all stars in range of the system's trade hub                
            foreach (StarData sData2 in starsInProvince)
            {              
                planetsInSystemOwned.Clear(); // clear the planet list
    
                if (Formulas.MeasureDistanceBetweenSystems(sData, sData2) <= (sData.GetRangeOfHub + sData2.GetRangeOfHub))
                {
                    if (sData != sData2 && !sData.SystemIsTradeHub) // not the same star, and the system is not already its own trade hub
                    {
                        planetsInSystemOwned = sData2.PlanetList.FindAll(p => p.Owner == hubPlanet.Owner); // add all the planets that belong to the province hub owner
                        newTG.SystemIDList.Add(sData2.ID);

                        // add each planet that belongs to the province hub owner
                        foreach (PlanetData pData in planetsInSystemOwned)
                        {
                            pData.PlanetIsLinkedToTradeHub = true;
                            newTG.PlanetIDList.Add(pData.ID);
                        }
                    }
                }

                // now look for stars that 'chain' to any of the current trade group hubs; if there are no further links, close the trade group
                while (ChainIsActive)
                {
                    TestActive:
                    foreach (string pDataID in newTG.PlanetIDList)
                    {
                        planetsInSystemOwned.Clear();
                        PlanetData pData = DataRetrivalFunctions.GetPlanet(pDataID);
                        StarData originStar = pData.System;

                        foreach (StarData checkedStar in starsWithHubs)
                        {
                            if (checkedStar != originStar && (!newTG.SystemIDList.Exists(p => p == checkedStar.ID)))
                            {
                                if (CheckForTradeHubLink(originStar, checkedStar, pData.TradeHub, PlanetData.eTradeHubType.SecondaryTradeHub))
                                {
                                    planetsInSystemOwned = checkedStar.PlanetList.FindAll(p => p.Owner == hubPlanet.Owner); // add all the planets that belong to the province hub owner
                                    newTG.SystemIDList.Add(checkedStar.ID);

                                    // add each planet that belongs to the province hub owner
                                    foreach (PlanetData checkedPlanet in planetsInSystemOwned)
                                    {
                                        checkedPlanet.PlanetIsLinkedToTradeHub = true;
                                        newTG.PlanetIDList.Add(checkedPlanet.ID);
                                    }
                                    goto TestActive; // recursive goto                         
                                }
                            }
                        }
                    }
                    ChainIsActive = false; // if you get here, you have iterated through the entire loop without finding another link, so end the group
                }

                // chain is broken, so add the new group and start again
                if (!ActiveTradeGroups.Exists(p => p.Name == newTG.Name)) // only take the largest group
                {
                    if (!ActiveTradeGroups.Exists(p => p.SystemIDList.Exists(x => x == sData.ID))) // if the star doesn't already belong to a trade group
                    {
                        newTG.GroupColor = new Color(UnityEngine.Random.Range(.5f, 1f), UnityEngine.Random.Range(.5f, 1f), UnityEngine.Random.Range(.5f, 1f));
                        ActiveTradeGroups.Add(newTG); // replace with the more iterated one
                        Logging.Logger.LogThis("New Trade Group Created! Name: " + newTG.Name + " containing " + newTG.PlanetIDList.Count.ToString("N0") + " planets.");
                    }
                }
            }
        }

        private bool CheckForTradeHubLink(StarData originStar, StarData checkedStar, PlanetData.eTradeHubType star1TradeHubType, PlanetData.eTradeHubType star2tradeHubType)
        {
            float distanceBetweenStars = Formulas.MeasureDistanceBetweenSystems(originStar, checkedStar);
        
            if (distanceBetweenStars <= (originStar.GetRangeOfHub + checkedStar.GetRangeOfHub))
                return true;
            else
                return false;
        }

        private bool ProvinceHubLinkedToCivHub(StarData originStar, StarData civHomeStar)
        {
            float distanceBetweenStars = Formulas.MeasureDistanceBetweenSystems(originStar, civHomeStar);

            if (distanceBetweenStars <= (originStar.GetRangeOfHub + civHomeStar.GetRangeOfHub))
                return true;
            else
                return false;
        }
        
        public void DetermineTradeFleetsAvailable(PlanetData pData)
        {
            int totalMerchants = 0;
            int merchantsAllocatedToFleets = 0;
            int merchantsAvailableForFleets = 0;
            totalMerchants = pData.TotalMerchants;

            // determine how many merchants are tied up with active fleets
            foreach (TradeFleet tFleet in ActiveTradeFleetsInGame)
            {
                if (tFleet.ExportPlanetID == pData.ID)
                    merchantsAllocatedToFleets += Constant.MerchantsPerTradeFleet;
            }

            merchantsAvailableForFleets = totalMerchants - merchantsAllocatedToFleets;
            Logging.Logger.LogThis("Currently, there are " + pData.TotalMerchants.ToString("N0") + " merchants on planet " + pData.Name + ", of which " + merchantsAvailableForFleets.ToString("N0") + " merchants are available for trade fleets.");
            pData.MerchantsAvailableForExport = merchantsAvailableForFleets;
            Logging.Logger.LogThis("This planet can currently support " + Math.Floor((double)pData.MerchantsAvailableForExport / (double)Constant.MerchantsPerTradeFleet).ToString("N0") + " more fleets.");
        }

        private static void RankResourcesByImportance(PlanetData pData, SortedList rP)
        {

            float errorCounter = 0f; // the incrementor that allows 'same' keys for sorted lists

            try
            {
                rP.Add(pData.FoodImportance, Trade.eTradeGood.Food);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.FoodImportance += errorCounter;
                rP.Add(pData.FoodImportance, Trade.eTradeGood.Food);
            }

            try
            {
                rP.Add(pData.EnergyImportance, Trade.eTradeGood.Energy);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.EnergyImportance += errorCounter;
                rP.Add(pData.EnergyImportance, Trade.eTradeGood.Energy);
            }

            try
            {
                rP.Add(pData.BasicImportance, Trade.eTradeGood.Basic);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.BasicImportance += errorCounter;
                rP.Add(pData.BasicImportance, Trade.eTradeGood.Basic);
            }

            try
            {
                rP.Add(pData.HeavyImportance, Trade.eTradeGood.Heavy);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.HeavyImportance += errorCounter;
                rP.Add(pData.HeavyImportance, Trade.eTradeGood.Heavy);
            }

            try
            {
                rP.Add(pData.RareImportance, Trade.eTradeGood.Rare);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.RareImportance += errorCounter;
                rP.Add(pData.RareImportance, Trade.eTradeGood.Rare);
            }
        }

        private static void DetermineTradeAgreementParameters(PlanetData pData, SortedList rP, float maxFoodImportBudget, float foodUnitsDesired, float maxEnergyImportBudget,
            float energyUnitsDesired, float maxBasicImportBudget, float basicUnitsDesired, float maxRareImportBudget, float heavyUnitsDesired, float maxHeavyImportBudget, float rareUnitsDesired, 
            float maxTotalImportBudget)
        {
            for (int i = 0; i < 3; i++)
            {
                float currentImportance = (float)rP.GetKey(4 - i);

                if (currentImportance >= (0f - pData.Viceroy.Intelligence)) //if the importance is too low, ignore it
                {
                    TradeProposal tP = new TradeProposal(); // generate new proposal
                    switch ((Trade.eTradeGood)rP.GetByIndex(4 - i))
                    {
                        // depending on the trade good, generate the trade agreement based on the type of good, the amount needed, and the viceroy's attributes
                        case Trade.eTradeGood.Food:
                            tP.TradeResource = Trade.eTradeGood.Food;
                            tP.MaxCrownsToPay = pData.Owner.CurrentFoodPrice * (1f / (pData.Viceroy.Caution / 100f)) / (50f / pData.Viceroy.Intelligence);
                            tP.MaxCrownsToPay *= 1.6f;
                            tP.AmountRequested = maxFoodImportBudget / tP.MaxCrownsToPay;
                            if (tP.AmountRequested > foodUnitsDesired)
                                tP.AmountRequested = foodUnitsDesired;
                            break;

                        case Trade.eTradeGood.Energy:
                            tP.TradeResource = Trade.eTradeGood.Energy;
                            tP.MaxCrownsToPay = pData.Owner.CurrentEnergyPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            tP.MaxCrownsToPay *= 1.3f;
                            tP.AmountRequested = maxEnergyImportBudget / tP.MaxCrownsToPay;
                            if (tP.AmountRequested > energyUnitsDesired)
                                tP.AmountRequested = energyUnitsDesired;
                            break;

                        case Trade.eTradeGood.Basic:
                            tP.TradeResource = Trade.eTradeGood.Basic;
                            tP.MaxCrownsToPay = pData.Owner.CurrentBasicPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            tP.MaxCrownsToPay *= 1.5f;
                            tP.AmountRequested = maxBasicImportBudget / tP.MaxCrownsToPay;
                            if (tP.AmountRequested > basicUnitsDesired)
                                tP.AmountRequested = basicUnitsDesired;
                            break;

                        case Trade.eTradeGood.Heavy:
                            tP.TradeResource = Trade.eTradeGood.Heavy;
                            tP.MaxCrownsToPay = pData.Owner.CurrentHeavyPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            tP.MaxCrownsToPay *= 2.2f;
                            tP.AmountRequested = maxHeavyImportBudget / tP.MaxCrownsToPay;
                            if (tP.AmountRequested > heavyUnitsDesired)
                                tP.AmountRequested = heavyUnitsDesired;                          
                            break;

                        case Trade.eTradeGood.Rare:
                            tP.TradeResource = Trade.eTradeGood.Rare;
                            tP.MaxCrownsToPay = pData.Owner.CurrentRarePrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            tP.MaxCrownsToPay *= 2.6f;
                            tP.AmountRequested = maxRareImportBudget / tP.MaxCrownsToPay;
                            if (tP.AmountRequested > rareUnitsDesired)
                                tP.AmountRequested = rareUnitsDesired;
                            break;

                        default:
                            break;
                    }

                    // add to trade list if amount requested is not zero
                    if ((tP.AmountRequested > 0f) && ((tP.AmountRequested * tP.MaxCrownsToPay) <= maxTotalImportBudget))
                    {
                        pData.ActiveTradeProposalList.Add(tP); // add the new trade proposal
                        Logging.Logger.LogThis("New Trade Request generated! Taking export budget into account, " + pData.Name + " requests " + tP.AmountRequested.ToString("N1") + " units of " + tP.TradeResource.ToString() + " at a max price per unit of " + tP.MaxCrownsToPay.ToString("N1") + ".");
                    }
                    else if (tP.AmountRequested == 0f)
                        Logging.Logger.LogThis("No trade generated - adjusted unit need was zero.");
                    else
                        Logging.Logger.LogThis("No trade generated - there is insufficient export budget available.");
                }
            }
        }

        public IEnumerator UpdateResourceStockBalances(Civilization civ)
        {
            foreach (PlanetData pData in civ.PlanetList)
            {
                pData.FoodStored += pData.FoodDifference;
                pData.EnergyStored += pData.EnergyDifference;
                pData.BasicStored += pData.AlphaTotalDifference;
                pData.HeavyStored += pData.HeavyPreProductionDifference;
                pData.RareStored += pData.RarePreProductionDifference;
            }

            yield return 0;
        }
    }
}
