using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlanetObjects;
using StellarObjects;
using HelperFunctions;
using CivObjects;

namespace CharacterObjects
{
    public class Emperor : Character
    {
        // leader/emperor variables
        public float BenevolentInfluence { get; set; }
        public float PragmaticInfluence { get; set; }
        public float TyrannicalInfluence { get; set; }
        public string EmperorModelID { get; set; }
        public string EmperorSuffix { get; set; } // what letter of the line the leader is

        public float Sanity { get; set; }
        public float EmperorPower { get; set; }
        public float PsyEXP { get; set; }
        public float StressLevel { get; set; }
        public float HuntingScore { get; set; } // may not use
        public float SkillEXP { get; set; }

        // need to create a Emperor Skill object and a list of those here

        // need to create a Psy Skill object and a list of those here

        // need to create a Status Effect object and a list of active ones here

        public List<string> FriendsList = new List<string>(); // pulls 5 best friends (may need to be get accessor)
        public List<string> EnemiesList = new List<string>(); // pulls 5 greatest enemies (may need to be get accessor)
    }

    public class Character
    {
        public enum eSex : int
        {
            Male,
            Female,
            None
        }

        public enum eRole : int
        {
            Emperor,
            Leader,
            Viceroy,
            ProvinceGovernor,
            SystemGovernor,
            Admiral,
            General,
            SciencePrime,
            FinancePrime,
            WarPrime,
            DomesticPrime,
            IntelPrime,
            Inquisitor,
            Pool
        }

        public enum eHouseRole : int
        {
            Leader,
            Heir,
            Pretender,
            Member
        }

        public enum eHealth : int
        {
            Perfect,
            Fine,
            Healthy,
            Impaired,
            Unhealthy,
            Bedridden,
            Critical,
            Dying,
            Dead,
            Functional,
            Malfunctioning,
            Seriously_Damaged,
            Critically_Damaged,
            Offline
        }

        public enum eBuildFocus : int // what type of build this character is focusing on at this time
        {
            Farms,
            Mines,
            Labs,
            Hightech,
            Admin,
            Factories,
            Nothing,
            Consolidation,
        }

        public enum eLifeformType : int
        {
            Human,
            Human_Immobile,
            Resuscitated,
            Hybrid,
            Machine,
            AI
        }

        public string Name { get; set; }
        public string ID { get; set; }       
        public Dictionary<string, Relationship> Relationships = new Dictionary<string, Relationship>(); // dictionary with the advanced relationship variables
        public string HouseID { get; set; }
        public eHouseRole HouseRole { get; set; }
        public string CivID { get; set; }
        public Civilization Civ
        {
            get
            {
                return DataRetrivalFunctions.GetCivilization(CivID);
            }
        }
        public string PlanetLocationID { get; set; }

        public string PlanetAssignedID { get; set; }
        public BuildPlan PlanetBuildPlan = new BuildPlan(); // add the build plan to the character; only active if they are a viceroy
        public float TimeInPosition { get; set; } // how long has the character been in the position?
       
        public PlanetData PlanetAssigned
        {
            get
            {
                return DataRetrivalFunctions.GetPlanet(PlanetAssignedID);
            }
        }

        public string SystemAssignedID { get; set; }
        public StarData SystemAssigned
        {
            get
            {
                return DataRetrivalFunctions.GetSystem(SystemAssignedID);
            }
        }

        public string ProvinceAssignedID { get; set; }
        public Province ProvinceAssigned
        {
            get
            {
                return DataRetrivalFunctions.GetProvince(ProvinceAssignedID);
            }
        }

        // basic stats
        public eSex Gender { get; set; }
        public eLifeformType Lifeform { get; set; } // what type of life form is the character?
        public eRole Role { get; set; }
        public eHealth Health { get; set; }
        public int Age { get; set; }
        public string History { get; set; }

        // power stats
        public int Wealth { get; set; }
        public int BaseInfluence { get; set; }
        public int Admin { get; set; }

