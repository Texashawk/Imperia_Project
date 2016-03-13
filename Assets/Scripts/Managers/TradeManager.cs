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
        private static GlobalGameData gameDataRef;

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

        public static void CreateTradeAgreements(Civilization civ)
        {
            UpdateResourceBasePrices(civ);
            UpdateGoodImportances(civ);
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

        // Step 2: determine Importance of each good on each planet
        public static void UpdateGoodImportances(Civilization civ)
        {
            foreach (PlanetData pData in civ.PlanetList)
            {
                // calculate each Importance using the basic formula to generate a base 0-100 Importance
                pData.FoodImportance = (((50f - (pData.FoodStored / pData.TotalFoodConsumed)) / 5f) + pData.FoodDifference) * Constant.FoodPriority;
                pData.EnergyImportance = (((50f - (pData.EnergyStored / pData.TotalEnergyConsumed)) / 5f) + pData.EnergyDifference) * Constant.EnergyPriority;
                pData.BasicImportance = (((50f - (pData.AlphaStored / pData.TotalAlphaMaterialsConsumed)) / 5f) + pData.AlphaTotalDifference) * Constant.BasicPriority;
                pData.HeavyImportance = (((50f - (pData.HeavyStored / pData.TotalHeavyMaterialsConsumed)) / 5f) + pData.HeavyTotalDifference) * Constant.HeavyPriority;
                pData.RareImportance = (((50f - (pData.RareStored / pData.TotalRareMaterialsConsumed)) / 5f) + pData.RareTotalDifference) * Constant.RarePriority;

                // now generate the trade proposals for each viceroy
                GenerateTradeProposals(pData);
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
            float errorCounter = 0f; // the incrementor that allows 'same' keys for sorted lists

            GlobalGameData gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            SortedList ResourcePriority = new SortedList(); // sorts in ascending order

            // prior to process, clear out the trade proposal list if needed
            pData.ActiveTradeProposalList.Clear();
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
                Debug.Log("No resources are determined to be needed by the viceroy this month, therefore no proposals will be considered. Exiting...");
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
            try
            {
                ResourcePriority.Add(pData.FoodImportance, Trade.eTradeGoodRequested.Food);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.FoodImportance += errorCounter;
                ResourcePriority.Add(pData.FoodImportance, Trade.eTradeGoodRequested.Food);
            }
      
            try
            {
                ResourcePriority.Add(pData.EnergyImportance, Trade.eTradeGoodRequested.Energy);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.EnergyImportance += errorCounter;
                ResourcePriority.Add(pData.EnergyImportance, Trade.eTradeGoodRequested.Energy);
            }
            
            try
            {
                ResourcePriority.Add(pData.BasicImportance, Trade.eTradeGoodRequested.Basic);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.BasicImportance += errorCounter;
                ResourcePriority.Add(pData.BasicImportance, Trade.eTradeGoodRequested.Basic);
            }
            
            try
            {
                ResourcePriority.Add(pData.HeavyImportance, Trade.eTradeGoodRequested.Heavy);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.HeavyImportance += errorCounter;
                ResourcePriority.Add(pData.HeavyImportance, Trade.eTradeGoodRequested.Heavy);
            }
            
            try
            {
                ResourcePriority.Add(pData.RareImportance, Trade.eTradeGoodRequested.Rare);
            }
            catch (ArgumentException e)
            {
                errorCounter += .000001f;
                pData.RareImportance += errorCounter;
                ResourcePriority.Add(pData.RareImportance, Trade.eTradeGoodRequested.Rare);
            }
            
            // now in order of top 3 resource priority, and excluding low priority items, calculate what base expenditure will be for each resource
            for (int i = 0; i < 3; i++)
            {
                float currentImportance = (float)ResourcePriority.GetKey(4-i);
                
                if ( currentImportance >= (0f - pData.Viceroy.Intelligence)) //if the importance is too low, ignore it
                {
                    TradeProposal tP = new TradeProposal(); // generate new proposal
                    switch ((Trade.eTradeGoodRequested)ResourcePriority.GetByIndex(4-i))
                    {
                        // depending on the trade good, generate the trade agreement based on the type of good, the amount needed, and the viceroy's attributes
                        case Trade.eTradeGoodRequested.Food:
                            tP.TradeResource = TradeProposal.eTradeResource.Food;
                            tP.MaxCrownsToPay = pData.Owner.CurrentFoodPrice * (1f / (pData.Viceroy.Caution / 100f)) / (50f / pData.Viceroy.Intelligence);

                            // raise amount desired by .1 until either amount desired reached or budget for that resource hit
                            while (maxFoodImportBudget > 0 && tP.AmountDesired < foodUnitsDesired)
                            {
                                tP.AmountDesired += .1f;
                                maxFoodImportBudget -= tP.AmountDesired * tP.MaxCrownsToPay;
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
                            while (heavyBudgetCheck < maxRareImportBudget && tP.AmountDesired < heavyUnitsDesired)
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
            Debug.Log("Trade Request analysis completed. Exiting for " + pData.Name + "...");
            Debug.Log("_________________________________________________________________________");
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
