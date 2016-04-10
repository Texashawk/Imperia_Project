using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using PlanetObjects;
using CharacterObjects;
using HelperFunctions;
using Constants;
using GameEvents;

namespace CivObjects
{
    public class Civilization
    {
        public enum eCivType : int
        {
            PlayerEmpire,
            Confederation,
            Satrapy,
            MinorEmpire,
            RenegadeFaction,
            BrokenCivilization,
            Pirates,
            XylHeralds
        }

        public enum eCivSize : int
        {
            SinglePlanet,
            Local,
            Minor,
            Major
        }

        public bool HumanCiv = false; // is this the human player civilization?
        public string CrestFile; // the name of the crest that the civilization uses
        public string Name { get; set; }
        public Color Color { get; set; }
        public float Treasury { get; set; }
        public eCivType Type { get; set; }
        public string ID { get; set; }
        public string CapitalPlanetID { get; set; }
        public int AdminRating { get; set; }
        public eCivSize Size { get; set; }
        public float Revenues { get; set; }
        public float Expenses { get; set; }
        public float Range { get; set; }
        public Character Leader
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetCharacter(LeaderID);
            }
        }
        public string LeaderID { get; set; }
        public List<GameEvent> LastTurnEvents = new List<GameEvent>(); // events from the last turn go here
        public int AstronomyRating { get; set; } // used to determine what information about a star can be gleaned from the civ's technology level
        public int PlanetMinTolerance { get; set; }
        public int CivMaxProvinceSize { get; set; } // used to set the maximum size of provinces

        // ratings for generating pop skill ratings from this civilization, derived from the Houses that make up the civ
        public int MiningBaseRating { get; set; }
        public int FarmingBaseRating { get; set; }
        public int ManufacturingBaseRating { get; set; }
        public int ScienceBaseRating { get; set; }
        public int HighTechBaseRating { get; set; }


        // empire prices for resources
        public float TotalResourcesGenerated
        {
            get
            {
                float totalResources = 0f;

                foreach (PlanetData pData in PlanetList)
                {
                    totalResources += pData.TotalFoodGenerated;
                    totalResources += pData.TotalEnergyGenerated;
                    totalResources += pData.TotalAlphaMaterialsGenerated;
                    totalResources += pData.TotalHeavyMaterialsGenerated;
                    totalResources += pData.TotalRareMaterialsGenerated;
                }

                return totalResources;
            }
        }

        // rolling price for each resource in this civilization
        public float[] Last6MonthsFoodPrices = new float[Constant.RollingMonthsToDeterminePrices];
        public float[] Last6MonthsEnergyPrices = new float[Constant.RollingMonthsToDeterminePrices];
        public float[] Last6MonthsBasicPrices = new float[Constant.RollingMonthsToDeterminePrices];
        public float[] Last6MonthsHeavyPrices = new float[Constant.RollingMonthsToDeterminePrices];
        public float[] Last6MonthsRarePrices = new float[Constant.RollingMonthsToDeterminePrices];

        // current resource prices for this civ
        private float _currentFoodPrice;
        public float CurrentFoodPrice
        {
            get { return _currentFoodPrice; }
            set { _currentFoodPrice = Mathf.Clamp(value, Constant.MinResourcePrice, Constant.MaxResourcePrice); }
        }

        private float _currentEnergyPrice;
        public float CurrentEnergyPrice
        {
            get { return _currentEnergyPrice; }
            set { _currentEnergyPrice = Mathf.Clamp(value, Constant.MinResourcePrice, Constant.MaxResourcePrice); }
        }

        private float _currentBasicPrice;
        public float CurrentBasicPrice
        {          
            get { return _currentBasicPrice; }
            set { _currentBasicPrice = Mathf.Clamp(value, Constant.MinResourcePrice, Constant.MaxResourcePrice); }           
        }

        private float _currentHeavyPrice;
        public float CurrentHeavyPrice
        {
            get { return _currentHeavyPrice; }
            set { _currentHeavyPrice = Mathf.Clamp(value, Constant.MinResourcePrice, Constant.MaxResourcePrice); }
        }

        private float _currentRarePrice;
        public float CurrentRarePrice    
        {
            get { return _currentRarePrice; }
            set { _currentRarePrice = Mathf.Clamp(value, Constant.MinResourcePrice, Constant.MaxResourcePrice); }
        }
        

        public List<string> PlanetIDList = new List<string>(); // which planets does the civilization own (will need to rewrite to take Holdings into account)?
        public List<PlanetData> PlanetList
        {
            get
            {
                List<PlanetData> pList = new List<PlanetData>();
                foreach (string pID in PlanetIDList)
                {
                    if (DataRetrivalFunctions.GetPlanet(pID) != null)
                    {
                        pList.Add(DataRetrivalFunctions.GetPlanet(pID));
                    }           
                }

                return pList;
            }
        }

        public List<Province> ProvinceList
        {
            get
            {
                List<Province> pList = new List<Province>();
                if (SystemList.Count > 0)
                {
                    foreach (StarData sData in SystemList)
                    {
                        if (sData.OwningCiv == this && sData.AssignedProvinceID != null)
                        {
                            if (!pList.Exists(p => p.ID == sData.AssignedProvinceID))
                                pList.Add(sData.Province);
                        }
                    }
                }
                return pList;
            }
        }
        
        public List<StarData> SystemList
        {
            get
            {
                List<StarData> sList = new List<StarData>();
                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.Owner == this)
                    {
                        if (!sList.Exists(p => p.ID == pData.SystemID))
                            sList.Add(pData.System);                      
                    }
                }

                return sList;
            }
        }

        public List<House> HouseList
        {
            get
            {
                List<House> hList = new List<House>();
                foreach (House hData in DataRetrivalFunctions.GetHouseList())
                {
                    if (hData.AffiliatedCiv == ID)
                    {
                        hList.Add(hData);
                    }
                }

                return hList;
            }           
        }

        public List<Pops> PopList // dynamic depending on pops in regions (not all pops in all regions are loyal!)
        {
            get
            {
                List<Pops> pList = new List<Pops>();
                foreach (Region rData in TileList)
                {
                    foreach (Pops pop in rData.PopsInTile)
                    {
                        if (pop.EmpireID == ID)
                            pList.Add(pop);
                    }
                }
                return pList;
            }
        }
       
        public List<Region> TileList // which tiles do the civilizations live on?
        {
            get
            {
                List<Region> rList = new List<Region>();
                foreach (PlanetData pData in DataRetrivalFunctions.GetCivPlanetList(this))
                {
                    foreach (Region rData in pData.RegionList)
                    {
                        if (rData.OwnerCivID == ID)
                            rList.Add(rData);
                    }
                }
                return rList;
            }
        }
        // Use this for initialization (equiv to New)
        public Civilization()
        {
            
        }

    }
}