        // primary character attributes (will change over the game)     
        // the motive attributes
        public float Piety { get; set; }
        public float Honor { get; set; }
        public float Humanity { get; set; }

        // the emotion attributes
        public float Passion { get; set; }
        public float Caution { get; set; }
        public float Drive { get; set; }

        // the talent attributes
        public float Intelligence { get; set; }
        public float Charm { get; set; }  
        public float Discretion { get; set; }
       
        
        
        public string GenderPronoun
        {
            get
            {
                if (Gender == eSex.Female)
                    return "She";
                else if (Gender == eSex.Male)
                    return "He";
                else
                    return "It";
            }
        }
        public int Power // this is pretty basic, and deterministic by current role and house. Will probably completely rewrite.
        {
            get
            {
                int totalInf = BaseInfluence;
             
                // add influence based on House power
                totalInf += (int)(AssignedHouse.Influence / 4);

                // adjust by house role
                if (HouseRole == eHouseRole.Leader)
                {
                    totalInf += (int)(AssignedHouse.Influence / 5);
                }
                if (HouseRole == eHouseRole.Heir)
                {
                    totalInf += (int)(AssignedHouse.Influence / 8);
                }

                // adjust by rank
                if (Role == eRole.Viceroy)
                {
                    if (PlanetAssigned != null)
                        totalInf += (int)(PlanetAssigned.BasePlanetValue / 3);
                }
                if (Role == eRole.SystemGovernor)
                {
                    if (SystemAssigned != null)
                        totalInf += (int)(SystemAssigned.BaseSystemValue / 2);
                }

                if (Role == eRole.ProvinceGovernor)
                {
                    if (ProvinceAssigned != null)
                        totalInf += (int)(ProvinceAssigned.BaseProvinceValue / 5);
                }
                if (Role == eRole.DomesticPrime)
                    totalInf += 50; // set prestige amount;

                return totalInf;
            }
        }

        // focuses
        public eBuildFocus BuildFocus { get; set; } // what is the character focused on building?
        public float TimeSinceBuildFocusChange { get; set; }
        
        // aptitudes (derived from House membership)
        public int MiningAptitude { get; set; }
        public int FarmingAptitide { get; set; }
        public int GovernmentAptitude { get; set; }
        public int ScienceAptitude { get; set; }
        public int HighTechAptitude { get; set; }
        public int EngineeringAptitude { get; set; }
        public int TradeAptitude { get; set; }
        public int WarAptitude { get; set; }

        // skills (determined during character generation)
        public int Administration { get; set; }
        public int Engineering { get; set; }
        public int Warfare { get; set; }
        public int Science { get; set; }
        public int Adaptation { get; set; } // multiplier in unorthodox tasks - spying, inquisition, etc
        public int Psy { get; set; } // psy potential of the character    
        public int ActionPoints { get; set; }
        public int BaseActionPoints { get; set; }

        // AI tendencies (will not change over the game, determine trait effects)
        public int ChangeTendency { get; set; }
        public int GoalFocusTendency { get; set; }
        public int WealthTendency { get; set; }
        public int PopsTendency { get; set; }
        public int BudgetTendency {get; set;}
        public int CourageTendency {get; set;}
        public int GoalStabilityTendency {get; set;}
        public int TaxTendency { get; set; }
        public int ScienceTendency { get; set; }       
        public int GluttonyTendency { get; set; }
        public int LearningTendency { get; set; }
        public int ReserveTendency { get; set; }
        public int TraderTendency { get; set; }
        public int DiplomacyTendency { get; set; }
        public int TravelTendency { get; set; }
        public int TrustTendency { get; set; }    
        public int AdminTendency { get; set; }

        // traits
        public List<CharacterTrait> Traits = new List<CharacterTrait>();

        public House AssignedHouse
        {
            get
            {          
                GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
                if (gameDataRef.HouseList.Exists(p => p.ID == HouseID))
                {
                    return gameDataRef.HouseList.Find(p => p.ID == HouseID);
                }
                else
                    return null; // error, no house
            }
        }

