using UnityEngine;
using PlanetObjects;
using StellarObjects;
using System;
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
