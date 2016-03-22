using UnityEngine;
using PlanetObjects;
using StellarObjects;
using CivObjects;
using EconomicObjects;
using Constants;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Assets.Scripts.Managers
{
    class TradeManager
    {
        // static data references
        private static GalaxyData galaxyDataRef;
        private static GameData gameDataRef;
        public static List<Trade> ActiveTradesInGame = new List<Trade>();
        public static List<TradeFleet> ActiveTradeFleetsInGame = new List<TradeFleet>();
        public static List<TradeGroup> ActiveTradeGroups = new List<TradeGroup>();

        // this will update trade fleets that are actively moving to their destinations (update positions, etc)
        public static void UpdateActiveTradeFleets()
        {

        }

        // once trade fleets have been determined, this function will create and load them, and assign a merchant pop
        public static void GenerateNewTradeFleets()
        {

        }

        // once a trade fleet has completed its mission, this function will deactivate them and release the merchant assigned
        public static void DeactivateTradeFleets()
        {

        }

        public static void CreateTradeAgreements()
        {
            gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            ActiveTradeGroups.Clear(); // clear out the trade groups
            foreach (Civilization civ in gameDataRef.CivList)
            {
                UpdateResourceBasePrices(civ);
                CheckForTrades(civ);
            }
        }

        // Step 1: Determine base prices of each good in the civ
        public static void UpdateResourceBasePrices(Civilization civ)
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

        public static void CheckForTrades(Civilization civ)
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
                GenerateTradeProposals(pData);
            }


            foreach (Province provData in civ.ProvinceList)
            {
                // now determine whether each planet is in the trade network
                if (provData != null)
                    CalculateTradeGroups(provData);
            }
        }

        // Step 2b: create Trade Proposals based on each resource's Importance to that Viceroy
        public static void GenerateTradeProposals(PlanetData pData)
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

            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            SortedList ResourcePriority = new SortedList(); // sorts in ascending order

            // prior to process, clear out the trade proposal list if needed
            pData.ActiveTradeProposalList.Clear();

            // log information used to analyze the trade algorithm
            Debug.Log("____________________________________________________________________");
            Debug.Log("TRADE ANALYSIS THREAD FOR PLANET " + pData.Name.ToUpper() + " OF THE CIVILIZATION " + pData.Owner.Name.ToUpper());
            Debug.Log("VICEROY HUMANITY: " + pData.Viceroy.Humanity.ToString("N0") + "  INTELLIGENCE: " + pData.Viceroy.Intelligence.ToString("N0") + "  CAUTION: " + pData.Viceroy.Caution.ToString("N0"));
            Debug.Log("Viceroy " + pData.Viceroy.Name + " on planet " + pData.Name + " is calculating Importances....");

            // log Importances of each resource
            Debug.Log("Food Importance: " + pData.FoodImportance.ToString("N1"));
            Debug.Log("Energy Importance: " + pData.EnergyImportance.ToString("N1"));
            Debug.Log("Basic Importance: " + pData.BasicImportance.ToString("N1"));
            Debug.Log("Heavy Importance: " + pData.HeavyImportance.ToString("N1"));
            Debug.Log("Rare Importance: " + pData.RareImportance.ToString("N1"));

            // first, determine the budget that can be allocated for these trades by taking the yearly import budget, subtracting the expenses, and dividing by the months left in the year
            maxTotalImportBudget = ((pData.YearlyImportBudget - pData.YearlyImportExpenses) / 10) * (11 - gDataRef.GameMonth); // max allowed total for this month
            Debug.Log("Total Import Budget for this month is " + maxTotalImportBudget.ToString("N1"));

            // now determine the base budget for each item based on weighted importance
            float totalImportance = pData.FoodImportance + pData.EnergyImportance + pData.BasicImportance + pData.HeavyImportance + pData.RareImportance;

            if (totalImportance == 0f) // if there is no importance on any resource, exit since there will not be any trades generated
            {
                Debug.Log("No resources are determined to be needed by the viceroy this month, therefore no proposals will be considered.");
                Debug.Log("Trade Request analysis completed. Exiting for " + pData.Name + "...");
                Debug.Log("_________________________________________________________________________");
                return;
            }

            maxFoodImportBudget = (maxTotalImportBudget * (pData.FoodImportance / totalImportance));
            Debug.Log("Viceroy allocates " + maxFoodImportBudget.ToString("N1") + " crowns towards food imports this month.");
            maxEnergyImportBudget = (maxTotalImportBudget * (pData.EnergyImportance / totalImportance));
            Debug.Log("Viceroy allocates " + maxEnergyImportBudget.ToString("N1") + " crowns towards energy imports this month.");
            maxBasicImportBudget = (maxTotalImportBudget * (pData.BasicImportance / totalImportance));
            Debug.Log("Viceroy allocates " + maxBasicImportBudget.ToString("N1") + " crowns towards basic imports this month.");
            maxHeavyImportBudget = (maxTotalImportBudget * (pData.HeavyImportance / totalImportance));
            Debug.Log("Viceroy allocates " + maxHeavyImportBudget.ToString("N1") + " crowns towards heavy imports this month.");
            maxRareImportBudget = (maxTotalImportBudget * (pData.RareImportance / totalImportance));
            Debug.Log("Viceroy allocates " + maxRareImportBudget.ToString("N1") + " crowns towards rare imports this month.");

            // determine what is left to spend if needed
            unallocatedImportBudget = maxTotalImportBudget - maxFoodImportBudget - maxEnergyImportBudget - maxBasicImportBudget - maxHeavyImportBudget - maxRareImportBudget;
            Debug.Log("After allocations, there is " + unallocatedImportBudget.ToString("N1") + " remaining this month to use for adjusting import bids, or if not used, to return to the yearly import budget.");

            // now determine the theoretical max amount of each resource based on current civ prices plus the transport costs
            foodUnitsDesired = (-1 * pData.FoodDifference) * (pData.Viceroy.Humanity / 50f); // adjust the food based on the humanity of the viceroy
            if (foodUnitsDesired < 0)
                foodUnitsDesired = 0;
            Debug.Log("With a monthly shortfall of " + (-1 * pData.FoodDifference).ToString("N1") + ", " + foodUnitsDesired.ToString("N1") + " food units are requested from the viceroy this month.");

            energyUnitsDesired = (-1 * pData.EnergyDifference);
            if (energyUnitsDesired < 0)
                energyUnitsDesired = 0;
            Debug.Log("With a monthly shortfall of " + (-1 * pData.EnergyDifference).ToString("N1") + ", " + energyUnitsDesired.ToString("N1") + " energy units are requested from the viceroy this month.");

            basicUnitsDesired = (-1 * pData.AlphaTotalDifference) * (50f / pData.Viceroy.Humanity) * (pData.Viceroy.Intelligence / 50f); // materials are based on low humanity and high intelligence
            if (basicUnitsDesired < 0)
                basicUnitsDesired = 0;
            Debug.Log("With a monthly shortfall of " + (-1 * pData.AlphaTotalDifference).ToString("N1") + ", " + basicUnitsDesired.ToString("N1") + " basic units are requested from the viceroy this month.");

            heavyUnitsDesired = (-1 * pData.HeavyTotalDifference) * (50f / pData.Viceroy.Humanity) * (pData.Viceroy.Intelligence / 50f);
            if (heavyUnitsDesired < 0)
                heavyUnitsDesired = 0;
            Debug.Log("With a monthly shortfall of " + (-1 * pData.HeavyTotalDifference).ToString("N1") + ", " + heavyUnitsDesired.ToString("N1") + " heavy units are requested from the viceroy this month.");

            rareUnitsDesired = (-1 * pData.RareTotalDifference) * (50f / pData.Viceroy.Humanity) * (pData.Viceroy.Intelligence / 50f);
            if (rareUnitsDesired < 0)
                rareUnitsDesired = 0;
            Debug.Log("With a monthly shortfall of " + (-1 * pData.RareTotalDifference).ToString("N1") + ", " + rareUnitsDesired.ToString("N1") + " rare units are requested from the viceroy this month.");

            // now, rank the Importance of each resource against each other
            RankResourcesByImportance(pData, ResourcePriority);

            // now in order of top 3 resource priority, and excluding low priority items, calculate what base expenditure will be for each resource
            DetermineTradeAgreementParameters(pData, ResourcePriority, maxFoodImportBudget, foodUnitsDesired, maxEnergyImportBudget, energyUnitsDesired, maxBasicImportBudget, basicUnitsDesired,
                maxRareImportBudget, heavyUnitsDesired, maxHeavyImportBudget, rareUnitsDesired, maxTotalImportBudget);

            // Step 3: Determine how many trade fleets each planet can support this month
            DetermineTradeFleetsAvailable(pData);

            Debug.Log("Trade Request analysis completed. Exiting for " + pData.Name + "...");
            Debug.Log("_________________________________________________________________________");
        }

        // Step 4: Each planet with an active TradeProposal in their queue looks at each trade hub in range that could potentially fill that proposal.
        public static void DetermineEligibleTradePartners(PlanetData currentPData)
        {
            List<string> EligiblePlanetIDsInRange = new List<string>(); // holding list for planets that are potential trade candidates
            List<string> EligiblePlanetIDsWithExports = new List<string>(); // holding list for planets that are in range AND have the export that is being sought
            //EligiblePlanetIDsInRange.Add(checkedPData.ID); // add that ID as an 'in-range' hub
        }

        // Step 4a: Determine Trade Groups that currently exist this turn
        private static void CalculateTradeGroups(Province provData)
        {
            if (provData.SystemList == null)
                return;

            List<StarData> starsInProvince = provData.SystemList;
            List<StarData> starsWithHubs = new List<StarData>(); // list of systems with hubs

            bool provinceTradeHubPresent = false;
            

            // first, find each star in the province that has a hub and put them in the hub list
            foreach (StarData sData in starsInProvince)
            {
                foreach (PlanetData pData in sData.PlanetList)
                {
                    pData.PlanetIsLinkedToTradeHub = false;
                    if (pData.TradeHub == PlanetData.eTradeHubType.CivTradeHub)
                    {
                        pData.PlanetIsLinkedToTradeHub = true; // flag as true (it's the empire!)
                        starsWithHubs.Add(sData);
                        provinceTradeHubPresent = true; // for now, a civ-level trade hub is treated the same as any other province
                        break; // get out; hub found
                    }

                    if (pData.TradeHub == PlanetData.eTradeHubType.ProvinceTradeHub)
                    {
                        pData.PlanetIsLinkedToTradeHub = true; // flag as true (it's the core!)
                        starsWithHubs.Add(sData);
                        provinceTradeHubPresent = true;
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

                if (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.ProvinceTradeHub) || (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.CivTradeHub)))
                {
                    TradeGroup newTG = new TradeGroup();
                    PlanetData hubPlanet = new PlanetData();
                    // if the trade hub is found, get the planet with the hub and create the group
                    if (sData.PlanetList.Exists(p => p.TradeHub == PlanetData.eTradeHubType.CivTradeHub))
                    {
                        hubPlanet = sData.PlanetList.Find(p => p.TradeHub == PlanetData.eTradeHubType.CivTradeHub);
                        //TradeGroup newTG = new TradeGroup();
                        newTG.Name = sData.Name.ToUpper() + " TRADE GROUP : " + hubPlanet.Name.ToUpper() + " IMPERIAL TRADE HUB.";
                        newTG.SystemIDList.Add(sData.ID);
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
                        //TradeGroup newTG = new TradeGroup();
                        newTG.Name = sData.Name.ToUpper() + " TRADE GROUP : " + hubPlanet.Name.ToUpper() + " PROVINCE TRADE HUB.";
                        newTG.SystemIDList.Add(sData.ID);
                        planetsInSystemOwned = sData.PlanetList.FindAll(p => p.Owner == hubPlanet.Owner);
                        // add each planet that belongs to the province hub owner
                        foreach (PlanetData pData in planetsInSystemOwned)
                        {
                            pData.PlanetIsLinkedToTradeHub = true;
                            newTG.PlanetIDList.Add(pData.ID);
                        }                    
                    }

                    CheckPlanetsAttachedToHub(newTG, sData, starsInProvince, planetsInSystemOwned, hubPlanet, starsWithHubs);
                }
            }
        }
        
        private static void CheckPlanetsAttachedToHub(TradeGroup newTG, StarData sData, List<StarData> starsInProvince, List<PlanetData> planetsInSystemOwned, PlanetData hubPlanet, List<StarData>starsWithHubs)
        {
            bool ChainIsActive = true; // recursive function looking for a chained trade group

            // now look for all stars in range of the province trade hub                   
            foreach (StarData sData2 in starsInProvince)
            {
                float sData2HubRange = 0;
                planetsInSystemOwned.Clear(); // clear the planet list
                switch (sData2.LargestTradeHub)
                {
                    case PlanetData.eTradeHubType.NotHub:
                        sData2HubRange = 0;
                        break;
                    case PlanetData.eTradeHubType.SecondaryTradeHub:
                        sData2HubRange = Constant.SecondaryHubRange;
                        break;
                    case PlanetData.eTradeHubType.ProvinceTradeHub:
                        sData2HubRange = Constant.ProvinceHubRange;
                        break;
                    case PlanetData.eTradeHubType.CivTradeHub:
                        sData2HubRange = Constant.ImperialHubRange;
                        break;
                    default:
                        sData2HubRange = 0;
                        break;
                }

                if (HelperFunctions.Formulas.MeasureDistanceBetweenSystems(sData, sData2) <= (Constant.ProvinceHubRange + sData2HubRange))
                {
                    if (sData != sData2) // not the same star!
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
                        PlanetData pData = HelperFunctions.DataRetrivalFunctions.GetPlanet(pDataID);
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
                    //TradeGroup oldTG = ActiveTradeGroups.Find(p => p.Name == newTG.Name);
                    //ActiveTradeGroups.Remove(oldTG);
                    ActiveTradeGroups.Add(newTG); // replace with the more iterated one
                    Debug.Log("New Trade Group Created! Name: " + newTG.Name + " containing " + newTG.PlanetIDList.Count.ToString("N0") + " planets.");
                }
                else
                {
                    //ActiveTradeGroups.Add(newTG); // replace with the more iterated one
                }

                
               
            }
        }

        private static bool CheckForTradeHubLink(StarData originStar, StarData checkedStar, PlanetData.eTradeHubType star1TradeHubType, PlanetData.eTradeHubType star2tradeHubType)
        {
            float distanceBetweenStars = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(originStar, checkedStar);
            int star1HubRadius = 0;
            int star2HubRadius = 0;
            switch (star1TradeHubType)
            {
                case PlanetData.eTradeHubType.SecondaryTradeHub:
                    star1HubRadius = Constant.SecondaryHubRange;
                    break;
                case PlanetData.eTradeHubType.ProvinceTradeHub:
                    star1HubRadius = Constant.ProvinceHubRange;
                    break;
                case PlanetData.eTradeHubType.CivTradeHub:
                    star1HubRadius = Constant.ImperialHubRange;
                    break;
                default:
                    star1HubRadius = 0;
                    break;
            }

            switch (star2tradeHubType)
            {
                case PlanetData.eTradeHubType.SecondaryTradeHub:
                    star2HubRadius = Constant.SecondaryHubRange;
                    break;
                case PlanetData.eTradeHubType.ProvinceTradeHub:
                    star2HubRadius = Constant.ProvinceHubRange;
                    break;
                case PlanetData.eTradeHubType.CivTradeHub:
                    star2HubRadius = Constant.ImperialHubRange;
                    break;
                default:
                    star2HubRadius = 0;
                    break;
            }

            if (distanceBetweenStars <= (star1HubRadius + star2HubRadius))
                return true;
            else
                return false;
        }
        
        public static int DetermineTradeFleetsAvailable(PlanetData pData)
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
            return merchantsAvailableForFleets;
        }

        private static void RankResourcesByImportance(PlanetData pData, SortedList rP)
        {

            float errorCounter = 0f; // the incrementor that allows 'same' keys for sorted lists

            try
            {
                rP.Add(pData.FoodImportance, Trade.eTradeGoodRequested.Food);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.FoodImportance += errorCounter;
                rP.Add(pData.FoodImportance, Trade.eTradeGoodRequested.Food);
            }

            try
            {
                rP.Add(pData.EnergyImportance, Trade.eTradeGoodRequested.Energy);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.EnergyImportance += errorCounter;
                rP.Add(pData.EnergyImportance, Trade.eTradeGoodRequested.Energy);
            }

            try
            {
                rP.Add(pData.BasicImportance, Trade.eTradeGoodRequested.Basic);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.BasicImportance += errorCounter;
                rP.Add(pData.BasicImportance, Trade.eTradeGoodRequested.Basic);
            }

            try
            {
                rP.Add(pData.HeavyImportance, Trade.eTradeGoodRequested.Heavy);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.HeavyImportance += errorCounter;
                rP.Add(pData.HeavyImportance, Trade.eTradeGoodRequested.Heavy);
            }

            try
            {
                rP.Add(pData.RareImportance, Trade.eTradeGoodRequested.Rare);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.RareImportance += errorCounter;
                rP.Add(pData.RareImportance, Trade.eTradeGoodRequested.Rare);
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
                    switch ((Trade.eTradeGoodRequested)rP.GetByIndex(4 - i))
                    {
                        // depending on the trade good, generate the trade agreement based on the type of good, the amount needed, and the viceroy's attributes
                        case Trade.eTradeGoodRequested.Food:
                            tP.TradeResource = TradeProposal.eTradeResource.Food;
                            tP.MaxCrownsToPay = pData.Owner.CurrentFoodPrice * (1f / (pData.Viceroy.Caution / 100f)) / (50f / pData.Viceroy.Intelligence);
                            float foodBudgetCheck = 0f;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (foodBudgetCheck < maxFoodImportBudget && tP.AmountDesired < foodUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                foodBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGoodRequested.Energy:
                            tP.TradeResource = TradeProposal.eTradeResource.Energy;
                            tP.MaxCrownsToPay = pData.Owner.CurrentEnergyPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float energyBudgetCheck = 0;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (energyBudgetCheck < maxEnergyImportBudget && tP.AmountDesired < energyUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                energyBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGoodRequested.Basic:
                            tP.TradeResource = TradeProposal.eTradeResource.Basic;
                            tP.MaxCrownsToPay = pData.Owner.CurrentBasicPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float basicBudgetCheck = 0f;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (basicBudgetCheck < maxBasicImportBudget && tP.AmountDesired < basicUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                basicBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGoodRequested.Heavy:
                            tP.TradeResource = TradeProposal.eTradeResource.Heavy;
                            tP.MaxCrownsToPay = pData.Owner.CurrentHeavyPrice * (1f / (pData.Viceroy.Caution / 100f)) * (50f / pData.Viceroy.Intelligence);
                            float heavyBudgetCheck = 0f;

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (heavyBudgetCheck < maxHeavyImportBudget && tP.AmountDesired < heavyUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                heavyBudgetCheck = tP.AmountDesired * tP.MaxCrownsToPay;
                            }
                            break;

                        case Trade.eTradeGoodRequested.Rare:
                            tP.TradeResource = TradeProposal.eTradeResource.Rare;
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
                        Debug.Log("New Trade Request generated! Taking export budget into account, " + pData.Name + " requests " + tP.AmountDesired.ToString("N1") + " units of " + tP.TradeResource.ToString() + " at a max price per unit of " + tP.MaxCrownsToPay.ToString("N1") + ".");
                    }
                    else if (tP.AmountDesired == 0f)
                        Debug.Log("No trade generated - adjusted unit need was zero.");
                }
            }
        }

        public static void UpdateResourceStockBalances()
        {
            galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            foreach (PlanetData pData in galaxyDataRef.GalaxyPlanetDataList)
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