        public float EmperorPoSup // weights popular support by planet population relative to the empire
        {
            get
            {
                float totalPopSupport = 0f;
                int totalPopulation = 0;
                foreach (PlanetData pData in Civ.PlanetList)
                {
                    totalPopulation += pData.TotalPopulation;
                }

                foreach (PlanetData pData in Civ.PlanetList)
                {
                    totalPopSupport += pData.PopularSupportLevel * ((float)pData.TotalPopulation / (float)totalPopulation);
                }

                return totalPopSupport;
            }
        }

        // intel levels
        public int IntelLevel { get; set; }
        public string PictureID { get; set; }

    }

    public class HouseTrait
    {
        public enum eHouseTrait : int
        {
            IsPrime,
            IsNotPrime,
            IsSciencePrime,
            IsWarPrime,
            IsIntelPrime,
            IsDomesticPrime,
            IsDiplomaticPrime,
            HasTitles,
            Upstart,
            LikesUpstarts,
            HatesUpstarts,
            HatesNonFaction,
            HasEmperorFavor,
            NoEmperorFavor,
            IsGeneral,
            IsAdmiral,
            IsGovernor,
            IsViceroy,
            HighPower,
            LowPower,
            IsWealthy,
            IsGreat,
            IsMinorOrBetter,
            IsCommon
        }
        public eHouseTrait Trait { get; set; }
        public int Modifier { get; set; }
    }

    // RELATIONSHIP CLASS
    public class Relationship
    {
        public enum eRelationshipState : int
        {
            None, // 0
            Allies, // 1
            Friends, // 2
            Superior, // 3
            Inferior, // 4
            Challenger, // 5
            Challenged, // 6
            Rival, // 7
            Shunning, // 8
            Shunned, // 9
            SwornVengeance, // 10
            ObjectOfVengeance, // 11
            Vendetta, // 12
            Married, // 13
            Lovers, // 14
            HangerOn, // 15
            HungUpon, // 16
            Patron, // 17
            Protegee, // 18
            Predator, // 19
            Prey, // 20
            Spouse // 21
        }

        public enum eSecretRelationshipState : int
        {
            LovesCharacterRomantically,
            LovesCharacterPlatonically,
            HasGrudge,
            WouldBetray,
            IsThreat
        }

        private const int maxTrust = 100;
        private const int maxFear = 100;

        public eRelationshipState RelationshipState;
        public eSecretRelationshipState SecretRelationshipState;

        // private (secret) attitudes of characters towards each other
        public int BaselineAttraction { get; set; } // base 'do I like this person?' attitude. Not public
        public bool PlatonicLove { get; set; }
        public bool RomanticLove { get; set; }
        public float GrudgeLevel { get; set; } // chance that a grudge is created against a character, leading to 100 - only death will allow this grudge to pass
        public float BetrayalLevel { get; set; } // chance that a character feels betrayed     
            
        // used to determine the amount of threat that the relationed character represents to this character
        private float _pFear;
        public float Fear
        {
            get
            {
                return _pFear;
            }

            set
            {
                _pFear = Mathf.Clamp(value, 0, maxFear);
            }
        }

        // implicit trust that this character won't screw the other one over
        private float _pTrust;
        public float Trust
        {
            get
            {
                return _pTrust;
            }

            set
            {
                _pTrust = Mathf.Clamp(value, 0, maxTrust);
            }
        }
    }

    // Challenge class (creates a 'battle' between 2 characters where Influence can be won or lost)
    public class Challenge
    {
        public string ChallengeID { get; set; } // a unique ID identifier for the challenge
        public string CharacterChallengerID { get; set; } // who issued the challenge
        public string CharacterChallengedID { get; set; } // who was challenged
        public int InfluenceStakedOnChallenge { get; set; } // the current influence to be gained/lost from the challenge
        public int TurnsSinceChallengeIssued { get; set; } // how many turns the challenge has been in effect
        public string WinnerOfChallengeID { get; set; } // the winnner!
        public string LoserOfChallengeID { get; set; } // the not-winner.
    }

