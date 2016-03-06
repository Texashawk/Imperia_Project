using UnityEngine;
using PlanetObjects;
using StellarObjects;
using System;
using System.Collections.Generic;


namespace Assets.Scripts.Managers
{
    class TradeManager
    {
        private static GalaxyData galaxyDataRef;
        private static GlobalGameData gameDataRef;

        public static void UpdateActiveTradeFleets()
        {

        }

        public static void GenerateNewTradeFleets()
        {

        }

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
