using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using PlanetObjects;
using Constants;
using CivObjects;
using CharacterObjects;
using EconomicObjects;

namespace StellarObjects //group all stellar objects into this namespace (may change this system later as I learn}
{
    public enum eStellarIntelLevel : int
    {
        None,
        Low,
        Medium,
        High
    }

    public enum eSupportLevel : int
    {
        None,
        Partial,
        Full
    }

    public class NebulaData
    {
        public string Name { get; set; }
        public string nebulaID { get; set; }
        public int NebulaSpriteNumber { get; set; }
        public float NebulaSize { get; set; }
        public Vector3 WorldLocation {get; set;}
        public float TextRotation { get; set; }
    }

    public class Nebula : MonoBehaviour
    {
        public NebulaData nebulaData = new NebulaData();  // the data encapsulated in the object
    }

	public class StarData
	{
        public enum eSpectralClass : int
        {
            NoStar,
            O_B,
            A,
            F,
            G,
            K,
            M,
            L,
            T,
            C,
            SG,
            RG,
            D,
            Neutron,
            BH,
            WR,
            SP,
            SP2
        };

        public enum eStarMultiple : int
        {
            Single,
            Binary,
            Trinary
        }

        public enum eSpecialTrait : int
        {
            Variable,
            Highly_Variable,
            Material_Shedding,
            Accretion_Disk,
            Nova,
            Strong_Solar_Winds,
            Flares,
            None
        }

		public enum eStarSize : int{
			I,
			II,
			IV,
			V,
			VII
		};

        // test variables
        public int timesClicked = 0;                                // tests to see if the data can be changed externally

        // base star variables
        public string Name;                                     // name, duh
        public string ID;                                       // data ID string to find in list
        public eSpectralClass SpectralClass = eSpectralClass.A;     // spectral class (B,K,dwarf, etc)
        public int SecondarySpectralClass = 0;                      // optional
        public int Size = 0;                                    // size of the star
        public int Age = 0;                                         // age of the star
        public double Metallicity = 0.0;                            // determines how rich the system is in minerals and metals
        public eSpecialTrait specialTrait1 = eSpecialTrait.None;    // 1st special trait (optional)
        public eSpecialTrait specialTrait2 = eSpecialTrait.None;    // 2nd special trait (optional)

