using UnityEngine;
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
                        Logging.Logger.LogThis("Looking within " + activeTradeGroup.Name + "....");

                        if (activeTradeGroup.ConnectedToCivHub)
                        {
                            pData = DataRetrivalFunctions.GetPlanet(civ.CapitalPlanetID);
                            Logging.Logger.LogThis("   Connected to civilization trade hub, so checking that trade hub as well...");
                            Logging.Logger.LogThis("Checking " + pData.TradeHub.ToString().ToLower() + ": " + pData.Name + ".");
                            CheckHubWithinTradeGroup(activeTradeGroup, pData, availableMerchants);
                            Logging.Logger.LogThis("Checking " + checkedPlanet.TradeHub.ToString().ToLower() + ": " + checkedPlanet.Name + ".");
                            CheckHubWithinTradeGroup(activeTradeGroup, checkedPlanet, availableMerchants);
                        }
                        else
                        {
                            Logging.Logger.LogThis("Checking " + pData.TradeHub.ToString().ToLower() + ": " + pData.Name + ".");
                            CheckHubWithinTradeGroup(activeTradeGroup, pData, availableMerchants);
                        }
                                             
                    }                  
                }
            }
            yield return 0;
        }

        private void CheckHubWithinTradeGroup(TradeGroup activeTradeGroup, PlanetData pData, int availableMerchants)
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
                        Logging.Logger.LogThis("Food requested: " + foodTradeProposal.AmountDesired.ToString("N0") + " units. Food available on " + pData.Name + ": " + pData.FoodExportAvailable.ToString("N0") + ".");
                        if (pData.FoodExportAvailable > foodTradeProposal.AmountDesired)
                        {
                            CreateNewTrade(foodTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("Trade for food denied due to insufficient food surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Energy) && pData.EnergyExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal energyTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Energy);
                        Logging.Logger.LogThis("Energy request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("Energy requested: " + energyTradeProposal.AmountDesired.ToString("N0") + " units. Energy available on " + pData.Name + ": " + pData.EnergyExportAvailable.ToString("N0") + ".");
                        if (pData.EnergyExportAvailable > energyTradeProposal.AmountDesired)
                        {
                            CreateNewTrade(energyTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("Trade for energy denied due to insufficient energy surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Basic) && pData.AlphaExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal basicTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Basic);
                        Logging.Logger.LogThis("Basic material request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("Basic materials requested: " + basicTradeProposal.AmountDesired.ToString("N0") + " units. Basic materials available on " + pData.Name + ": " + pData.AlphaExportAvailable.ToString("N0") + ".");
                        if (pData.AlphaExportAvailable > basicTradeProposal.AmountDesired)
                        {
                            CreateNewTrade(basicTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("Trade for basic materials denied due to insufficient basic material surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Heavy) && pData.HeavyExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal heavyTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Heavy);
                        Logging.Logger.LogThis("Heavy material request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("Heavy materials requested: " + heavyTradeProposal.AmountDesired.ToString("N0") + " units. Heavy materials available on " + pData.Name + ": " + pData.HeavyExportAvailable.ToString("N0") + ".");
                        if (pData.HeavyExportAvailable > heavyTradeProposal.AmountDesired)
                        {
                            CreateNewTrade(heavyTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("Trade for heavy materials denied due to insufficient heavy material surplus.");
                        }
                    }

                    if (tData.ActiveTradeProposalList.Exists(p => p.TradeResource == Trade.eTradeGood.Rare) && pData.RareExportAvailable > 0 && availableMerchants > 0)
                    {
                        TradeProposal rareTradeProposal = tData.ActiveTradeProposalList.Find(p => p.TradeResource == Trade.eTradeGood.Rare);
                        Logging.Logger.LogThis("Rare material request found for " + tData.Name + "! Checking stockpiles to see if it can be considered...");
                        Logging.Logger.LogThis("Rare materials requested: " + rareTradeProposal.AmountDesired.ToString("N0") + " units. Rare materials available on " + pData.Name + ": " + pData.RareExportAvailable.ToString("N0") + ".");
                        if (pData.RareExportAvailable > rareTradeProposal.AmountDesired)
                        {
                            CreateNewTrade(rareTradeProposal, tData, pData);
                            availableMerchants -= 1;
                        }
                        else
                        {
                            Logging.Logger.LogThis("Trade for rare materials denied due to insufficient rare material surplus.");
                        }
                    }
                }
            }
        }

        private void CreateNewTrade(TradeProposal proposal, PlanetData tData, PlanetData pData)
        {
            Trade newTrade = new Trade();
            newTrade.AmountRequested = proposal.AmountDesired;
            newTrade.TradeGood = proposal.TradeResource;
            newTrade.ImportingPlanetID = tData.ID;
            newTrade.Status = Trade.eTradeStatus.Request;
            newTrade.ExportingPlanetID = pData.ID;
            newTrade.OfferPerUnit = proposal.MaxCrownsToPay;
            newTrade.RunsRequested = 1; // how many times back and forth
            pData.ActiveTradesList.Add(newTrade);
            Logging.Logger.LogThis("New trade request created and under consideration: " + newTrade.AmountRequested.ToString("N0") + " " + newTrade.TradeGood.ToString() + " units of " + newTrade.TradeGood.ToString().ToLower() + " requested from " + pData.Name + " to " + tData.Name + ".");
        }

        public IEnumerator CreateTradeAgreements(Civilization civ)
        {          
            UpdateResourceBasePrices(civ);
            yield return StartCoroutine(CheckForTrades(civ));
            yield return StartCoroutine(CreateTrades(civ));
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

        private IEnumerator CheckForTrades(Civilization civ)
        {
            foreach (PlanetData pData in civ.PlanetList)
            {
                // Step 2: determine Importance of each good on each planet
                pData.FoodImportance = (((50f - (pData.FoodStored / pData.TotalFoodConsumed)) / 5f) - pData.FoodDifference) * Constant.FoodPriority;
                pData.EnergyImportance = (((50f - (pData.EnergyStored / pData.TotalEnergyConsumed)) / 5f) - pData.EnergyDifference) * Constant.EnergyPriority;
                pData.BasicImportance = (((50f - (pData.AlphaStored / pData.TotalAlphaMaterialsConsumed)) / 5f) - pData.AlphaTotalDifference) * Constant.BasicPriority;
                pData.HeavyImportance = (((50f - (pData.HeavyStored / pData.TotalHeavyMaterialsConsumed)) / 5f) - pData.HeavyTotalDifference) * Constant.HeavyPriority;
                pData.RareImportance = (((50f - (pData.RareStored / pData.TotalRareMaterialsConsumed)) / 5f) - pData.RareTotalDifference) * Constant.RarePriority;

                // now generate the trade proposals for each viceroy

                yield return StartCoroutine(GenerateTradeProposals(pData));        
            }

            if (civ.ProvinceList.Count > 0)
            {
                foreach (Province provData in civ.ProvinceList)
                {
                    // now determine whether each planet is in the trade network
                    if (provData != null)
                        yield return StartCoroutine(CalculateTradeGroups(provData));
                }
            }

            UpdateResourceStockBalances(civ);
            //yield return 0;
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
                Logging.Logger.LogThis("Trade Request analysis completed. Exiting for " + pData.Name + "...");
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

        // Step 4: Each planet with an active TradeProposal in their queue looks at each trade hub in range that could potentially fill that proposal.
        public void DetermineEligibleTradePartners(PlanetData currentPData)
        {
            List<string> EligiblePlanetIDsInRange = new List<string>(); // holding list for planets that are potential trade candidates
            List<string> EligiblePlanetIDsWithExports = new List<string>(); // holding list for planets that are in range AND have the export that is being sought
           
        }

        // Step 4a: Determine Trade Groups that currently exist this turn
        private IEnumerator CalculateTradeGroups(Province provData)
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
                    merchantsAllocatedToFleets += 1;
            }

            merchantsAvailableForFleets = totalMerchants - merchantsAllocatedToFleets;
            Debug.Log("Based on " + pData.TotalMerchants.ToString("N0") + " on planet " + pData.Name + ", there are " + merchantsAvailableForFleets.ToString("N0") + " merchants available for trade fleets.");
            pData.MerchantsAvailableForExport = merchantsAvailableForFleets;
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
                            float foodBudgetCheck = 0f;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (foodBudgetCheck < maxFoodImportBudget && tP.AmountDesired < foodUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                foodBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGood.Energy:
                            tP.TradeResource = Trade.eTradeGood.Energy;
                            tP.MaxCrownsToPay = pData.Owner.CurrentEnergyPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float energyBudgetCheck = 0;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (energyBudgetCheck < maxEnergyImportBudget && tP.AmountDesired < energyUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                energyBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGood.Basic:
                            tP.TradeResource = Trade.eTradeGood.Basic;
                            tP.MaxCrownsToPay = pData.Owner.CurrentBasicPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float basicBudgetCheck = 0f;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (basicBudgetCheck < maxBasicImportBudget && tP.AmountDesired < basicUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                basicBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGood.Heavy:
                            tP.TradeResource = Trade.eTradeGood.Heavy;
                            tP.MaxCrownsToPay = pData.Owner.CurrentHeavyPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float heavyBudgetCheck = 0f;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (heavyBudgetCheck < maxHeavyImportBudget && tP.AmountDesired < heavyUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                heavyBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGood.Rare:
                            tP.TradeResource = Trade.eTradeGood.Rare;
                            tP.MaxCrownsToPay = pData.Owner.CurrentRarePrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float rareBudgetCheck = 0f;
                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (rareBudgetCheck < maxRareImportBudget && tP.AmountDesired < rareUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                rareBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        default:
                            break;
                    }

                    // add to trade list if amount requested is not zero
                    if ((tP.AmountDesired > 0f) && ((tP.AmountDesired * tP.MaxCrownsToPay) <= maxTotalImportBudget))
                    {
                        pData.ActiveTradeProposalList.Add(tP); // add the new trade proposal
                        Logging.Logger.LogThis("New Trade Request generated! Taking export budget into account, " + pData.Name + " requests " + tP.AmountDesired.ToString("N1") + " units of " + tP.TradeResource.ToString() + " at a max price per unit of " + tP.MaxCrownsToPay.ToString("N1") + ".");
                    }
                    else if (tP.AmountDesired == 0f)
                        Logging.Logger.LogThis("No trade generated - adjusted unit need was zero.");
                    else
                        Logging.Logger.LogThis("No trade generated - there is insufficient export budget available.");
                }
            }
        }

        public void UpdateResourceStockBalances(Civilization civ)
        {
            foreach (PlanetData pData in civ.PlanetList)
            {
                pData.FoodStored += pData.FoodDifference;
                pData.EnergyStored += pData.EnergyDifference;
                pData.AlphaStored += pData.AlphaTotalDifference;
                pData.HeavyStored += pData.HeavyPreProductionDifference;
                pData.RareStored += pData.RarePreProductionDifference;
            }
        }
    }
}