    // HOUSE CLASS //
    public class House
    {
        public enum eHouseRank : int
        {
            Great,
            Minor,
            Common
        }

        public enum eHouseSpecialty : int
        {
            Science,
            Manufacturing,
            HighTech,
            Mining,
            Farming,
            Government,
            Trade
        }

        public enum eHousePersonality : int
        {
            VeryBenevolent,
            SomewhatBenevolent,
            Pragmatic,
            ModerarelyTyrannical,
            HighlyTyrannical
        }

        public enum eHouseStability : int
        {
            VeryUnstable,
            SomewhatUnstable,
            SomewhatStable,
            VeryStable
        }

        public enum eHouseWealth : int
        {
            Poor,
            Wanting,
            Sufficient,
            Affluent,
            Wealthy
        }

        public enum eHouseAmbition : int
        {
            Expansionist,
            Opportunistic,
            Content,
            Consolidating
        }

        // basic stats common to all Houses
        public string Name { get; set; }
        public string ID { get; set; }
        public string SwornFealtyCivID { get; set; }
        public eHouseRank Rank { get; set; }
        public eHouseSpecialty Specialty { get; set; }
        public eHousePersonality Personality { get; set; }
        public eHouseStability Stability { get; set; }
        public eHouseWealth Wealth { get; set; }
        public eHouseAmbition Ambition { get; set; }
        public Culture Culture { get; set; }
        public bool IsRulingHouse { get; set; }
        public bool IsPlayerHouse { get; set; }
        private List<string> planetHoldingIDs = new List<string>();
        public List<string>PlanetHoldingIDs
        {
            get
            {
                return null;
            }
        } 

        // Traditions are values from 1-100 that determine how much a particular sector of activity has been learned and passed through the House for generations.
        // Great Houses might have values near 80, Minor Houses might have values near 40, and common Houses will have values around 5-10 (small and diffuse Houses don't accrue tradition)
        public int ScienceTradition { get; set; }
        public int ManufacturingTradition { get; set; }
        public int HighTechTradition { get; set; }
        public int FarmingTradition { get; set; }
        public int GovernmentTradition { get; set; }
        public int MiningTradition { get; set; }
        public int WarTradition { get; set; }
        public int TradeTradition { get; set; }

        // trait list
        public List<HouseTrait> Traits = new List<HouseTrait>();

        // history stats
        public int Age { get; set; }
        public string BannerID { get; set; }
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public string LeaderID { get; set; }
        public List<string> HeirListIDs = new List<string>();
        public string LeaderTitle { get; set; }
        public string History { get; set; }
        public int Power; // derived
        public int Loyalty; // derived
        public int Influence { get; set; } // formerly respect      

        public void AddNewTrait(HouseTrait.eHouseTrait trait, int value)
        {
            HouseTrait hTrait = new HouseTrait();
            hTrait.Trait = trait;
            hTrait.Modifier = value;
            Traits.Add(hTrait);
        }
    }

    // CHARACTER TRAIT CLASS
    public class CharacterTrait
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OppositeTraitID { get; set; }
        public int ChangeTendency { get; set; }
        public int GoalFocusTendency { get; set; }
        public int WealthTendency { get; set; }
        public int PopsTendency { get; set; }
        public int BudgetTendency { get; set; }
        public int CourageTendency { get; set; }
        public int GoalStabilityTendency { get; set; }
        public int TaxTendency { get; set; }
        public int ScienceTendency { get; set; }
        public int GluttonyTendency { get; set; }
        public int LearningTendency { get; set; }
        public int ReserveTendency { get; set; }
        public int TraderTendency { get; set; }
        public int DiplomacyTendency { get; set; }
        public int TravelerTendency { get; set; }
        public int TrustTendency { get; set; }
        public int AdminTendency { get; set; }
    }


}