        // binary/trinary star variables
        public eStarMultiple starMultipleType = eStarMultiple.Single; // if star system is a binary or trinary, flagged here
        public eSpectralClass compSpectralClass = eSpectralClass.NoStar;
        public int compSecondarySpectralClass = 0;
        public eSpectralClass terniaryCompSpectralClass = eSpectralClass.NoStar;
        public int terniaryCompSecondarySpectralClass = 0;
        public bool IsOccupied
        {
            get
            {
                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.IsInhabited)
                        return true;
                }
                return false;
            }
        }

        public Civilization OwningCiv
        {
            get
            {
                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.IsInhabited)
                    {
                        if (pData.Rank == PlanetData.ePlanetRank.SystemCapital || pData.Rank == PlanetData.ePlanetRank.ProvinceCapital || pData.Rank == PlanetData.ePlanetRank.ImperialCapital)
                        {
                            return pData.Owner;
                        }
                    }
                }
                return null;
            }
        }

        // location of star in the universe
		public Vector3 WorldLocation; // the world location of this star

        // intel data (0 == no info beyond star type; 3 == number of planets 6 == general type of planets 10 == full info of all objects in system)
        private int pIntelLevel = 0;
        public int IntelValue
        {
            get
            {
                return pIntelLevel;
            }

            set
            {
                pIntelLevel = Mathf.Clamp(value, 0, 10);
            }
        }

        public eStellarIntelLevel IntelLevel
        {
            get
            {
                if (IntelValue == 0)
                    return eStellarIntelLevel.None;
                else if (IntelValue <= Constants.Constant.LowIntelLevelMax)
                    return eStellarIntelLevel.Low;
                else if (IntelValue <= Constants.Constant.MediumIntelLevelMax)
                    return eStellarIntelLevel.Medium;
                else
                    return eStellarIntelLevel.High;
            }
        }
        // population for the player empire in this system
        public int CivPopulation(Civilization civ)
        {           
            GameData gData;
            gData = GameObject.Find("GameManager").GetComponent<GameData>();
            int popCount = 0;
            foreach (PlanetData pData in PlanetList)
            {
                if (pData.Owner == civ)
                    popCount += pData.TotalPopulation;
            }

            return popCount;
            
        }

        public bool SystemIsTradeHub { get; set; }

        public PlanetData.eTradeHubType LargestTradeHub
        {
            get
            {
                PlanetData.eTradeHubType CurrentTradeHub = PlanetData.eTradeHubType.NotHub; // set initial
                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.TradeHub > CurrentTradeHub)
                        CurrentTradeHub = pData.TradeHub;
                }
                return CurrentTradeHub;
            }
        }

        // to get the effective hub range
        public float GetRangeOfHub
        {
            get
            {
                int TotalMerchantPower = 0;
                float hubRange = 0;

                PlanetData.eTradeHubType CurrentTradeHub = PlanetData.eTradeHubType.NotHub; // set initial
                PlanetData pDataHub = new PlanetData();

                // first get the largest hub
                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.TradeHub > CurrentTradeHub)
                    {
                        CurrentTradeHub = pData.TradeHub;
                        pDataHub = pData;
                    }
                }

                TotalMerchantPower = pDataHub.TotalMerchants * pDataHub.AverageMerchantSkill;
                
                // now calculate the effective range
                switch (pDataHub.TradeHub)
                {
                    case PlanetData.eTradeHubType.NotHub:
                        hubRange = 0;
                        break;
                    case PlanetData.eTradeHubType.SecondaryTradeHub:
                        hubRange = Constant.SecondaryHubBaseRange;
                        hubRange += TotalMerchantPower / 6f; // base modifier
                        break;
                    case PlanetData.eTradeHubType.ProvinceTradeHub:
                        hubRange = Constant.ProvinceHubBaseRange;
                        hubRange += TotalMerchantPower / 6f; // base modifier
                        break;
                    case PlanetData.eTradeHubType.CivTradeHub:
                        hubRange = Constant.ImperialHubBaseRange;
                        hubRange += TotalMerchantPower / 5f; // base modifier
                        break;
                    default:
                        break;
                }

                return hubRange;
            }
        }

        // array list
        public PlanetData[] PlanetSpots = new PlanetData[5];
        public List<PlanetData> PlanetList
        {
            get
            {
                List<PlanetData> pList = new List<PlanetData>();
                GalaxyData gData;
                gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();

                pList = gData.GalaxyPlanetDataList.FindAll(p => p.SystemID == ID);

                return pList;
            }
        }

        public Character Governor
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                if (gData.CharacterList.Exists(p => p.SystemAssignedID == ID))
                {
                    return gData.CharacterList.Find(p => p.SystemAssignedID == ID);
                }
                else
                    return null;
            }
        }

        public float BaseSystemValue
        {
            get
            {
                float totalValue = 0;
                int planetCount = 0;

                planetCount = PlanetList.Count;

                foreach (PlanetData pData in PlanetList)
                {
                    totalValue += pData.BasePlanetValue;
                }

                return (totalValue);
            }
        }

        // other vars
        public bool IsProvinceHub = false;
        private string pAssignedProvince = "";
        public string HouseIDHolding { get; set; } // which House holds this as a Holding
        public string SystemCapitalID = "";
        public string AssignedProvinceID
        {
            get
            {
                return pAssignedProvince;
            }

            set
            {
                pAssignedProvince = value;
            }
        }

        public Province Province
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetProvince(AssignedProvinceID);
            }
        }
       

        // calculate stellar planetary material last as a property
        public double pMaterial
        {
            get
            {
                double pMat = Math.Sqrt(Metallicity) * 3 + 1; // get base pMaterial
                float pMult = 1; // base multiplier

                switch ((int)SpectralClass)
                {
                    case 0:
                        pMult = .75f;
                        break;
                    case 1:
                        pMult = 2;
                        break;
                    case 2:
                        pMult = 1.5f;
                        break;
                    case 3:
                        pMult = 1.25f;
                        break;
                    case 4:
                        pMult = 1f;
                        break;
                    case 5:
                        pMult = 1f;
                        break;
                    case 6:
                        pMult = .8f;
                        break;
                    case 7:
                        pMult = .6f;
                        break;
                    case 8:
                        pMult = .5f;
                        break;
                    case 9:
                        pMult = 1.25f;
                        break;
                    case 10:
                        pMult = .85f;
                        break;
                    case 11:
                        pMult = .75f;
                        break;
                    case 12:
                        pMult = .5f;
                        break;
                    case 13:
                        pMult = .4f;
                        break;
                    case 14:
                        pMult = .2f;
                        break;
                    case 15:
                        pMult = .2f;
                        break;
                    case 16:
                        pMult = 1f;
                        break;
                    case 17:
                        pMult = 1f;
                        break;
                    default:
                        pMult = 1;
                        break;
                }

                // adjust for age and multiplier based on star type
                pMat *= pMult;
                pMat *= 1.25 - (Age / 20);

                //adjust for companions
                if (starMultipleType == eStarMultiple.Binary)
                    pMat *= .8;
                else if (starMultipleType == eStarMultiple.Trinary)
                    pMat *= .5;

                //normalize the value
                if (pMat < 1)
                    pMat = 1;
                if (pMat > 10)
                    pMat = 10;

                return pMat;
            }
        }     
       
		public StarData ()  // expand to include more initialization
		{
	    
		}

		public void SetWorldLocation(Vector3 loc)  //moves the star object to a specific location
		{
            WorldLocation = loc;
		}
         
	}

    public class Star : MonoBehaviour
    {
        public StarData starData = new StarData();  // the data encapsulated in the object
    }

    public class Planet : MonoBehaviour
    {
        public PlanetData planetData = new PlanetData();  // planetary data
    }

	public class PlanetData
  	{

        // constants for generation
        const int maxRings = 5;
        const int maxMoons = 10;
        const float maxIndMultiplier = 1.5f;

        public GameData gData;

        public enum ePlanetRank : int
        {
            Uninhabited,
            Outpost,
            NewColony,
            EstablishedColony,
            SystemCapital,
            ProvinceCapital,
            ImperialCapital
        }

        public enum eTradeStatus : int
        {
            NoTrade,
            ImportOnly,
            HasTradePort,
            HasTradeHub    
        }

        public enum eTradeHubType : int
        {
            NotHub,
            SecondaryTradeHub,
            ProvinceTradeHub,
            CivTradeHub
        }

        // enumerations for planets
        public enum ePlanetType : int
        {
            NoPlanet,
            AsteroidBelt,
            Barren,
            Greenhouse,
            Desert,
            Terran,
            Ice,
            IceGiant,
            GasGiant,
            IceBelt,
            Lava,
            Irradiated,
            SuperEarth,
            Ocean,
            BrownDwarf,
            Organic,
            DustRing,
            City
        };

        // basic public variables
        public string Name { get; set; } // name of the planet
        public string Description { get; set; } // description of the planet that is generated at runtime
        public string ID { get; set; } // unique ID of planet
        public string HouseIDHolding { get; set; } // which House owns this planet as a Holding
        public int IntelLevel { get; set; } // the intel level of the planet; will change to a table that each civ has (one civ doesn't have the same intel level as another)
        public ePlanetType Type { get; set; } // type of planet (desert, ocean, etc)             

        // planet budget parameters that are set
        public float PercentGPPForTax { get; set; } // tax that is paid out
        public float PercentGPPForTrade { get; set; } // budget set for interal trade?
        public float PercentGPPForImports { get; set; } // budget set for importing goods       
        public float ExportRevenue { get; set; } // GPP that is derived from export sales
        public float YearlyImportExpenses { get; set; } // GPP that is subtracted throughout the year due to import purchases

        // trade variables
        public string SupplyToPlanetID = "";
        private float _pYearlyImportBudget;
        public float YearlyImportBudget // this is set at the beginning of the year
        {
            get
            {
                _pYearlyImportBudget = Mathf.Clamp(GrossPlanetaryProduct * PercentGPPForImports, 0, GrossPlanetaryProduct);
                return _pYearlyImportBudget;
            }
        }
        public eTradeStatus TradeStatus { get; set; }
        public eTradeHubType TradeHub { get; set; }
        public List<TradeProposal> ActiveTradeProposalList = new List<TradeProposal>();
        public List<Trade> ActiveTradesList = new List<Trade>();
        public int MerchantsAvailableForExport { get; set; }  
        
        public bool PlanetIsLinkedToTradeHub { get; set; } // may change this to 'real-time' variable
        public List<string> PlanetsInTradeGroup = new List<string>(); // list of planets that this planet may trade with (in their 'trade group') - calculated externally
       
        // resource Importances on each planet
        private float _foodImportance;
        public float FoodImportance
        { 
            get { return _foodImportance; }
            set { _basicImportance = Mathf.Clamp(value, 0, 100); }
        }
        private float _energyImportance;
        public float EnergyImportance
        {
            get { return _energyImportance; }
            set { _energyImportance = Mathf.Clamp(value, 0, 100); }
        }
        private float _basicImportance;
        public float BasicImportance
        {
            get { return _basicImportance; }
            set { _basicImportance = Mathf.Clamp(value, 0, 100); }
        }
        private float _heavyImportance;
        public float HeavyImportance
        {
            get { return _heavyImportance; }
            set { _heavyImportance = Mathf.Clamp(value, 0, 100); }
        }
        private float _rareImportance;
        public float RareImportance
        {
            get { return _rareImportance; }
            set { _rareImportance = Mathf.Clamp(value, 0, 100); }
        }

        // export percent holds
        public float FoodExportPercentHold { get; set; }
        public float EnergyExportPercentHold { get; set; }
        public float BasicExportPercentHold { get; set; }
        public float HeavyExportPercentHold { get; set; }
        public float RareExportPercentHold { get; set; }

        // commerce percent holds
        public float FoodCommercePercentHold { get; set; }
        public float EnergyCommercePercentHold { get; set; }
        public float BasicCommercePercentHold { get; set; }
        public float HeavyCommercePercentHold { get; set; }
        public float RareCommercePercentHold { get; set; }

        // stockpile % allowed to export or use for commerce
        private float _foodStockpilePercentAvailable;
        public float FoodStockpilePercentAvailable
        {
            get { return _foodStockpilePercentAvailable; }
            set { _foodStockpilePercentAvailable = Mathf.Clamp(value, 0f, 1f); }          
        }
        private float _energyStockpilePercentAvailable;
        public float EnergyStockpilePercentAvailable
        {
            get { return _energyStockpilePercentAvailable; }
            set { _energyStockpilePercentAvailable = Mathf.Clamp(value, 0f, 1f); }
        }
        private float _basicStockpilePercentAvailable;
        public float BasicStockpilePercentAvailable
        {
            get { return _basicStockpilePercentAvailable; }
            set { _basicStockpilePercentAvailable = Mathf.Clamp(value, 0f, 1f); }
        }
        private float _heavyStockpilePercentAvailable;
        public float HeavyStockpilePercentAvailable
        {
            get { return _heavyStockpilePercentAvailable; }
            set { _heavyStockpilePercentAvailable = Mathf.Clamp(value, 0f, 1f); }
        }
        private float _rareStockpilePercentAvailable;
        public float RareStockpilePercentAvailable
        {
            get { return _rareStockpilePercentAvailable; }
            set { _rareStockpilePercentAvailable = Mathf.Clamp(value, 0f, 1f); }
        }

        public float CommerceTax { get; set; }
        public int PlanetSpot { get; set; }
        
        public float PlanetSystemScaleSize { get; set; } // a place to store what the base scale of a planet object should be in system view

        // derived public accessors //
        public int Diameter // diameter of the planet
        {
            get
            {
                int diam = 0;
                diam = Size * 200;
                return diam;
            }
        }

        public float AxialTilt { get; set; }

        // get the owner
        public Civilization Owner
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                Civilization owningCiv = null;

                foreach (Civilization civ in gData.CivList)
                {
                    if (civ.PlanetList.Exists(p => p.ID == ID))
                    {
                        owningCiv = civ;
                    }
                }

                return owningCiv;
            }
        }

        // % of scan level completed for atmosphere
        public float AtmosphereScanLevel
        {
            get
            {
                float scanLevel = 0;
                if (ScanLevel / .33f >= 1f)
                {
                    scanLevel = 1f;
                }
                else
                {
                    scanLevel = ScanLevel / .33f;
                }
                return scanLevel;
            }
        }

        // % of scan level completed for surface
        public float SurfaceScanLevel
        {
            get
            {
                float scanLevel = 0;
                if (ScanLevel / .75f >= 1f)
                {
                    scanLevel = 1f;
                }
                else
                {
                    scanLevel = ScanLevel / .75f;
                }
                return scanLevel;
            }
        }

        // % of scan level completed for interior
        public float InteriorScanLevel
        {
            get
            {
                float scanLevel = 0;
                if (ScanLevel / 1f >= 1f)
                {
                    scanLevel = 1f;
                }
                else
                {
                    scanLevel = ScanLevel / 1f;
                }
                return scanLevel;
            }
        }
       
        // viceroy of the planet
        public Character Viceroy
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                if (gData.CharacterList.Exists(p => p.PlanetAssignedID == ID))
                {
                    return gData.CharacterList.Find(p => p.PlanetAssignedID == ID);
                }
                else
                    return null;
            }
        }

        // build plan set for the planet
        public BuildPlan BuildPlan = new BuildPlan();
       
        // total gross planetary product that is generated every month (used to set taxes and money sent every month)
        public float GrossPlanetaryProduct
        {
            get
            {
                // find base amount
                float baseGPP = 0;
                float exports = 0;
                float imports = 0;
                float retail = 0;
                float totalGPP = 0;

                baseGPP = BaseGrossPlanetaryProduct;
                exports = ExportRevenue;
                imports = YearlyImportExpenses;
                retail = RetailRevenue;
                totalGPP = baseGPP + retail + exports - imports;

                return totalGPP;                      
            }
        }

        public float BaseGrossPlanetaryProduct
        {
            get
            {
                return AverageDevelopmentLevel * TotalPopulation * Constant.BaseEconFactor;
            }
        }

        public float DomesticFoodAvailable
        {
            get
            {
                float domesticFoodAvailable = FoodDifference * FoodCommercePercentHold;
                if (domesticFoodAvailable < 0)
                    domesticFoodAvailable = 0;
                return domesticFoodAvailable;
            }
        }

        public float DomesticEnergyAvailable
        {
            get
            {
                float domesticEnergyAvailable = EnergyDifference * EnergyCommercePercentHold;
                if (domesticEnergyAvailable < 0)
                    domesticEnergyAvailable = 0;
                return domesticEnergyAvailable;
            }
        }

        public float RetailRevenue
        {
            get
            {
                float retailRevenue;
                retailRevenue = FoodRetailRevenue + EnergyRetailRevenue;
                return retailRevenue;
            }
        }

        public float EnergyRetailRevenue
        {
            get
            {
                float energyRetailRevenue = 0f;
                float energyAvailable = DomesticEnergyAvailable;
                float merchantsTotal = TotalMerchants;
                float merchantEfficiency = TotalMerchants / energyAvailable;

                if (merchantEfficiency > 1f) // normalize efficiency
                {
                    merchantEfficiency = 1f;
                }

                energyRetailRevenue = (energyAvailable * merchantEfficiency) * (AverageMerchantSkill / 100f) * Owner.CurrentEnergyPrice * energyAvailable * CommerceTax;

                return energyRetailRevenue;
            }
        }

        public float FoodRetailRevenue
        {
            get
            {
                float foodRetailRevenue = 0f;
                float foodAvailable = DomesticFoodAvailable;
                float merchantsTotal = TotalMerchants;
                float merchantEfficiency = TotalMerchants / foodAvailable;

                if (merchantEfficiency > 1f) // normalize efficiency
                {
                    merchantEfficiency = 1f;
                }

                foodRetailRevenue = (foodAvailable * merchantEfficiency) * (AverageMerchantSkill / 100f) * Owner.CurrentFoodPrice * foodAvailable * CommerceTax;

                return foodRetailRevenue;
            }
        }

        public float TradeAmount
        {
            get
            {
                return ExportRevenue - YearlyImportExpenses;
            }
        }

       
        // support levels
        public eSupportLevel SysGovSupport
        {
            get
            {             
                bool houseMatch = false;
                float relationshipValue = HelperFunctions.DataRetrivalFunctions.GetSystem(SystemID).Governor.Relationships[Viceroy.ID].Trust;

                if (HelperFunctions.DataRetrivalFunctions.GetSystem(SystemID).Governor.HouseID == Viceroy.HouseID)
                {
                    houseMatch = true;
                }

                // 
                if (houseMatch || relationshipValue > 60)
                {
                    return eSupportLevel.Full;
                }
                else if ((!houseMatch && relationshipValue > 40) || (houseMatch && relationshipValue > 30))
                {
                    return eSupportLevel.Partial;
                }
                else
                    return eSupportLevel.None;
            }
        }

        public eSupportLevel ProvGovSupport
        {
            get
            {
                bool houseMatch = false;
                float relationshipValue = HelperFunctions.DataRetrivalFunctions.GetProvince(HelperFunctions.DataRetrivalFunctions.GetSystem(SystemID).AssignedProvinceID).Governor.Relationships[Viceroy.ID].Trust;

                if (HelperFunctions.DataRetrivalFunctions.GetProvince(HelperFunctions.DataRetrivalFunctions.GetSystem(SystemID).AssignedProvinceID).Governor.HouseID == Viceroy.HouseID)
                {
                    houseMatch = true;
                }

                // 
                if (houseMatch || relationshipValue > 60)
                {
                    return eSupportLevel.Full;
                }
                else if ((!houseMatch && relationshipValue > 40) || (houseMatch && relationshipValue > 30))
                {
                    return eSupportLevel.Partial;
                }
                else
                    return eSupportLevel.None;
            }
        }
        
        // stored resources
        private float _pFoodStored;
        public float FoodStored
        {
            get
            {
                return _pFoodStored;
            }

            set
            {
                _pFoodStored = Mathf.Clamp(value,0,999999);
            }
        }
        private float _pBasicStored;
        public float BasicStored
        {
            get
            {
                return _pBasicStored;
            }

            set
            {
                _pBasicStored = Mathf.Clamp(value, 0, 999999);
            }
        }
        private float _pHeavyStored;
        public float HeavyStored
        {
            get
            {
                return _pHeavyStored;
            }

            set
            {
                _pHeavyStored = Mathf.Clamp(value, 0, 999999);
            }
        }
        private float _pRareStored;
        public float RareStored
        {
            get
            {
                return _pRareStored;
            }

            set
            {
                _pRareStored = Mathf.Clamp(value, 0, 999999);
            }
        }
        private float _pEnergyStored;
        public float EnergyStored
        {
            get
            {
                return _pEnergyStored;
            }

            set
            {
                _pEnergyStored = Mathf.Clamp(value, 0, 999999);
            }
        }

        // trade infrastructure
        private int _pStarbaseLevel;
        public int StarbaseLevel
        {
            get
            {
                return _pStarbaseLevel;
            }

            set
            {
                _pStarbaseLevel = Mathf.Clamp(value, 0, 5);
            }
        }

        public int StarbaseCapacity
        {
            get
            {
                switch (StarbaseLevel)
                {
                    case 1:
                        {
                            return Constants.Constant.Level1StarbaseCapacity;                         
                        }
                    case 2:
                        {
                            return Constants.Constant.Level2StarbaseCapacity;                        
                        }
                    case 3:
                        {
                            return Constants.Constant.Level3StarbaseCapacity;                          
                        }
                    case 4:
                        {
                            return Constants.Constant.Level4StarbaseCapacity;                          
                        }
                    case 5:
                        {
                            return Constants.Constant.Level5StarbaseCapacity;                    
                        }
                    default:
                        {
                            return 0;
                        }
                }
            }
        }

        public float StarbaseCapacityRemaining
        {
            get
            {
                float baseCapacity = StarbaseCapacity;
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();

                foreach (TradeFleet tradeAg in gData.ActiveTradeFleets)
                {
                    if (tradeAg.ExportPlanetID == this.ID)
                    {
                        baseCapacity -= tradeAg.TotalSent;
                    }
                }

                if (baseCapacity < 0)
                {
                    baseCapacity = 0;
                }

                return baseCapacity;
                
            }
        }

        // production variables and functions
        private int overDriveMult;
        public int OverdriveMultiplier
        {
            get
            {
                return overDriveMult;
            }

            set
            {
                overDriveMult = Mathf.Clamp(value,1,10);
            }
        }
            // the amount of materials that are used in factories per turn (up to 10x) - normal is 1

        public float FactoriesOnline // get the total factories that are currently staffed
        {
            get
            {
                float facOnline = 0;
                foreach (Region rData in RegionList)
                {
                    facOnline += rData.FactoriesStaffed;
                }
                return facOnline;
            }
        }

        public float ProductionBasicMaterialsAllocated
        {
            get
            {
                float basicMaterialsRequested = 0; // what is the amount of materials needed to conform to overdrive/build plan?
                float basicMaterialsAllocated = 0; // what will actually be used?

                basicMaterialsRequested = FactoriesOnline * (1 * BuildPlan.OverdriveMultiplier);
               
                if (BasicPreProductionDifference > 0) // if there are alpha materials available
                {
                    if (basicMaterialsRequested < BasicPreProductionDifference)
                        basicMaterialsAllocated += basicMaterialsRequested;
                    else
                        basicMaterialsAllocated += BasicPreProductionDifference;                    
                }

                // if there is still a shortage after using any difference in production
                if (basicMaterialsAllocated < basicMaterialsRequested)
                {
                    if (BasicStored > (basicMaterialsRequested - basicMaterialsAllocated))
                    {
                        basicMaterialsAllocated += (basicMaterialsRequested - basicMaterialsAllocated);
                    }
                }
               
                return basicMaterialsAllocated;
            }
        }

        public float ProductionHeavyMaterialsAllocated
        {
            get
            {
                float heavyMaterialsRequested = 0; // what is the amount of materials needed to conform to overdrive/build plan?
                float heavyMaterialsAllocated = 0; // what will actually be used?

                heavyMaterialsRequested = FactoriesOnline * (.5f * BuildPlan.OverdriveMultiplier);

                if (HeavyPreProductionDifference > 0) // if there are alpha materials available
                {
                    if (heavyMaterialsRequested < HeavyPreProductionDifference)
                        heavyMaterialsAllocated += heavyMaterialsRequested;
                    else
                        heavyMaterialsAllocated += HeavyPreProductionDifference;
                }

                // if there is still a shortage after using any difference in production
                if (heavyMaterialsAllocated < heavyMaterialsRequested)
                {
                    if (HeavyStored > (heavyMaterialsRequested - heavyMaterialsAllocated))
                    {
                        heavyMaterialsAllocated += (heavyMaterialsRequested - heavyMaterialsAllocated);
                    }
                }
           
                return heavyMaterialsAllocated;
            }
        }

        public float ProductionRareMaterialsAllocated
        {
            get
            {
                float rareMaterialsRequested = 0; // what is the amount of materials needed to conform to overdrive/build plan?
                float rareMaterialsAllocated = 0; // what will actually be used?

                rareMaterialsRequested = FactoriesOnline * (.25f * BuildPlan.OverdriveMultiplier);

                if (RarePreProductionDifference > 0) // if there are alpha materials available
                {
                    if (rareMaterialsRequested < RarePreProductionDifference)
                        rareMaterialsAllocated += rareMaterialsRequested;
                    else
                        rareMaterialsAllocated += RarePreProductionDifference;
                }

                // if there is still a shortage after using any difference in production
                if (rareMaterialsAllocated < rareMaterialsRequested)
                {
                    if (RareStored > (rareMaterialsRequested - rareMaterialsAllocated))
                    {
                        rareMaterialsAllocated += (rareMaterialsRequested - rareMaterialsAllocated);
                    }
                }
                
                return rareMaterialsAllocated;
            }
        }

        public float PlanetEngineerRating
        {
            get
            {
                int engineersEmployed = 0;
                int engineerRatings = 0;

                foreach (Region rData in RegionList)
                {
                    if (rData.EmployedEngineers > 0)
                    {
                        foreach (Pops pop in rData.PopsInTile)
                        {
                            if (pop.Employment == Pops.ePopEmployment.FullyEmployed || pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            {
                                if (pop.PopClass == Pops.ePopClass.Engineer)
                                {
                                    engineerRatings += pop.ManufacturingSkill;
                                    engineersEmployed += 1; // add to the region count
                                }
                            }
                        }
                    }
                }

                if (engineersEmployed > 0)
                    return engineerRatings / engineersEmployed;
                else
                    return 1;
            }
        }

        public double BasicBPsGeneratedMonthly
        {
            get
            {
                // determine the number of materials used
                float materialsUsed = ProductionBasicMaterialsAllocated;
                if (FactoriesOnline == 0)
                    return 0;
                float worldEngineerRating = PlanetEngineerRating;
                float basicMaterialsUsedPerFactory = ProductionBasicMaterialsAllocated / FactoriesOnline;
                
                double engineerModifier = Math.Pow(worldEngineerRating / 25,2);
                double viceroyAptitudeModifier = 1 + Math.Pow(Viceroy.EngineeringAptitude / 150,2);
                double viceroyEngineeringSkillModifier =  1 + Math.Pow(Viceroy.Engineering / 150,2);
                double alphaBPsGenerated = ((engineerModifier * viceroyAptitudeModifier * viceroyEngineeringSkillModifier) * (Math.Log10(basicMaterialsUsedPerFactory + 1))) * FactoriesOnline * IndustrialMultiplier;
                
                return alphaBPsGenerated;
            }
        }

        public double HeavyBPsGeneratedMonthly
        {
            get
            {
                // determine the number of materials used
                float materialsUsed = ProductionHeavyMaterialsAllocated;
                if (FactoriesOnline == 0)
                    return 0;
                float worldEngineerRating = PlanetEngineerRating;
                float heavyMaterialsUsedPerFactory = ProductionHeavyMaterialsAllocated / FactoriesOnline;

                double engineerModifier = Math.Pow(worldEngineerRating / 30, 2);
                double viceroyAptitudeModifier = 1 + Math.Pow(Viceroy.EngineeringAptitude / 150, 2);
                double viceroyEngineeringSkillModifier = 1 + Math.Pow(Viceroy.Engineering / 150, 2);
                double heavyBPsGenerated = ((engineerModifier * viceroyAptitudeModifier * viceroyEngineeringSkillModifier) * (Math.Log10(heavyMaterialsUsedPerFactory + 1))) * FactoriesOnline * IndustrialMultiplier;

                return heavyBPsGenerated;
            }
        }

        public double RareBPsGeneratedMonthly
        {
            get
            {
                // determine the number of materials used
                float materialsUsed = ProductionRareMaterialsAllocated;
                if (FactoriesOnline == 0)
                    return 0;
                float worldEngineerRating = PlanetEngineerRating;
                float rareMaterialsUsedPerFactory = ProductionRareMaterialsAllocated / FactoriesOnline;

                double engineerModifier = Math.Pow(worldEngineerRating / 35, 2);
                double viceroyAptitudeModifier = 1 + Math.Pow(Viceroy.EngineeringAptitude / 150, 2);
                double viceroyEngineeringSkillModifier = 1 + Math.Pow(Viceroy.Engineering / 150, 2);
                double rareBPsGenerated = ((engineerModifier * viceroyAptitudeModifier * viceroyEngineeringSkillModifier) * (Math.Log10(rareMaterialsUsedPerFactory + 1))) * FactoriesOnline * IndustrialMultiplier;

                return rareBPsGenerated;
            }
        }

        public float UnemploymentLevel
        {
            get
            {
                float eligiblePops = 0;
                float workingPops = 0;

                foreach (Region rData in RegionList)
                {
                    if (rData.PopsInTile.Count > 0)
                    {
                        foreach (Pops pop in rData.PopsInTile)
                        {
                            if (pop.Type == Pops.ePopType.Worker)
                            {
                                eligiblePops += 1;
                                if (pop.Employment != Pops.ePopEmployment.Unemployed)
                                {
                                    workingPops += 1;
                                }
                            }
                        }
                    }
                }

                float uLevel = (workingPops / eligiblePops);
                return 1 - uLevel;
            }
        }

        public int PopulationChangeLastTurn
        {
            get
            {
                int change = 0;
                foreach (Region rData in RegionList)
                {
                    change += rData.PopulationChange;
                }

                return change;
            }
           
        }     

        public void SendTaxUpward()
        {
            float viceroyGPPTax = (PercentGPPForTax * GrossPlanetaryProduct) * .05f;
            float sysGovGPPTax = (PercentGPPForTax * GrossPlanetaryProduct) * .10f;
            float provGovGPPTax = (PercentGPPForTax * GrossPlanetaryProduct) * .15f;
            float empireGPPTax = (PercentGPPForTax * GrossPlanetaryProduct) * .75f;

            if (Viceroy != null)
            {
                Viceroy.Wealth += (int)viceroyGPPTax;
            }

            if (System.Governor != null)
            {
                System.Governor.Wealth += (int)sysGovGPPTax;
            }

            if (System.Province != null)
            {
                if (System.Province.Governor != null)
                {
                    System.Province.Governor.Wealth += (int)provGovGPPTax;
                }               
            }

            if (Owner != null)
            {
                if (empireGPPTax > 0)
                    Owner.Revenues += empireGPPTax;
            }
        }

        public List<TradeFleet> ActiveTradeFleets
        {
            get
            {
                List<TradeFleet> tradeFleets = new List<TradeFleet>();
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();

                foreach (TradeFleet tradeAg in gData.ActiveTradeFleets)
                {
                    if (tradeAg.ExportPlanetID == ID)
                        tradeFleets.Add(tradeAg);
                }

                return tradeFleets;
            }
        }
        public float FoodExportAvailable
        {
            get
            {
                float exportAvailable = (FoodDifference - DomesticFoodAvailable) * FoodExportPercentHold + (FoodStored * FoodStockpilePercentAvailable);
                return exportAvailable;
            }
        }
        public float EnergyExportAvailable
        {
            get
            {
                float exportAvailable = (EnergyDifference - DomesticEnergyAvailable) * EnergyExportPercentHold + (EnergyStored * EnergyStockpilePercentAvailable);
                return exportAvailable;
            }
        }

        public float BasicExportAvailable
        {
            get
            {
                float exportAvailable = AlphaTotalDifference * BasicExportPercentHold + (BasicStored * BasicStockpilePercentAvailable);
                return exportAvailable;
            }
        }

        public float HeavyExportAvailable
        {
            get
            {
                float exportAvailable = HeavyTotalDifference * HeavyExportPercentHold + (HeavyStored * HeavyStockpilePercentAvailable);
                return exportAvailable;
            }
        }

        public float RareExportAvailable
        {
            get
            {
                float exportAvailable = RareTotalDifference * RareExportPercentHold + (RareStored * RareStockpilePercentAvailable);
                return exportAvailable;
            }
        }

        public void UpdateEmployment()
        {
            foreach (Region rData in RegionList)
            {
                rData.UpdateEmployment();
            }
        }

        public void UpdatePopularSupport()
        {
            foreach (Region rData in RegionList)
            {
                rData.UpdatePopularSupport();
            }
        }

        public void UpdateBirthAndDeath()
        {
            foreach (Region rData in RegionList)
            {
                rData.UpdateBirthAndDeath();
            }
        }

        public void UpdateMigrationStatus()
        {
            foreach (Region rData in RegionList)
            {
                foreach (Pops p in rData.PopsInTile)
                {
                    if (p.PlanetHappinessLevel < 30)
                    {
                        int migrationChance = Random.Range(0, 100);
                        if (migrationChance + p.PlanetHappinessLevel + (100 - p.UnrestLevel) < Constants.Constant.MigrationTarget)
                        {
                            p.IsMigratingOffPlanet = true;
                        }
                    }
                }
            }
        }

        public bool StarvationOnPlanet { get; set; }
        public bool BlackoutsOnPlanet { get; set; }
        public int PopsStarvingOnPlanet
        {
            get
            {
                int starvingPops = 0;
                foreach (Region rData in RegionList)
                {
                    
                    starvingPops += rData.TotalStarvingPops;
                }

                return starvingPops;
            }
        }

        public int PopsBlackedOutOnPlanet
        {
            get
            {
                int blackedOutPops = 0;
                foreach (Region rData in RegionList)
                {
                    blackedOutPops += rData.TotalBlackedOutPops;
                }

                return blackedOutPops;
            }
        }

        public void UpdateShortfallConditions()
        {
            StarvationOnPlanet = false; // always assume false until proven otherwise
            BlackoutsOnPlanet = false;

            // if there is a food shortfall, check for starvation
            if (FoodDifference < 0)
            {
                if (FoodStored > Mathf.Abs(FoodDifference))
                {
                    FoodStored -= Mathf.Abs(FoodDifference); // remove the food from the stockpile if there is a shortfall
                }
                else
                {
                    foreach (Region rData in RegionList)
                    {
                        foreach (Pops pop in rData.PopsInTile)
                        {
                            int starvationChance = Random.Range(0, 100);
                            if (pop.IsStarving)
                            {
                                starvationChance += 30;
                            }

                            starvationChance += (int)(Mathf.Abs(FoodDifference) / RegionList.Count);

                            if (pop.Employment == Pops.ePopEmployment.Unemployed)
                            {
                                if (starvationChance > 85)
                                {
                                    pop.IsStarving = true; // pop is starving
                                }
                            }

                            if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            {
                                if (starvationChance > 92)
                                {
                                    pop.IsStarving = true; // pop is starving
                                }
                            }

                            if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            {
                                if (starvationChance > 97)
                                {
                                    pop.IsStarving = true; // pop is starving
                                }
                            }
                            if (pop.IsStarving) // any pop is starving, starvation flag effects
                                StarvationOnPlanet = true;
                        }
                    }
                }
            }
            else
            {
                foreach (Region rData in RegionList)
                {
                    foreach (Pops pop in rData.PopsInTile)
                    {
                        pop.IsStarving = false; // reset if no longer occurring
                    }
                }
            }    

            // check for energy difference
            if (EnergyDifference < 0)
            {
                if (EnergyStored > Mathf.Abs(EnergyDifference))
                {
                    EnergyStored -= Mathf.Abs(EnergyDifference); // remove the food from the stockpile if there is a shortfall
                }
                else
                {
                    foreach (Region rData in RegionList)
                    {
                        foreach (Pops pop in rData.PopsInTile)
                        {
                            int blackoutChance = Random.Range(0, 100);
                            if (pop.IsBlackedOut)
                            {
                                blackoutChance += 30;
                            }

                            blackoutChance += (int)(EnergyDifference / RegionList.Count);

                            if (pop.Employment == Pops.ePopEmployment.Unemployed)
                            {
                                if (blackoutChance > 80)
                                {
                                    pop.IsBlackedOut = true; // pop is suffering from blackouts
                                }
                            }

                            if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            {
                                if (blackoutChance > 90)
                                {
                                    pop.IsBlackedOut = true; // pop is suffering from blackouts
                                }
                            }

                            if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            {
                                if (blackoutChance > 96)
                                {
                                    pop.IsBlackedOut = true; // pop is suffering from blackouts
                                }
                            }
                            if (pop.IsBlackedOut)
                                BlackoutsOnPlanet = true;
                        }
                    }
                }
            }
            else
            {
                foreach (Region rData in RegionList)
                {
                    foreach (Pops pop in rData.PopsInTile)
                    {
                        pop.IsBlackedOut = false;
                    }
                }
            }
        }

        public void MigratePopsBetweenRegions()
        {
            foreach (Region r in RegionList)
            {
                foreach (Pops p in r.PopsInTile.ToArray())
                {
                    if (p.Employment == Pops.ePopEmployment.Unemployed) // || p.Employment == Pops.ePopEmployment.PartiallyEmployed)
                    {
                        switch (p.PopClass)
                        {
                            case Pops.ePopClass.Scientist:
                                break;
                            case Pops.ePopClass.Farmer:
                                foreach (Region reg in RegionList)
                                {
                                    if (reg.FarmsStaffed < reg.FarmingLevel)
                                    {
                                        if (MovePopBetweenRegions(p, r, reg))
                                        {
                                            reg.UpdateEmployment();
                                            break;
                                        }
                                    }
                                }
                                break;
                            case Pops.ePopClass.Miner:
                                foreach (Region reg in RegionList)
                                {
                                    if (reg.MinesStaffed < reg.MiningLevel)
                                    {
                                        if (MovePopBetweenRegions(p, r, reg))
                                        {
                                            reg.UpdateEmployment();
                                            break;
                                        }
                                    }
                                }
                                break;
                            case Pops.ePopClass.Engineer:
                                foreach (Region reg in RegionList)
                                {
                                    if (reg.FactoriesStaffed < reg.ManufacturingLevel)
                                    {
                                        if (MovePopBetweenRegions(p, r, reg))
                                        {
                                            reg.UpdateEmployment();
                                            break;
                                        }
                                        
                                    }
                                }
                                break;
                            case Pops.ePopClass.Fluxmen:
                                foreach (Region reg in RegionList)
                                {
                                    if (reg.HighTechFacilitiesStaffed < reg.HighTechLevel)
                                    {
                                        if (MovePopBetweenRegions(p, r, reg))
                                        {
                                            reg.UpdateEmployment();
                                            break;
                                        }
                                        
                                    }
                                }
                                break;
                            case Pops.ePopClass.Merchants:
                                break;
                            case Pops.ePopClass.Administrators:
                                foreach (Region reg in RegionList)
                                {
                                    if (reg.GovernmentFacilitiesStaffed < reg.GovernmentLevel)
                                    {
                                        if (MovePopBetweenRegions(p, r, reg))
                                        {
                                            reg.UpdateEmployment();
                                            break;
                                        }                                     
                                    }
                                }
                                break;
                            case Pops.ePopClass.None:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public bool MovePopBetweenRegions(Pops pop, Region oldRegion, Region newRegion)
        {
            if ((newRegion.MaxSafePopulationLevel * 2) > newRegion.PopsInTile.Count) // only move pops if there is room!
            {
                oldRegion.PopsInTile.Remove(pop);
                newRegion.PopsInTile.Add(pop);
                pop.RegionLocationID = newRegion.ID;
                //Debug.Log("A " + pop.PopClass.ToString().ToLower() + " from region " + oldRegion.ID + " moved to region " + newRegion.ID + ".");
                return true;
            }
            else
                return false;
        }

        // trade functions
       
        // planet traits/rank
        public ePlanetRank Rank { get; set; }
        public List<PlanetTraits> PlanetTraits = new List<PlanetTraits>();

        // percent towards the next construction level
        public float AlphaBPsUsed { get; set; }
        public float HeavyBPsUsed { get; set; }
        public float RareBPsUsed { get; set; }

        // structures built last turn
        public int FarmsBuiltLastTurn { get; set; }
        public int MinesBuiltLastTurn { get; set; }
        public int HighTechBuiltLastTurn { get; set; }
        public int FactoriesBuiltLastTurn { get; set; }
        public int InfraLevelsBuiltLastTurn { get; set; }

        // BPs allocated to each build type currently
        public float TotalAlphaBPsAllocatedFarms { get; set; }  
        public float TotalHeavyBPsAllocatedFarms { get; set; }
        public float TotalRareBPsAllocatedFarms { get; set; }
        public float PercentToNewFarmLevel { get; set; }
        public bool FarmLevelBuilt = false;

        public float TotalAlphaBPsAllocatedHighTech { get; set; }
        public float TotalHeavyBPsAllocatedHighTech { get; set; }
        public float TotalRareBPsAllocatedHighTech { get; set; }
        public float PercentToNewHighTechLevel { get; set; }
        public bool HighTechLevelBuilt = false;

        public float TotalAlphaBPsAllocatedFactory { get; set; }
        public float TotalHeavyBPsAllocatedFactory { get; set; }
        public float TotalRareBPsAllocatedFactory { get; set; }
        public float PercentToNewFactoryLevel { get; set; }
        public bool FactoryLevelBuilt = false;

        public float TotalAlphaBPsAllocatedMine { get; set; }
        public float TotalHeavyBPsAllocatedMine { get; set; }
        public float TotalRareBPsAllocatedMine { get; set; }
        public float PercentToNewMineLevel { get; set; }
        public bool MineLevelBuilt = false;

        public float TotalAlphaBPsAllocatedInfra { get; set; }
        public float TotalHeavyBPsAllocatedInfra { get; set; }
        public float TotalRareBPsAllocatedInfra { get; set; }
        public float PercentToNewInfraLevel { get; set; }
        public bool InfraLevelBuilt = false;
                    
        public float PercentToNewLabLevel { get; set; }
        public float PercentToNewAdminLevel { get; set; }    

        // function to build new levels
        public void ExecuteProductionPlan()
        {
            // reset BPs used
            AlphaBPsUsed = 0;
            HeavyBPsUsed = 0;
            RareBPsUsed = 0;

            // reset structures built last turn
            FarmsBuiltLastTurn = 0;
            MinesBuiltLastTurn = 0;
            HighTechBuiltLastTurn = 0;
            MinesBuiltLastTurn = 0;
            FactoriesBuiltLastTurn = 0;
            InfraLevelsBuiltLastTurn = 0;
            AllocateFarmProduction();
            AllocateHighTechProduction();
            AllocateFactoryProduction();
            AllocateMineProduction();
            AllocateInfraProduction(BuildPlan.TargetRegion);
        }

        private void AllocateFarmProduction()
        {
            bool alphaBPsReachedForFarm = false;
            bool heavyBPsReachedForFarm = false;
            bool rareBPsReachedForFarm = false;
            int alphaBPMultiplier = 0;
            int heavyBPMultiplier = 0;
            int rareBPMultiplier = 0;

            

            if (BasicBPsGeneratedMonthly > 0)
            {
                if (Constants.Constant.AlphaMaterialsPerFarmLevel <= TotalAlphaBPsAllocatedFarms)
                {
                    alphaBPsReachedForFarm = true;
                    alphaBPMultiplier = (int)(TotalAlphaBPsAllocatedFarms / Constants.Constant.AlphaMaterialsPerFarmLevel);
                }

                if (Constants.Constant.HeavyMaterialsPerFarmLevel <= TotalHeavyBPsAllocatedFarms)
                {
                    heavyBPsReachedForFarm = true;
                    heavyBPMultiplier = (int)(TotalHeavyBPsAllocatedFarms / Constants.Constant.HeavyMaterialsPerFarmLevel);
                    if (heavyBPMultiplier == 0)
                        heavyBPMultiplier = 1;
                }

                if (Constants.Constant.RareMaterialsPerFarmLevel <= TotalRareBPsAllocatedFarms)
                {
                    rareBPsReachedForFarm = true;
                    rareBPMultiplier = (int)(TotalRareBPsAllocatedFarms / Constants.Constant.RareMaterialsPerFarmLevel);
                    if (rareBPMultiplier == 0)
                        rareBPMultiplier = 1;
                }

                if (alphaBPsReachedForFarm && heavyBPsReachedForFarm && rareBPsReachedForFarm)
                {
                    // determine the max amount of levels that can be built at once
                    int totalLevels = alphaBPMultiplier;
                    FarmLevelBuilt = true;
                    if (heavyBPMultiplier < totalLevels)
                        totalLevels = heavyBPMultiplier;
                    if (rareBPMultiplier < totalLevels)
                        totalLevels = rareBPMultiplier;

                    // build those levels
                    AddNewLevels("Farm", totalLevels, null);

                    // reset the allocations
                    TotalAlphaBPsAllocatedFarms = 0;
                    TotalHeavyBPsAllocatedFarms = 0;
                    TotalRareBPsAllocatedFarms = 0;
                }

                // determine farms BPs
                if (!alphaBPsReachedForFarm)
                {
                    float previousTotal = TotalHeavyBPsAllocatedFarms;
                    TotalAlphaBPsAllocatedFarms += (float)(BasicBPsGeneratedMonthly * BuildPlan.FarmsAllocation);
                    if (TotalAlphaBPsAllocatedFarms > Constants.Constant.AlphaMaterialsPerFarmLevel)
                    {
                        AlphaBPsUsed = Constants.Constant.AlphaMaterialsPerFarmLevel - previousTotal;
                        TotalAlphaBPsAllocatedFarms = Constants.Constant.AlphaMaterialsPerFarmLevel;  
                    }
                    else
                    {
                        AlphaBPsUsed += (float)(BasicBPsGeneratedMonthly * BuildPlan.FarmsAllocation);
                    }
                   
                }

                if (!heavyBPsReachedForFarm)
                {
                    float previousTotal = TotalHeavyBPsAllocatedFarms;
                    TotalHeavyBPsAllocatedFarms += (float)(HeavyBPsGeneratedMonthly * BuildPlan.FarmsAllocation);
                    if (TotalHeavyBPsAllocatedFarms > Constants.Constant.HeavyMaterialsPerFarmLevel)
                    {
                        HeavyBPsUsed = Constants.Constant.HeavyMaterialsPerFarmLevel - previousTotal;
                        TotalHeavyBPsAllocatedFarms = Constants.Constant.HeavyMaterialsPerFarmLevel;  
                    }
                    else
                    {
                        HeavyBPsUsed += (float)(HeavyBPsGeneratedMonthly * BuildPlan.FarmsAllocation);                  
                    }                
                }

                if (!rareBPsReachedForFarm)
                {
                    float previousTotal = TotalRareBPsAllocatedFarms;
                    TotalRareBPsAllocatedFarms += (float)(RareBPsGeneratedMonthly * BuildPlan.FarmsAllocation);
                    if (TotalRareBPsAllocatedFarms > Constants.Constant.RareMaterialsPerFarmLevel)
                    {

                        RareBPsUsed = Constants.Constant.RareMaterialsPerFarmLevel - previousTotal;
                        TotalRareBPsAllocatedFarms = Constants.Constant.RareMaterialsPerFarmLevel;                      
                    }
                    else
                    {
                        RareBPsUsed += (float)(RareBPsGeneratedMonthly * BuildPlan.FarmsAllocation);
                    }
                }

                float TotalBPs = TotalAlphaBPsAllocatedFarms + TotalHeavyBPsAllocatedFarms + TotalRareBPsAllocatedFarms;
                float TotalBPsNeeded = Constants.Constant.AlphaMaterialsPerFarmLevel + Constants.Constant.RareMaterialsPerFarmLevel + Constants.Constant.HeavyMaterialsPerFarmLevel;

                if (FarmLevelBuilt)
                {
                    PercentToNewFarmLevel = 1f; // farm is built, so show 100%
                    FarmLevelBuilt = false; // reset
                }
                else
                {
                    PercentToNewFarmLevel = TotalBPs / TotalBPsNeeded;
                    if (PercentToNewFarmLevel > 1f)
                    {
                        PercentToNewFarmLevel = 1f;
                    }
                }
            }
        }

        private void AllocateHighTechProduction()
        {
            bool alphaBPsReachedForHighTech = false;
            bool heavyBPsReachedForHighTech = false;
            bool rareBPsReachedForHighTech = false;
            int alphaBPMultiplier = 0;
            int heavyBPMultiplier = 0;
            int rareBPMultiplier = 0;

            if (BasicBPsGeneratedMonthly > 0)
            {
                if (Constants.Constant.AlphaMaterialsPerHighTechLevel <= TotalAlphaBPsAllocatedHighTech)
                {
                    alphaBPsReachedForHighTech = true;
                    alphaBPMultiplier = (int)(TotalAlphaBPsAllocatedHighTech / Constants.Constant.AlphaMaterialsPerHighTechLevel);
                }

                if (Constants.Constant.HeavyMaterialsPerHighTechLevel <= TotalHeavyBPsAllocatedHighTech)
                {
                    heavyBPsReachedForHighTech = true;
                    heavyBPMultiplier = (int)(TotalHeavyBPsAllocatedHighTech / Constants.Constant.HeavyMaterialsPerHighTechLevel);
                    if (heavyBPMultiplier == 0)
                        heavyBPMultiplier = 1;
                }

                if (Constants.Constant.RareMaterialsPerHighTechLevel <= TotalRareBPsAllocatedHighTech)
                {
                    rareBPsReachedForHighTech = true;
                    rareBPMultiplier = (int)(TotalRareBPsAllocatedHighTech / Constants.Constant.RareMaterialsPerHighTechLevel);
                    if (rareBPMultiplier == 0)
                        rareBPMultiplier = 1;
                }

                if (alphaBPsReachedForHighTech && heavyBPsReachedForHighTech && rareBPsReachedForHighTech)
                {
                    // determine the max amount of levels that can be built at once
                    int totalLevels = alphaBPMultiplier;
                    HighTechLevelBuilt = true;
                    if (heavyBPMultiplier < totalLevels)
                        totalLevels = heavyBPMultiplier;
                    if (rareBPMultiplier < totalLevels)
                        totalLevels = rareBPMultiplier;

                    // build those levels
                    AddNewLevels("Hightech", totalLevels, null);

                    // reset the allocations
                    TotalAlphaBPsAllocatedHighTech = 0;
                    TotalHeavyBPsAllocatedHighTech = 0;
                    TotalRareBPsAllocatedHighTech = 0;
                }

                // determine high tech BPs
                if (!alphaBPsReachedForHighTech)
                {
                    float previousTotal = TotalHeavyBPsAllocatedHighTech;
                    TotalAlphaBPsAllocatedHighTech += (float)(BasicBPsGeneratedMonthly * BuildPlan.HighTechAllocation);
                    if (TotalAlphaBPsAllocatedHighTech > Constants.Constant.AlphaMaterialsPerHighTechLevel)
                    {
                        AlphaBPsUsed = Constants.Constant.AlphaMaterialsPerHighTechLevel - previousTotal;
                        TotalAlphaBPsAllocatedHighTech = Constants.Constant.AlphaMaterialsPerHighTechLevel;
                    }
                    else
                    {
                        AlphaBPsUsed += (float)(BasicBPsGeneratedMonthly * BuildPlan.HighTechAllocation);
                    }

                }

                if (!heavyBPsReachedForHighTech)
                {
                    float previousTotal = TotalHeavyBPsAllocatedHighTech;
                    TotalHeavyBPsAllocatedHighTech += (float)(HeavyBPsGeneratedMonthly * BuildPlan.HighTechAllocation);
                    if (TotalHeavyBPsAllocatedHighTech > Constants.Constant.HeavyMaterialsPerHighTechLevel)
                    {
                        HeavyBPsUsed = Constants.Constant.HeavyMaterialsPerHighTechLevel - previousTotal;
                        TotalHeavyBPsAllocatedHighTech = Constants.Constant.HeavyMaterialsPerHighTechLevel;
                    }
                    else
                    {
                        HeavyBPsUsed += (float)(HeavyBPsGeneratedMonthly * BuildPlan.HighTechAllocation);
                    }
                }

                if (!rareBPsReachedForHighTech)
                {
                    float previousTotal = TotalRareBPsAllocatedHighTech;
                    TotalRareBPsAllocatedHighTech += (float)(RareBPsGeneratedMonthly * BuildPlan.HighTechAllocation);
                    if (TotalRareBPsAllocatedHighTech > Constants.Constant.RareMaterialsPerHighTechLevel)
                    {

                        RareBPsUsed = Constants.Constant.RareMaterialsPerHighTechLevel - previousTotal;
                        TotalRareBPsAllocatedHighTech = Constants.Constant.RareMaterialsPerHighTechLevel;
                    }
                    else
                    {
                        RareBPsUsed += (float)(RareBPsGeneratedMonthly * BuildPlan.HighTechAllocation);
                    }
                }

                float TotalBPs = TotalAlphaBPsAllocatedHighTech + TotalHeavyBPsAllocatedHighTech + TotalRareBPsAllocatedHighTech;
                float TotalBPsNeeded = Constants.Constant.AlphaMaterialsPerHighTechLevel + Constants.Constant.RareMaterialsPerHighTechLevel + Constants.Constant.HeavyMaterialsPerHighTechLevel;

                if (HighTechLevelBuilt)
                {
                    PercentToNewHighTechLevel = 1f; // farm is built, so show 100%
                    HighTechLevelBuilt = false; // reset
                }
                else
                {
                    PercentToNewHighTechLevel = TotalBPs / TotalBPsNeeded;
                    if (PercentToNewHighTechLevel > 1f)
                    {
                        PercentToNewHighTechLevel = 1f;
                    }
                }
            }
        }

        private void AllocateFactoryProduction()
        {
            bool alphaBPsReachedForFactory = false;
            bool heavyBPsReachedForFactory = false;
            bool rareBPsReachedForFactory = false;
            int alphaBPMultiplier = 0;
            int heavyBPMultiplier = 0;
            int rareBPMultiplier = 0;

            

            if (BasicBPsGeneratedMonthly > 0)
            {
                if (Constants.Constant.AlphaMaterialsPerFactoryLevel <= TotalAlphaBPsAllocatedFactory)
                {
                    alphaBPsReachedForFactory = true;
                    alphaBPMultiplier = (int)(TotalAlphaBPsAllocatedFactory / Constants.Constant.AlphaMaterialsPerFactoryLevel);
                }

                if (Constants.Constant.HeavyMaterialsPerFactoryLevel <= TotalHeavyBPsAllocatedFactory)
                {
                    heavyBPsReachedForFactory = true;
                    heavyBPMultiplier = (int)(TotalHeavyBPsAllocatedFactory / Constants.Constant.HeavyMaterialsPerFactoryLevel);
                    if (heavyBPMultiplier == 0)
                        heavyBPMultiplier = 1;
                }

                if (Constants.Constant.RareMaterialsPerFactoryLevel <= TotalRareBPsAllocatedFactory)
                {
                    rareBPsReachedForFactory = true;
                    rareBPMultiplier = (int)(TotalRareBPsAllocatedFactory / Constants.Constant.RareMaterialsPerFactoryLevel);
                    if (rareBPMultiplier == 0)
                        rareBPMultiplier = 1;
                }

                if (alphaBPsReachedForFactory && heavyBPsReachedForFactory && rareBPsReachedForFactory)
                {
                    // determine the max amount of levels that can be built at once
                    int totalLevels = alphaBPMultiplier;
                    FactoryLevelBuilt = true;
                    if (heavyBPMultiplier < totalLevels)
                        totalLevels = heavyBPMultiplier;
                    if (rareBPMultiplier < totalLevels)
                        totalLevels = rareBPMultiplier;

                    // build those levels
                    AddNewLevels("Factory", totalLevels, null);

                    // reset the allocations
                    TotalAlphaBPsAllocatedFactory = 0;
                    TotalHeavyBPsAllocatedFactory = 0;
                    TotalRareBPsAllocatedFactory = 0;
                }

                // determine high tech BPs
                if (!alphaBPsReachedForFactory)
                {
                    float previousTotal = TotalHeavyBPsAllocatedFactory;
                    TotalAlphaBPsAllocatedFactory += (float)(BasicBPsGeneratedMonthly * BuildPlan.FactoryAllocation);
                    if (TotalAlphaBPsAllocatedFactory > Constants.Constant.AlphaMaterialsPerFactoryLevel)
                    {
                        AlphaBPsUsed = Constants.Constant.AlphaMaterialsPerFactoryLevel - previousTotal;
                        TotalAlphaBPsAllocatedFactory = Constants.Constant.AlphaMaterialsPerFactoryLevel;
                    }
                    else
                    {
                        AlphaBPsUsed += (float)(BasicBPsGeneratedMonthly * BuildPlan.FactoryAllocation);
                    }

                }

                if (!heavyBPsReachedForFactory)
                {
                    float previousTotal = TotalHeavyBPsAllocatedFactory;
                    TotalHeavyBPsAllocatedFactory += (float)(HeavyBPsGeneratedMonthly * BuildPlan.FactoryAllocation);
                    if (TotalHeavyBPsAllocatedFactory > Constants.Constant.HeavyMaterialsPerFactoryLevel)
                    {
                        HeavyBPsUsed = Constants.Constant.HeavyMaterialsPerFactoryLevel - previousTotal;
                        TotalHeavyBPsAllocatedFactory = Constants.Constant.HeavyMaterialsPerFactoryLevel;
                    }
                    else
                    {
                        HeavyBPsUsed += (float)(HeavyBPsGeneratedMonthly * BuildPlan.FactoryAllocation);
                    }
                }

                if (!rareBPsReachedForFactory)
                {
                    float previousTotal = TotalRareBPsAllocatedFactory;
                    TotalRareBPsAllocatedFactory += (float)(RareBPsGeneratedMonthly * BuildPlan.FactoryAllocation);
                    if (TotalRareBPsAllocatedFactory > Constants.Constant.RareMaterialsPerFactoryLevel)
                    {

                        RareBPsUsed = Constants.Constant.RareMaterialsPerFactoryLevel - previousTotal;
                        TotalRareBPsAllocatedFactory = Constants.Constant.RareMaterialsPerFactoryLevel;
                    }
                    else
                    {
                        RareBPsUsed += (float)(RareBPsGeneratedMonthly * BuildPlan.FactoryAllocation);
                    }
                }

                float TotalBPs = TotalAlphaBPsAllocatedFactory + TotalHeavyBPsAllocatedFactory + TotalRareBPsAllocatedFactory;
                float TotalBPsNeeded = Constants.Constant.AlphaMaterialsPerFactoryLevel + Constants.Constant.RareMaterialsPerFactoryLevel + Constants.Constant.HeavyMaterialsPerFactoryLevel;

                if (FactoryLevelBuilt)
                {
                    PercentToNewFactoryLevel = 1f; // farm is built, so show 100%
                    FactoryLevelBuilt = false; // reset
                }
                else
                {
                    PercentToNewFactoryLevel = TotalBPs / TotalBPsNeeded;
                    if (PercentToNewFactoryLevel > 1f)
                    {
                        PercentToNewFactoryLevel = 1f;
                    }
                }
            }
        }

        private void AllocateMineProduction()
        {
            bool alphaBPsReachedForMine = false;
            bool heavyBPsReachedForMine = false;
            bool rareBPsReachedForMine = false;
            int alphaBPMultiplier = 0;
            int heavyBPMultiplier = 0;
            int rareBPMultiplier = 0;

            
            if (BasicBPsGeneratedMonthly > 0)
            {
                if (Constants.Constant.AlphaMaterialsPerMineLevel <= TotalAlphaBPsAllocatedMine)
                {
                    alphaBPsReachedForMine = true;
                    alphaBPMultiplier = (int)(TotalAlphaBPsAllocatedMine / Constants.Constant.AlphaMaterialsPerMineLevel);
                }

                if (Constants.Constant.HeavyMaterialsPerMineLevel <= TotalHeavyBPsAllocatedMine)
                {
                    heavyBPsReachedForMine = true;
                    heavyBPMultiplier = (int)(TotalHeavyBPsAllocatedMine / Constants.Constant.HeavyMaterialsPerMineLevel);
                    if (heavyBPMultiplier == 0)
                        heavyBPMultiplier = 1;
                }

                if (Constants.Constant.RareMaterialsPerMineLevel <= TotalRareBPsAllocatedMine)
                {
                    rareBPsReachedForMine = true;
                    rareBPMultiplier = (int)(TotalRareBPsAllocatedMine / Constants.Constant.RareMaterialsPerMineLevel);
                    if (rareBPMultiplier == 0)
                        rareBPMultiplier = 1;
                }

                if (alphaBPsReachedForMine && heavyBPsReachedForMine && rareBPsReachedForMine)
                {
                    // determine the max amount of levels that can be built at once
                    int totalLevels = alphaBPMultiplier;
                    MineLevelBuilt = true;
                    if (heavyBPMultiplier < totalLevels)
                        totalLevels = heavyBPMultiplier;
                    if (rareBPMultiplier < totalLevels)
                        totalLevels = rareBPMultiplier;

                    // build those levels
                    AddNewLevels("Mine", totalLevels, null);

                    // reset the allocations
                    TotalAlphaBPsAllocatedMine = 0;
                    TotalHeavyBPsAllocatedMine = 0;
                    TotalRareBPsAllocatedMine = 0;
                }

                // determine high tech BPs
                if (!alphaBPsReachedForMine)
                {
                    float previousTotal = TotalHeavyBPsAllocatedMine;
                    TotalAlphaBPsAllocatedMine += (float)(BasicBPsGeneratedMonthly * BuildPlan.MineAllocation);
                    if (TotalAlphaBPsAllocatedMine > Constants.Constant.AlphaMaterialsPerMineLevel)
                    {
                        AlphaBPsUsed = Constants.Constant.AlphaMaterialsPerMineLevel - previousTotal;
                        TotalAlphaBPsAllocatedMine = Constants.Constant.AlphaMaterialsPerMineLevel;
                    }
                    else
                    {
                        AlphaBPsUsed += (float)(BasicBPsGeneratedMonthly * BuildPlan.MineAllocation);
                    }

                }

                if (!heavyBPsReachedForMine)
                {
                    float previousTotal = TotalHeavyBPsAllocatedMine;
                    TotalHeavyBPsAllocatedMine += (float)(HeavyBPsGeneratedMonthly * BuildPlan.MineAllocation);
                    if (TotalHeavyBPsAllocatedMine > Constants.Constant.HeavyMaterialsPerMineLevel)
                    {
                        HeavyBPsUsed = Constants.Constant.HeavyMaterialsPerMineLevel - previousTotal;
                        TotalHeavyBPsAllocatedMine = Constants.Constant.HeavyMaterialsPerMineLevel;
                    }
                    else
                    {
                        HeavyBPsUsed += (float)(HeavyBPsGeneratedMonthly * BuildPlan.MineAllocation);
                    }
                }

                if (!rareBPsReachedForMine)
                {
                    float previousTotal = TotalRareBPsAllocatedMine;
                    TotalRareBPsAllocatedMine += (float)(RareBPsGeneratedMonthly * BuildPlan.MineAllocation);
                    if (TotalRareBPsAllocatedMine > Constants.Constant.RareMaterialsPerMineLevel)
                    {

                        RareBPsUsed = Constants.Constant.RareMaterialsPerMineLevel - previousTotal;
                        TotalRareBPsAllocatedMine = Constants.Constant.RareMaterialsPerMineLevel;
                    }
                    else
                    {
                        RareBPsUsed += (float)(RareBPsGeneratedMonthly * BuildPlan.MineAllocation);
                    }
                }

                float TotalBPs = TotalAlphaBPsAllocatedMine + TotalHeavyBPsAllocatedMine + TotalRareBPsAllocatedMine;
                float TotalBPsNeeded = Constants.Constant.AlphaMaterialsPerMineLevel + Constants.Constant.RareMaterialsPerMineLevel + Constants.Constant.HeavyMaterialsPerMineLevel;

                if (MineLevelBuilt)
                {
                    PercentToNewMineLevel = 1f; // farm is built, so show 100%
                    MineLevelBuilt = false; // reset
                }
                else
                {
                    PercentToNewMineLevel = TotalBPs / TotalBPsNeeded;
                    if (PercentToNewMineLevel > 1f)
                    {
                        PercentToNewMineLevel = 1f;
                    }
                }
            }
        }

        private void AllocateInfraProduction(Region targetRegion)
        {
            if (targetRegion == null)
                return;

            bool alphaBPsReachedForInfra = false;
            bool heavyBPsReachedForInfra = false;
            bool rareBPsReachedForInfra = false;
            int alphaBPMultiplier = 0;
            int heavyBPMultiplier = 0;
            int rareBPMultiplier = 0;
            
            float AlphaMaterialsRequired = Constant.AlphaMaterialsPerInfraLevel * (100 / AdjustedBio) * Mathf.Pow(targetRegion.HabitatationInfrastructureLevel,3);
            float HeavyMaterialsRequired = Constant.HeavyMaterialsPerInfraLevel * (100 / AdjustedBio) * Mathf.Pow(targetRegion.HabitatationInfrastructureLevel, 3);
            float RareMaterialsRequired = Constant.RareMaterialsPerInfraLevel * (100 / AdjustedBio) * Mathf.Pow(targetRegion.HabitatationInfrastructureLevel, 3);

            if (BasicBPsGeneratedMonthly > 0)
            {
                if (AlphaMaterialsRequired * (100 / AdjustedBio) <= targetRegion.TotalAlphaBPsAllocatedInfra)
                {
                    alphaBPsReachedForInfra = true;
                    alphaBPMultiplier = (int)(targetRegion.TotalAlphaBPsAllocatedInfra / AlphaMaterialsRequired);
                }

                if (HeavyMaterialsRequired <= targetRegion.TotalHeavyBPsAllocatedInfra)
                {
                    heavyBPsReachedForInfra = true;
                    heavyBPMultiplier = (int)(targetRegion.TotalHeavyBPsAllocatedInfra / HeavyMaterialsRequired);
                    if (heavyBPMultiplier == 0)
                        heavyBPMultiplier = 1;
                }

                if (RareMaterialsRequired <= targetRegion.TotalRareBPsAllocatedInfra)
                {
                    rareBPsReachedForInfra = true;
                    rareBPMultiplier = (int)(targetRegion.TotalRareBPsAllocatedInfra / RareMaterialsRequired);
                    if (rareBPMultiplier == 0)
                        rareBPMultiplier = 1;
                }

                if (alphaBPsReachedForInfra && heavyBPsReachedForInfra && rareBPsReachedForInfra)
                {
                    // determine the max amount of levels that can be built at once
                    int totalLevels = alphaBPMultiplier;
                    InfraLevelBuilt = true;
                    if (heavyBPMultiplier < totalLevels)
                        totalLevels = heavyBPMultiplier;
                    if (rareBPMultiplier < totalLevels)
                        totalLevels = rareBPMultiplier;

                    // build those levels
                    AddNewLevels("Infra", totalLevels, targetRegion);

                    // reset the allocations
                    targetRegion.TotalAlphaBPsAllocatedInfra = 0;
                    targetRegion.TotalHeavyBPsAllocatedInfra = 0;
                    targetRegion.TotalRareBPsAllocatedInfra = 0;
                }

                // determine Infras BPs
                if (!alphaBPsReachedForInfra)
                {
                    float previousTotal = TotalHeavyBPsAllocatedInfra;
                    targetRegion.TotalAlphaBPsAllocatedInfra += (float)(BasicBPsGeneratedMonthly * BuildPlan.InfraAllocation);
                    if (targetRegion.TotalAlphaBPsAllocatedInfra > AlphaMaterialsRequired)
                    {
                        AlphaBPsUsed = AlphaMaterialsRequired - previousTotal;
                        targetRegion.TotalAlphaBPsAllocatedInfra = AlphaMaterialsRequired;
                    }
                    else
                    {
                        AlphaBPsUsed += (float)(BasicBPsGeneratedMonthly * BuildPlan.InfraAllocation);
                    }

                }

                if (!heavyBPsReachedForInfra)
                {
                    float previousTotal = TotalHeavyBPsAllocatedInfra;
                    targetRegion.TotalHeavyBPsAllocatedInfra += (float)(HeavyBPsGeneratedMonthly * BuildPlan.InfraAllocation);
                    if (targetRegion.TotalHeavyBPsAllocatedInfra > HeavyMaterialsRequired)
                    {
                        HeavyBPsUsed = HeavyMaterialsRequired - previousTotal;
                        targetRegion.TotalHeavyBPsAllocatedInfra = HeavyMaterialsRequired;
                    }
                    else
                    {
                        HeavyBPsUsed += (float)(HeavyBPsGeneratedMonthly * BuildPlan.InfraAllocation);
                    }
                }

                if (!rareBPsReachedForInfra)
                {
                    float previousTotal = targetRegion.TotalRareBPsAllocatedInfra;
                    targetRegion.TotalRareBPsAllocatedInfra += (float)(RareBPsGeneratedMonthly * BuildPlan.InfraAllocation);
                    if (targetRegion.TotalRareBPsAllocatedInfra > RareMaterialsRequired)
                    {

                        RareBPsUsed = RareMaterialsRequired - previousTotal;
                        targetRegion.TotalRareBPsAllocatedInfra = RareMaterialsRequired;
                    }
                    else
                    {
                        RareBPsUsed += (float)(RareBPsGeneratedMonthly * BuildPlan.InfraAllocation);
                    }
                }

                float TotalBPs = targetRegion.TotalAlphaBPsAllocatedInfra + targetRegion.TotalHeavyBPsAllocatedInfra + targetRegion.TotalRareBPsAllocatedInfra;
                float TotalBPsNeeded = AlphaMaterialsRequired + HeavyMaterialsRequired + RareMaterialsRequired;

                if (InfraLevelBuilt)
                {
                    PercentToNewInfraLevel = 1f; // Infra is built, so show 100%
                    InfraLevelBuilt = false; // reset
                }
                else
                {
                    PercentToNewInfraLevel = TotalBPs / TotalBPsNeeded;
                    if (PercentToNewInfraLevel > 1f)
                    {
                        PercentToNewInfraLevel = 1f;
                    }
                }
            }
        }

        private void AddNewLevels(string levType, int levels, Region targetRegion)
        {
            for (int x = 0; x < levels; x++)
            {
                // determine all eligible regions on the planet (not overpopulated)
                List<string> regionsEligible = new List<string>();
                foreach (Region rData in RegionList)
                {
                    if (rData.TotalDevelopmentLevel < rData.MaxDevelopmentLevel && levType != "Infra" && rData.IsHabitable)
                    {
                        regionsEligible.Add(rData.ID);
                    }
                    else
                    {
                        if (rData.IsHabitable)
                            regionsEligible.Add(rData.ID);
                    }
                }
                
                // then choose a region from this list
                int regionChoice = Random.Range(0, regionsEligible.Count);
                if (levType == "Farm")
                {              
                    HelperFunctions.DataRetrivalFunctions.GetRegion(regionsEligible[regionChoice]).FarmingLevel += 1;
                    FarmsBuiltLastTurn += 1;
                }

                if (levType == "Hightech")
                {
                    HelperFunctions.DataRetrivalFunctions.GetRegion(regionsEligible[regionChoice]).HighTechLevel += 1;
                    HighTechBuiltLastTurn += 1;
                }

                if (levType == "Factory")
                {
                    HelperFunctions.DataRetrivalFunctions.GetRegion(regionsEligible[regionChoice]).ManufacturingLevel += 1;
                    FactoriesBuiltLastTurn += 1;
                }

                if (levType == "Mine")
                {
                    HelperFunctions.DataRetrivalFunctions.GetRegion(regionsEligible[regionChoice]).MiningLevel += 1;
                    MinesBuiltLastTurn += 1;
                }

                if (levType == "Infra")
                {
                    targetRegion.HabitatationInfrastructureLevel += 1;
                    InfraLevelsBuiltLastTurn += 1;
                }
            }
        }

        public int TotalMiners
        {
            get
            {
                int _pMiners = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Miner)
                        {
                            _pMiners += 1;
                        }
                    }
                }

                return _pMiners;
            }
        }

        public int TotalFarmers
        {
            get
            {
                int _pFarmers = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Farmer)
                        {
                            _pFarmers += 1;
                        }
                    }
                }

                return _pFarmers;
            }
        }

        public int TotalBuilders
        {
            get
            {
                int _pBuilders = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Fluxmen)
                        {
                            _pBuilders += 1;
                        }
                    }
                }

                return _pBuilders;
            }
        }

        public int TotalEngineers
        {
            get
            {
                int _pEngineers = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Engineer)
                        {
                            _pEngineers += 1;
                        }
                    }
                }

                return _pEngineers;
            }
        }

        public int TotalScientists
        {
            get
            {
                int _pScientists = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Scientist)
                        {
                            _pScientists += 1;
                        }
                    }
                }

                return _pScientists;
            }
        }

        public int TotalMerchants
        {
            get
            {
                int _pMerchants = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Merchants)
                        {
                            _pMerchants += 1;
                        }
                    }
                }

                return _pMerchants;
            }
        }

        public int AverageMerchantSkill
        {
            get
            {
                int _pSkill = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops p in r.PopsInTile)
                    {
                        if (p.PopClass == Pops.ePopClass.Merchants)
                        {
                            _pSkill += p.MerchantSkill;
                        }
                    }
                }

                if (TotalMerchants > 0)
                    return _pSkill / TotalMerchants;
                else
                    return 0;
            }
        }

        public int TotalAdmin
        {
            get
            {
                int _pAdmin = 0;
                foreach (Region r in RegionList)
                {
                    _pAdmin += r.GovernmentLevel;
                }

                return _pAdmin;
            }
        }

        public int TotalPopulation
        {
            get
            {
                int _pPop = 0;
                foreach (Region r in RegionList)
                {
                    _pPop += r.PopsInTile.Count;
                }
                return _pPop;
            }
        }

        public float TotalEnergyGenerated
        {
            get
            {
                float energy = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        energy += r.EnergyPerTile;
                }
                return energy;
            }
      
        }

        public float TotalEnergyConsumed
        {
            get
            {
                float energy = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        energy += r.EnergyUsedPerTile;
                }
                return energy;
            }
        }

        public float TotalFoodGenerated
        {
            get
            {
                float food = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        food += r.FoodPerTile;
                }
                return food;
            }

        }

        public float TotalFoodConsumed
        {
            get
            {
                float foodUsed = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        foodUsed += r.FoodUsedPerTile;
                }
                return foodUsed;
            }
        }

        public float TotalAlphaMaterialsGenerated
        {
            get
            {
                float alpha = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        alpha += r.BasicMaterialsPerTile;
                }
                return alpha;
            }

        }

        public float TotalAlphaMaterialsConsumed
        {
            get
            {
                float alphaUsed = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        alphaUsed += r.AlphaUsedPerTile;
                }

                return alphaUsed;
            }
        }

        public float TotalHeavyMaterialsGenerated
        {
            get
            {
                float heavy = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        heavy += r.HeavyPerTile;
                }
                return heavy;
            }

        }

        public float TotalHeavyMaterialsConsumed
        {
            get
            {
                float heavyUsed = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        heavyUsed += r.HeavyUsedPerTile;
                }
                return heavyUsed;
            }
        }

        public float TotalRareMaterialsGenerated
        {
            get
            {
                float rare = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        rare += r.RarePerTile;
                }
                return rare;
            }

        }

        public float TotalRareMaterialsConsumed
        {
            get
            {
                float rareUsed = 0f;
                foreach (Region r in RegionList)
                {
                    if (r.OwnerCivID == "CIV0") // only add to total if the player controls the region
                        rareUsed += r.RareUsedPerTile;
                }
                return rareUsed;
            }
        }

        public float EnergyDifference
        {
            get
            {
                return TotalEnergyGenerated - TotalEnergyConsumed + EnergyImported;
            }
        }

        public float FoodDifference
        {
            get
            {
                return TotalFoodGenerated - TotalFoodConsumed + FoodImported;
            }
        }

        public float BasicPreProductionDifference
        {
            get
            {
                return TotalAlphaMaterialsGenerated - TotalAlphaMaterialsConsumed + AlphaImported;
            }
        }

        public float AlphaTotalDifference
        {
            get
            {
                return BasicPreProductionDifference - ProductionBasicMaterialsAllocated;
            }
        }

        public float HeavyPreProductionDifference
        {
            get
            {
                return TotalHeavyMaterialsGenerated - TotalHeavyMaterialsConsumed + HeavyImported;
            }
        }

        public float HeavyTotalDifference
        {
            get
            {
                return HeavyPreProductionDifference - ProductionHeavyMaterialsAllocated;
            }
        }

        public float RarePreProductionDifference
        {
            get
            {
                return TotalRareMaterialsGenerated - TotalRareMaterialsConsumed + RareImported;
            }
        }

        public float RareTotalDifference
        {
            get
            {
                return RarePreProductionDifference - ProductionRareMaterialsAllocated;
            }
        }

        public float FoodImported { get; set; }
        public float FoodExported { get; set; }

        public float FoodTrade
        {
            get
            {
                return FoodImported - FoodExported;
            }
        }

        public float EnergyTrade
        {
            get
            {
                return EnergyImported - EnergyExported;
            }
        }

        public float AlphaTrade
        {
            get
            {
                return AlphaImported - AlphaExported;
            }
        }

        public float HeavyTrade
        {
            get
            {
                return HeavyImported - HeavyExported;
            }
        }

        public float RareTrade
        {
            get
            {
                return RareImported - RareExported;
            }
        }

        public float EnergyImported { get; set; }


        public float EnergyExported { get; set; }
        

        public float AlphaImported
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                float totalAlpha = 0f;

                if (gData.ActiveTradeFleets.Exists(p => p.ImportPlanetID == this.ID))
                {
                    List<TradeFleet> expList = gData.ActiveTradeFleets.FindAll(p => p.ImportPlanetID == this.ID);
                    foreach (TradeFleet t in expList)
                    {
                        if (t.BasicOnBoard > 0)
                            totalAlpha += t.BasicOnBoard;
                    }
                }
                return totalAlpha;
            }
        }

        public float AlphaExported
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                float totalAlpha = 0f;

                if (gData.ActiveTradeFleets.Exists(p => p.ExportPlanetID == this.ID))
                {
                    List<TradeFleet> expList = gData.ActiveTradeFleets.FindAll(p => p.ExportPlanetID == this.ID);
                    foreach (TradeFleet t in expList)
                    {
                        if (t.BasicOnBoard > 0)
                            totalAlpha += t.BasicOnBoard;
                    }
                }
                return totalAlpha;
            }
        }

        public float HeavyImported
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                float totalHeavy = 0f;

                if (gData.ActiveTradeFleets.Exists(p => p.ImportPlanetID == this.ID))
                {
                    List<TradeFleet> expList = gData.ActiveTradeFleets.FindAll(p => p.ImportPlanetID == this.ID);
                    foreach (TradeFleet t in expList)
                    {
                        if (t.HeavyOnBoard > 0)
                            totalHeavy += t.HeavyOnBoard;
                    }
                }
                return totalHeavy;
            }
        }

        public float HeavyExported
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                float totalHeavy = 0f;

                if (gData.ActiveTradeFleets.Exists(p => p.ExportPlanetID == this.ID))
                {
                    List<TradeFleet> expList = gData.ActiveTradeFleets.FindAll(p => p.ExportPlanetID == this.ID);
                    foreach (TradeFleet t in expList)
                    {
                        if (t.HeavyOnBoard > 0)
                            totalHeavy += t.HeavyOnBoard;
                    }
                }
                return totalHeavy;
            }
        }

        public float RareImported
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                float totalRare = 0f;

                if (gData.ActiveTradeFleets.Exists(p => p.ImportPlanetID == this.ID))
                {
                    List<TradeFleet> expList = gData.ActiveTradeFleets.FindAll(p => p.ImportPlanetID == this.ID);
                    foreach (TradeFleet t in expList)
                    {
                        if (t.RareOnBoard > 0)
                            totalRare += t.RareOnBoard;
                    }
                }
                return totalRare;
            }
        }

        public float RareExported
        {
            get
            {
                GameData gData = GameObject.Find("GameManager").GetComponent<GameData>();
                float totalRare = 0f;

                if (gData.ActiveTradeFleets.Exists(p => p.ExportPlanetID == this.ID))
                {
                    List<TradeFleet> expList = gData.ActiveTradeFleets.FindAll(p => p.ExportPlanetID == this.ID);
                    foreach (TradeFleet t in expList)
                    {
                        if (t.RareOnBoard > 0)
                            totalRare += t.RareOnBoard;
                    }
                }
                return totalRare;
            }
        }

        public int ManufacturingLevel
        {
            get
            {
                int _pMan = 0;
                foreach (Region r in RegionList)
                {
                    _pMan += r.ManufacturingLevel;
                }
                return _pMan;
            }
        }

        public float FactoriesStaffed
        {
            get
            {
                float _pStaff = 0;
                foreach (Region r in RegionList)
                {
                    _pStaff += r.FactoriesStaffed;
                }
                return _pStaff;
            }
        }

        public int FarmingLevel
        {
            get
            {
                int _pFarm = 0;
                foreach (Region r in RegionList)
                {
                    _pFarm += r.FarmingLevel;
                }
                return _pFarm;
            }
        }

        public float FarmsStaffed
        {
            get
            {
                float _pFarm = 0;
                foreach (Region r in RegionList)
                {
                    _pFarm += r.FarmsStaffed;
                }
                return _pFarm;
            }
        }

        public int HighTechLevel
        {
            get
            {
                int _pHTech = 0;
                foreach (Region r in RegionList)
                {
                    _pHTech += r.HighTechLevel;
                }
                return _pHTech;
            }
        }

        public float HighTechFacilitiesStaffed
        {
            get
            {
                float _pFarm = 0;
                foreach (Region r in RegionList)
                {
                    _pFarm += r.HighTechFacilitiesStaffed;
                }
                return _pFarm;
            }
        }

        public int ScienceLevel
        {
            get
            {
                int _pSci = 0;
                foreach (Region r in RegionList)
                {
                    _pSci += r.ScienceLevel;
                }
                return _pSci;
            }
        }

        public float LabsStaffed
        {
            get
            {
                float _pFarm = 0;
                foreach (Region r in RegionList)
                {
                    _pFarm += r.LabsStaffed;
                }
                return _pFarm;
            }
        }

        public int GovernmentLevel
        {
            get
            {
                int _pGov = 0;
                foreach (Region r in RegionList)
                {
                    _pGov += r.GovernmentLevel;
                }
                return _pGov;
            }
        }

        public float GovernmentFacilitiesStaffed
        {
            get
            {
                float _pFarm = 0;
                foreach (Region r in RegionList)
                {
                    _pFarm += r.GovernmentFacilitiesStaffed;
                }
                return _pFarm;
            }
        }

        public int MiningLevel
        {
            get
            {
                int _pMin = 0;
                foreach (Region r in RegionList)
                {
                    _pMin += r.MiningLevel;
                }
                return _pMin;
            }
        }

        public float MinesStaffed
        {
            get
            {
                float _pStaff = 0;
                foreach (Region r in RegionList)
                {
                    _pStaff += r.MinesStaffed;
                }
                return _pStaff;
            }
        }

        public float PopularSupportLevel
        {
            get
            {
                float _pPopSupport = 0f;
                float _pTotalPopSupport = 0f;
                int _pPopCount = 0;
                foreach (Region r in RegionList)
                {
                    foreach (Pops pop in r.PopsInTile)
                    {
                        if (pop.Type != Pops.ePopType.Retired)
                        {
                            _pTotalPopSupport += pop.PopSupport;
                            _pPopCount += 1;
                        }
                    }
                }
                if (_pPopCount > 0)
                    _pPopSupport = _pTotalPopSupport / _pPopCount;
                else
                    _pPopSupport = 0;

                return _pPopSupport;
            }
        }


        private int basePlanetValue = 0;
        public int BasePlanetValue // calculates a base planet value based on the starting stats
        {
            get
            {
                int pValue = 0;
                pValue = ((AdjustedBio * Size) / 5) + Energy + BasicMaterials + HeavyMaterials + RareMaterials;
                basePlanetValue = (pValue / 10);
                return basePlanetValue;
            }
        }

        public float FarmingMigrationDesirability
        {
            get
            {
                float totalFarmDesire = 0f;
                
                foreach (Region r in RegionList)
                {
                    float regionFarmDesire = 0f;
                    float employment = r.FarmsStaffed / r.FarmingLevel;
                    regionFarmDesire = r.FarmingLevel; // add the number of farms
                    if (employment > .8f)
                    {
                        regionFarmDesire = (regionFarmDesire / .8f) * (employment - .8f);// if employment is near 100%, cap the desire, then decrease it as employment increases
                    }
                    else if (employment > .25f)
                    {
                        regionFarmDesire = (regionFarmDesire / employment) * Constants.Constant.FarmJobDesirability; // then divide by the employment ratio
                    }
                    else
                        regionFarmDesire = (regionFarmDesire / .25f) * Constants.Constant.FarmJobDesirability; // if too low then cap at 25%
                    totalFarmDesire += regionFarmDesire;
                }

                return totalFarmDesire;
            }
        }

        public float MiningMigrationDesirability
        {
            get
            {
                float totalMiningDesire = 0f;
                
                foreach (Region r in RegionList)
                {
                    float regionMiningDesire = 0f;
                    float employment = r.MinesStaffed / r.MiningLevel;

                    regionMiningDesire = r.MiningLevel; // add the number of farms
                    if (employment > .8f)
                    {
                        regionMiningDesire = (regionMiningDesire / .8f) * (employment - .8f);// if employment is near 100%, cap the desire, then decrease it as employment increases
                    }
                    else if (employment > .25f)
                    {
                        regionMiningDesire = (regionMiningDesire / employment) * Constants.Constant.MineJobDesirability; // then divide by the employment ratio
                    }
                    else
                        regionMiningDesire = (regionMiningDesire / .25f) * Constants.Constant.MineJobDesirability; // if too low then cap at 25%
                    
                    totalMiningDesire += regionMiningDesire;
                }

                return totalMiningDesire;
            }
        }

        public float EngineerMigrationDesirability
        {
            get
            {
                float totalEngineerDesire = 0f;

                foreach (Region r in RegionList)
                {
                    float regionEngineerDesire = 0f;
                    float employment = r.FactoriesStaffed / r.ManufacturingLevel;
                    regionEngineerDesire = r.ManufacturingLevel; // add the number of staffed

                    if (employment > .8f)
                    {
                        regionEngineerDesire = (regionEngineerDesire / .8f) * (employment - .8f);// if employment is near 100%, cap the desire, then decrease it as employment increases
                    }
                    else if (employment > .25f)
                    {
                        regionEngineerDesire = (regionEngineerDesire / employment) * Constants.Constant.BuilderJobDesirability; // then divide by the employment ratio
                    }
                    else
                        regionEngineerDesire = (regionEngineerDesire / .25f) * Constants.Constant.BuilderJobDesirability; // if too low then cap at 25%

                    totalEngineerDesire += regionEngineerDesire;
                }

                return totalEngineerDesire;
            }
        }

        public float FluxmenMigrationDesirability
        {
            get
            {
                float totalFluxmenDesire = 0f;

                foreach (Region r in RegionList)
                {
                    float regionFluxmenDesire = 0f;
                    float employment = r.HighTechFacilitiesStaffed / r.HighTechLevel;

                    regionFluxmenDesire = r.HighTechLevel; // add the number of farms
                    if (employment > .8f)
                    {
                        regionFluxmenDesire = (regionFluxmenDesire / .8f) * (employment - .8f);// if employment is near 100%, cap the desire, then decrease it as employment increases
                    }
                    else if (employment > .25f)
                    {
                        regionFluxmenDesire = (regionFluxmenDesire / employment) * Constants.Constant.EngineerJobDesirability; // then divide by the employment ratio
                    }
                    else
                        regionFluxmenDesire = (regionFluxmenDesire / .25f) * Constants.Constant.EngineerJobDesirability; // if too low then cap at 25%

                    totalFluxmenDesire += regionFluxmenDesire;
                }

                return totalFluxmenDesire;
            }
        }

        public float ScientistMigrationDesirability
        {
            get
            {
                float totalScientistDesire = 0f;
                
                foreach (Region r in RegionList)
                {
                    float regionScientistDesire = 0f;
                    float employment = r.LabsStaffed / r.ScienceLevel;

                    regionScientistDesire = r.ScienceLevel; // add the number of farms
                    if (employment > .8f)
                    {
                        regionScientistDesire = (regionScientistDesire / .8f) * (employment - .8f);// if employment is near 100%, cap the desire, then decrease it as employment increases
                    }
                    else if (employment > .25f)
                    {
                        regionScientistDesire = (regionScientistDesire / employment) * Constants.Constant.ScientistJobDesirability; // then divide by the employment ratio
                    }
                    else
                        regionScientistDesire = (regionScientistDesire / .25f) * Constants.Constant.ScientistJobDesirability; // if too low then cap at 25%

                    totalScientistDesire += regionScientistDesire;
                }

                return totalScientistDesire;
            }
        }

        

        private bool b_isInhabited = false;
        public bool IsInhabited
        {
            get
            {
                bool inhabited = false;
                foreach  (Region rData in RegionList)
                {
                    if (rData.PopsInTile.Count > 0)
                    {
                        inhabited = true;
                        break;
                    }
                }
                b_isInhabited = inhabited;
                return b_isInhabited;
            }
        }

        private int pSize; // backing property
        public int Size
        {
            get
            {
                return pSize;
            }

            set
            {
                pSize = Mathf.Clamp(value, 0, 100);
            }
        }

        private float pScanLevel = 0.0f; // backing property
        public float ScanLevel
        {
            get
            {
                return pScanLevel;
            }

            set
            {
                pScanLevel = Mathf.Clamp(value, 0, 1);
            }
        }

        public float AverageDevelopmentLevel
        {
            get
            {
                float farmingDevelopmentLevel = FarmingLevel * Constants.Constant.FarmingDevelopmentModifier;
                float miningDevelopmentLevel = MiningLevel * Constants.Constant.MiningDevelopmentModifier;
                float highTechDevelopmentLevel = HighTechLevel * Constants.Constant.HighTechDevelopmentModifier;
                float scienceDevelopmentLevel = ScienceLevel * Constants.Constant.ScienceDevelopmentModifier;
                float manufacturingDevelopmentLevel = ManufacturingLevel * Constants.Constant.ManufacturingDevelopmentModifier;
                float governmentDevelopmentLevel = TotalAdmin * Constants.Constant.GovernmentDevelopmentModifier;

                int developedRegions = RegionList.FindAll(p => p.PopsInTile.Count > 0).Count;

                return (farmingDevelopmentLevel + miningDevelopmentLevel + highTechDevelopmentLevel + scienceDevelopmentLevel + manufacturingDevelopmentLevel + governmentDevelopmentLevel) / developedRegions;
            }
        }

        private int pHabitability; // backing property
        public int Bio
        {
            get
            {
                return pHabitability;
            }

            set
            {
                pHabitability = Mathf.Clamp(value, 0, 100);
            }
        }

        private int pAdjustedHabitability; // backing property
        public int AdjustedBio
        {
            get
            {
                int bioVar = 0;
                if (PlanetTraits.Count > 0)
                {
                    foreach (PlanetTraits trait in PlanetTraits)
                    {
                        bioVar += trait.HabMod;
                    }
                }
                pAdjustedHabitability = Bio + bioVar;
                pAdjustedHabitability = Mathf.Clamp(pAdjustedHabitability, 0, 100);
                return pAdjustedHabitability;
            }
        }

        private float pIndMult; // backing property
        public float IndustrialMultiplier
        {
            get
            {
                return pIndMult;
            }

            set
            {
                pIndMult = Mathf.Clamp(value, 0, maxIndMultiplier);
            }
        }

        public bool Rings { get; set;}
      
        private int pMoons; // backing property
        public int Moons
        {
            get
            {
                return pMoons;
            }

            set
            {
                pMoons = Mathf.Clamp(value, 0, maxMoons);
            }
        }

        private int pBasicMaterials; // backing property
        public int BasicMaterials
        {
            get
            {
                return pBasicMaterials;
            }

            set
            {
                pBasicMaterials = Mathf.Clamp(value, 0, 100);
            }
        }

        private int pHeavyMaterials; // backing property
        public int HeavyMaterials
        {
            get
            {
                return pHeavyMaterials;
            }

            set
            {
                pHeavyMaterials = Mathf.Clamp(value, 0, 100);
            }
        }

        private int pRareMaterials; // backing property
        public int RareMaterials
        {
            get
            {
                return pRareMaterials;
            }

            set
            {
                pRareMaterials = Mathf.Clamp(value, 0, 100);
            }
        }

        private int pEnergy; // backing property
        public int Energy
        {
            get
            {
                return pEnergy;
            }

            set
            {
                pEnergy = Mathf.Clamp(value, 0, 100);
            }
        }
     
        public int PlanetSpriteNumber { get; set; }
        public int PlanetRingSpriteNumber { get; set; }
        public int PlanetRingTilt { get; set; }
        public string SystemID { get; set; }
        public StarData System
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetSystem(SystemID);
            }
        }
        public int MaxTiles { get; set; }
        public int MaxHabitableTiles { get; set; }
        private int pAdjustedMaxHabitableTiles; // backing property
        public int AdjustedMaxHabitableTiles
        {
            get
            {
                int maxHabitableVar = 0;
                if (PlanetTraits.Count > 0)
                {
                    foreach (PlanetTraits trait in PlanetTraits)
                    {
                        maxHabitableVar += trait.HabitableTilesMod;
                    }
                }
                pAdjustedMaxHabitableTiles = MaxHabitableTiles + maxHabitableVar;
                pAdjustedMaxHabitableTiles = Mathf.Clamp(pAdjustedMaxHabitableTiles, 0, MaxTiles);
                return pAdjustedMaxHabitableTiles;
            }
        }
        public List<string> RegionIDList = new List<string>();
        public List<Region> RegionList = new List<Region>(); // list of tiles on the planet; populated from tile IDs
	}

    public class PlanetAttributeTable
    {
        public int size;
        public int sizeVar; // variance
        public int habitability;
        public int habVar; // hab variance
        public float indMult;
        public int ringChance;
        public int moonChance;
        public int maxMoons;
        public int alpMaterial;
        public int alpVar;
        public int heavyMaterial;
        public int heavVar;
        public int rareMaterial;
        public int rareVar;
        public int energy;
        public int[] validRegions = new int[8];
    }

    public class PlanetGenerationData
    {
        public StarData.eSpectralClass starType;
        public List<SpotPlanetGenerationTable> planetGenerationTable = new List<SpotPlanetGenerationTable>();       
    }

    public class PlanetAttributeData
    {
        public PlanetData.ePlanetType planetType;
        public PlanetAttributeTable planetTraitsTable = new PlanetAttributeTable();
    }

    public class SpotPlanetGenerationTable
    {
        public int baseChance = 0;
        public float materialMultiplier = 0.0f;
        public int companionModifier = 0;
        public int planetSpot = 0;
        public List<int> chanceData = new List<int>();
        public List<int> typeData = new List<int>();
    }
}

