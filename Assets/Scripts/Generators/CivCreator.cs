using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// object namespaces
using StellarObjects;
using PlanetObjects;
using CivObjects;
using CharacterObjects;

// classes
using CameraScripts;
using Assets.Scripts.States;

namespace CivCreator
{
    public class CivCreator : MonoBehaviour
    {
        private GalaxyData gData;
        private GameData gameDataRef;

        void Start()
        {
            gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            // now generate civilizations
            GenerateHumanCiv();
            GenerateAICivs();
            GenerateCommonHouses(); // generate houses
            GenerateHouseStats(); // generate the other house status
            GenerateCharacters(); // generate characters for the global pool           
            AssignHouses(); // assign houses
            GenerateCivLeaders();         
            GenerateProvinces(); // generate provinces for each civilization
            DetermineSystemCapitals();
            GenerateStellarObjectLeaders(); // test; put back after generate primes if needed
            GenerateInfrastructure();
            SetPlanetTaxes();
            GeneratePrimes();                  
            GeneratePlanetIntelLevels(gameDataRef.CivList.Find(p => p.HumanCiv == true));
            GenerateRelationships(); // determine everyone's relationship to everyone else
            gameDataRef.CivsGenerated = true; // sends flag to move to the galaxy screen
        }

        void GenerateAICivs()
        {
            for (int x = 0; x < gameDataRef.NumberOfAICivs; x++)
            {
                Civilization newCiv = new Civilization();
                newCiv = GenerateGameObject.CreateNewCivilization(); // may need to add parameters
                gameDataRef.AddNewCivilization(newCiv);
            }
        }

        void GenerateCharacters()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                int poolCount = Random.Range(120 - ((int)civ.Size * 5), 120); // generate from 60-120 characters initially
                for (int x = 0; x < poolCount; x++)
                {
                    gameDataRef.CharacterList.Add(GenerateGameObject.GenerateNewCharacter(Character.eRole.Pool, civ.ID)); // assign the character to the global list
                }

                // randomly assign planets for now - leaders will be reassigned to their capital planet later
                int planetsInEmpire = gameDataRef.CivList[0].PlanetList.Count;
                foreach (Character cData in gameDataRef.CharacterList)
                {
                    int planetLocation = Random.Range(0, planetsInEmpire);
                    cData.PlanetLocationID = gameDataRef.CivList[0].PlanetIDList[planetLocation];
                }
            }
        }
        
        void GenerateRelationships()
        {
            Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has Houses
            List<Character> civCharList = new List<Character>();
            civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == humanCiv.ID);

            foreach (Character cData in civCharList)
            {
                foreach (Character otherCData in civCharList)
                {
                    float baseTrust = 0f;
                    float baseFear = 0f;
                    float trustModifier = 0f;
                    float fearModifier = 0f;

                    Relationship newRelationship = new Relationship();

                    if (cData.ID != otherCData.ID) // no entry for yourself!
                    {
                        if (cData.HouseID == otherCData.HouseID)
                        {
                            trustModifier += 75; // if you are in the same house, huge bonus (change)
                            fearModifier -= 55; // you don't fear those in your house!
                        }
                        else if (cData.AssignedHouse.Personality == otherCData.AssignedHouse.Personality)
                        {
                            trustModifier += 30; // if not in the same house, bonus if you share personalities
                            fearModifier -= 15;
                        }
                        else
                        {
                            trustModifier -= 30;
                            fearModifier += 10 * (cData.AssignedHouse.Rank - otherCData.AssignedHouse.Rank);  // more fearful of people from houses of higher rank
                        }

                        if (otherCData.Role == Character.eRole.Emperor) // adjustments for the Emperor's role (he is always feared a little)
                        {
                            trustModifier -= 15;
                            fearModifier += 10;
                        }

                        baseTrust = Random.Range(0, 80);
                        baseFear = Random.Range(0, 60);

                        baseTrust += trustModifier;
                        baseFear += fearModifier;

                        // adjust existing relationship pairs appropriately if needed
                        if (otherCData.Relationships.ContainsKey(cData.ID))
                        {
                            if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Challenged)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Challenger;
                                baseTrust -= 20;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Challenger)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Challenged;
                                baseTrust -= 20;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Predator)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Prey;
                                baseTrust -= 40;
                                baseFear += 50;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Allies)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Allies;
                                baseTrust += 80;
                                baseFear -= 80;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Friends)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Friends;
                                baseTrust += 60;
                                baseFear -= 60;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.HangerOn)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.HungUpon;
                                baseFear -= 40;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Lovers)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Lovers;
                                baseFear -= 50;
                                baseTrust += 30;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Married)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Married;
                                baseFear -= 70;
                                baseTrust += 70;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.HungUpon)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.HangerOn;
                                baseTrust += 40;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Shunning)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Shunned;
                                baseTrust -= 20;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Inferior)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Superior;
                                baseFear -= 30;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Superior)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Inferior;
                                baseFear += 30;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Rival)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Rival;
                                baseTrust -= 50;
                                baseFear -= 20;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Vendetta)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Vendetta;
                                baseTrust -= 70;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.SwornVengeance)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.ObjectOfVengeance;
                                baseTrust -= 40;
                                baseFear += 60;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.ObjectOfVengeance)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.SwornVengeance;
                                baseTrust -= 90;
                            }

                            else if (otherCData.Relationships[cData.ID].RelationshipState == Relationship.eRelationshipState.None)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.None;
                            }

                            else
                            {
                                if (baseTrust > 60)
                                {
                                    int chance = Random.Range(0, 3);
                                    if (chance == 0)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Allies;
                                    else if (chance <= 2)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Friends;
                                    else
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.None;
                                }

                                else if (baseTrust < 30)
                                {
                                    int chance = Random.Range(0, 5);
                                    if (chance == 0)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Rival;
                                    else if (chance == 1)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Vendetta;
                                    else if (chance == 2)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.SwornVengeance;
                                }
                                else
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.None;
                            }
                        }
                        else // if no existing states occur, or no reciprocal relationship yet, create one based on trust and fear factors
                        {
                            if (baseTrust > 70)
                            {
                                int chance = Random.Range(0, 15);
                                if (chance <= 2)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Allies;
                                else if (chance <= 7)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Friends;
                                else if (chance <= 8)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Lovers;
                                else if (chance <= 9 && cData.Gender != otherCData.Gender) // no same-sex marriages here
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Married;
                                else
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.None;
                            }

                            else if (baseTrust < 30)
                            {
                                int chance = Random.Range(0, 6) - (int)(baseTrust / 10);
                                if (chance == 0)
                                    if (cData.Power < (otherCData.Power * 1.5f))
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Predator;
                                    else if (cData.Power > (otherCData.Power * 1.5f))
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Prey;
                                    else
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Rival;
                                else if (chance == 1)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Vendetta;
                                else if (chance == 2)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.SwornVengeance;
                                else if (chance == 3)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Shunning;
                                else
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.None;
                            }
                            else
                                newRelationship.RelationshipState = Relationship.eRelationshipState.None; // UnityEngine.Random.Range(0, 21);
                        }

                        newRelationship.Trust = baseTrust;
                        newRelationship.Fear = baseFear;
                        cData.Relationships.Add(otherCData.ID, newRelationship);
                    }
                    else
                        cData.Relationships.Add(cData.ID, newRelationship);       
                }
            }
        }

        public House CreatePlayerHouse()
        {
            House pHouse = new House();
            pHouse.Name = "Orthos"; // temp name
            pHouse.SwornFealtyCivID = "CIV0";
            pHouse.IsPlayerHouse = true;
            pHouse.IsRulingHouse = true;
            pHouse.Loyalty = 100; // your House is loyal to you to start
            pHouse.Power = 100; // the ruling House starts out with full Power
            pHouse.BannerID = "HOUSE001";
            return pHouse;
        }
       

        void GenerateCivLeaders()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<Character> civCharList = new List<Character>();
                civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == civ.ID);

            tryAssignLeader:
                int choice = Random.Range(0, civCharList.Count);
                if (civCharList[choice].Role == Character.eRole.Pool)
                {
                    civCharList[choice].Role = Character.eRole.Leader;
                    civCharList[choice].BaseActionPoints = Random.Range(1, 3); // temporary
                    if (civ.HumanCiv)
                    {
                        CreatePlayerEmperor(civCharList[choice]);
                        continue;
                    }
                    civCharList[choice].PlanetLocationID = civ.CapitalPlanetID;
                    civ.LeaderID = civCharList[choice].ID;
                }
                else // if already assigned, try again
                {
                    goto tryAssignLeader;
                }
            }
        }

        void CreatePlayerEmperor(Character choice)
        {
            House playerHouse = CreatePlayerHouse();
            gameDataRef.HouseList.Add(playerHouse);
            Emperor newEmp = new Emperor();
            newEmp.Charm = choice.Charm;
            newEmp.Discretion = choice.Discretion;
            newEmp.Drive = choice.Drive;
            newEmp.Humanity = choice.Humanity;
            newEmp.Intelligence = choice.Intelligence;
            newEmp.Passion = choice.Passion;
            newEmp.Piety = choice.Piety;
            newEmp.Caution = choice.Caution;
            newEmp.BaseActionPoints = Random.Range(3, 7); // temporary
            newEmp.BenevolentInfluence = Random.Range(1f, 5f);
            newEmp.PragmaticInfluence = Random.Range(5f, 10f);
            newEmp.TyrannicalInfluence = Random.Range(1f, 5f);
            newEmp.Name = "STEVE I"; // temporary
            newEmp.Health = Character.eHealth.Perfect;
            newEmp.Age = 18;
            newEmp.ID = "EMP001";
            newEmp.CivID = gameDataRef.CivList[0].ID;
            newEmp.Gender = Character.eSex.Male;
            newEmp.EmperorPower = Random.Range(5f, 10f); // will need to be an algorithm to show increase in power through Holdings, etc.
            newEmp.HouseID = gameDataRef.CivList[0].RulingHouseID;
            newEmp.HouseRole = Character.eHouseRole.Leader;
            newEmp.EmperorModelID = "EMP0"; // will be assigned later
            newEmp.PlanetLocationID = gameDataRef.CivList[0].CapitalPlanetID;
            gameDataRef.CivList[0].LeaderID = newEmp.ID;
            gameDataRef.CivList[0].PlayerEmperor = newEmp;
            gameDataRef.CharacterList.Remove(choice);
            gameDataRef.CharacterList.Add(newEmp);
        }

        void GenerateCommonHouses()
        {

            int houseNumber = 0; // for ID generation

            foreach (string name in DataManager.commonHouseNameList)
            {
                int creationChance = Random.Range(0, 100);
                if (creationChance > 95) // only one out of 10 houses will be generated
                {
                    House comHouse = new House(); // generate temp house
                    houseNumber += 1;
                    comHouse.Name = name;
                    comHouse.ID = "COM" + houseNumber.ToString("N0");
                    comHouse.Rank = House.eHouseRank.Common;
                    comHouse.SwornFealtyCivID = "CIV0";
                    comHouse.Power = Random.Range(0, 10); // power from 1-10
                    comHouse.Influence = Random.Range(0, 10); // influence from 1-10
                    comHouse.Age = Random.Range(20, 500); // age of the house
                    comHouse.FarmingTradition = Random.Range(0, 20);
                    comHouse.GovernmentTradition = Random.Range(0, 10);
                    comHouse.HighTechTradition = Random.Range(0, 10);
                    comHouse.ManufacturingTradition = Random.Range(0, 15);
                    comHouse.MiningTradition = Random.Range(0, 10);
                    comHouse.ScienceTradition = Random.Range(0, 10);
                    comHouse.TradeTradition = Random.Range(0, 10);
                    comHouse.WarTradition = Random.Range(0, 10);
                    comHouse.Specialty = (House.eHouseSpecialty)Random.Range(0, 7);
                    comHouse.History = "The common House of " + name + " has an undistinguished tradition. Established to no fanfare in the year " + (3050 - comHouse.Age).ToString("N0") + ", the House has continued as unremarkably as it started.";
                    Debug.Log("Common House of " + name + " Created");
                    gameDataRef.HouseList.Add(comHouse);
                }
            }
        }

        void GenerateHouseStats()
        {
            //Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has Houses
            List<House> greatHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Great);
            List<House> minorHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Minor);

            foreach (House gHouse in greatHouseList)
            {
                gHouse.Influence = Random.Range(40, 80); // temporary code; will be derived once Holdings and house Wealth are generated
                gHouse.IsRulingHouse = false; // initially set all houses to false
                gHouse.SwornFealtyCivID = "CIV0";
            }

            foreach (House mHouse in minorHouseList)
            {
                mHouse.Influence = Random.Range(15, 30); // temporary code; will be derived once Holdings and house Wealth are generated
                mHouse.IsRulingHouse = false; // initially set all houses to false
                mHouse.SwornFealtyCivID = "CIV0";
            }

        }

        void AssignHouses()
        {
            Civilization currentCiv = gameDataRef.CivList[0]; // only the player empire has Houses, but this will change very soon! (Add a loop to go through each civ)
            List<Character> civCharList = new List<Character>();
            List<House>commonHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Common);
            List<House> minorHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Minor);
            List<House>greatHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Great);
            civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == currentCiv.ID);

            foreach (Character cData in civCharList)
            {
                int houseTypeChance = Random.Range(0, 100);

                if (houseTypeChance < Constants.Constant.CommonHouseChance)
                {
                    int houseAssignment = Random.Range(0, commonHouseList.Count);
                    cData.HouseID = commonHouseList[houseAssignment].ID;
                    cData.Wealth = ((int)(commonHouseList[houseAssignment].Wealth + 1) * Random.Range(1, 4)) / 15; // set wealth based on house
                }
                else if (houseTypeChance < Constants.Constant.MinorHouseChance + Constants.Constant.CommonHouseChance)
                {
                    int houseAssignment = Random.Range(0, minorHouseList.Count);
                    cData.HouseID = minorHouseList[houseAssignment].ID;
                    cData.Wealth = ((int)(minorHouseList[houseAssignment].Wealth + 1) * Random.Range(1, 7)) / 15; // set wealth based on house
                }
                else
                {
                    int houseAssignment = Random.Range(0, greatHouseList.Count);
                    cData.HouseID = greatHouseList[houseAssignment].ID;
                    cData.Wealth = ((int)(greatHouseList[houseAssignment].Wealth + 1) * Random.Range(1, 15)) / 10; // set wealth based on house
                }
                string creationType = "born to";
                if (cData.Lifeform == Character.eLifeformType.Machine || cData.Lifeform == Character.eLifeformType.Hybrid || cData.Lifeform == Character.eLifeformType.AI)
                {
                    creationType = "designed and manufactured for";
                }
                cData.History += cData.GenderPronoun + " was " + creationType + " the " + cData.AssignedHouse.Rank.ToString() + " House of " + cData.AssignedHouse.Name + ". ";

                if (cData.Traits.Exists(p=> p.Name.ToUpper() == "AVARICIOUS"))
                {
                    cData.Wealth = cData.Wealth * 5;
                }

                if (cData.Traits.Exists(p => p.Name.ToUpper() == "ASCETIC"))
                {
                    cData.Wealth = cData.Wealth / 20;
                }
            }
        }

        void GeneratePrimes()
        {
            Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has primes
            List<Character> civCharList = new List<Character>();

            civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == humanCiv.ID);

            tryAssignDomPrime:
                int choice = Random.Range(0, civCharList.Count);
                if (civCharList[choice].Role == Character.eRole.Pool)
                {
                    civCharList[choice].Role = Character.eRole.DomesticPrime;
                    civCharList[choice].TimeInPosition = Random.Range(0, (civCharList[choice].Age - 18));
                    civCharList[choice].PlanetLocationID = humanCiv.CapitalPlanetID;
                }
                else // if already assigned, try again
                {
                    goto tryAssignDomPrime;
                }

            tryAssignIntPrime:
                int intChoice = Random.Range(0, civCharList.Count);
                if (civCharList[intChoice].Role == Character.eRole.Pool)
                {
                    civCharList[intChoice].Role = Character.eRole.IntelPrime;
                    civCharList[choice].TimeInPosition = Random.Range(0,(civCharList[choice].Age - 18));
                    civCharList[intChoice].PlanetLocationID = humanCiv.CapitalPlanetID;
                }
                else // if already assigned, try again
                {
                    goto tryAssignIntPrime;
                }
        }

        void GenerateStellarObjectLeaders()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<PlanetData> pDataList = new List<PlanetData>();
                List<Province> provDataList = new List<Province>();
                List<StarData> sDataList = new List<StarData>();
                List<Character> civCharList = new List<Character>();

                // create the 3 character lists so that they can be weighted when they are chosen against
                List<Character> civGreatHouseCharacterList = new List<Character>();
                List<Character> civMinorHouseCharacterList = new List<Character>();
                List<Character> civCommonHouseCharacterList = new List<Character>();

                pDataList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ);             
                civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == civ.ID);
                provDataList = HelperFunctions.DataRetrivalFunctions.GetCivProvinceList(civ);
                sDataList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                if (civ.HumanCiv)
                {
                    civGreatHouseCharacterList = civCharList.FindAll(p => HelperFunctions.DataRetrivalFunctions.GetHouse(p.HouseID).Rank == House.eHouseRank.Great);
                    civMinorHouseCharacterList = civCharList.FindAll(p => HelperFunctions.DataRetrivalFunctions.GetHouse(p.HouseID).Rank == House.eHouseRank.Minor);
                    civCommonHouseCharacterList = civCharList.FindAll(p => HelperFunctions.DataRetrivalFunctions.GetHouse(p.HouseID).Rank == House.eHouseRank.Common);
                }

                // assign a province governor to each province
                if (civ.HumanCiv == true)
                {
                    foreach (Province pData in provDataList)
                    {
                 
                    tryAssignCharacter:
                        int houseType = Random.Range(0, 100);
                        int choice = 0;
                        Character provGov = null;

                        if (houseType < Constants.Constant.ChanceProvinceGovernorGreat)
                        {
                            choice = Random.Range(0, civGreatHouseCharacterList.Count);
                            provGov = civGreatHouseCharacterList[choice];
                        }
                        else if (houseType < Constants.Constant.ChanceProvinceGovernorGreat + Constants.Constant.ChanceProvinceGovernorMinor)
                        {
                            choice = Random.Range(0, civMinorHouseCharacterList.Count);
                            provGov = civMinorHouseCharacterList[choice];
                        }
                        else
                        {
                            choice = Random.Range(0, civCommonHouseCharacterList.Count);
                            provGov = civCommonHouseCharacterList[choice];
                        }

                        if (provGov.Role == Character.eRole.Pool)
                        {
                            provGov.Role = Character.eRole.ProvinceGovernor;
                            provGov.PlanetLocationID = pData.CapitalPlanetID;
                            provGov.ProvinceAssignedID = pData.ID;
                            provGov.TimeInPosition = Random.Range(0, (provGov.Age - 18));
                            provGov.History += "In " + (gameDataRef.GameDate - provGov.TimeInPosition).ToString("G0") + ", " + provGov.GenderPronoun.ToLower() +" was assigned as provincial governor of the " + pData.Name + " province.";
                            Debug.Log("Province governor assigned to " + pData.Name + " province.");
                        }
                        else // if already assigned, try again
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
                else
                {
                    foreach (Province pData in provDataList)
                    {
                    tryAssignCharacter:
                        int choice = Random.Range(0, civCharList.Count);
                        if (civCharList[choice].Role == Character.eRole.Pool)
                        {
                            civCharList[choice].Role = Character.eRole.ProvinceGovernor;
                            civCharList[choice].PlanetLocationID = pData.CapitalPlanetID;
                            civCharList[choice].ProvinceAssignedID = pData.ID;
                            civCharList[choice].History += civCharList[choice].GenderPronoun + " was assigned as provincial governor of the " + pData.Name + " province.";
                            Debug.Log("Province governor assigned to " + pData.Name + " province.");
                        }
                        else
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }

                // assign a system governor to each province
                if (civ.HumanCiv == true)
                {
                    foreach (StarData sData in sDataList)
                    {

                    tryAssignCharacter:
                        int houseType = Random.Range(0, 100);
                        int choice = 0;
                        Character sysGov = null;

                        if (houseType < Constants.Constant.ChanceSystemGovernorGreat)
                        {
                            choice = Random.Range(0, civGreatHouseCharacterList.Count);
                            sysGov = civGreatHouseCharacterList[choice];
                        }
                        else if (houseType < Constants.Constant.ChanceSystemGovernorGreat + Constants.Constant.ChanceSystemGovernorMinor)
                        {
                            choice = Random.Range(0, civMinorHouseCharacterList.Count);
                            sysGov = civMinorHouseCharacterList[choice];
                        }
                        else
                        {
                            choice = Random.Range(0, civCommonHouseCharacterList.Count);
                            sysGov = civCommonHouseCharacterList[choice];
                        }

                        if (sysGov.Role == Character.eRole.Pool)
                        {
                            sysGov.Role = Character.eRole.SystemGovernor;
                            sysGov.PlanetLocationID = sData.SystemCapitalID;
                            sysGov.SystemAssignedID = sData.ID;
                            sysGov.TimeInPosition = Random.Range(0, (sysGov.Age - 18));
                            sysGov.History += "In " + (gameDataRef.GameDate - Random.Range(0, (sysGov.Age - 18))).ToString("G0") + ", " + sysGov.GenderPronoun.ToLower() + " was assigned as system governor of the " + sData.Name + " system.";
                            Debug.Log("System governor assigned to " + sData.Name);
                        }
                        else // if already assigned, try again
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
                else
                {
                    foreach (StarData sData in sDataList)
                    {
                    tryAssignCharacter:
                        int choice = Random.Range(0, civCharList.Count);
                        if (civCharList[choice].Role == Character.eRole.Pool)
                        {
                            civCharList[choice].Role = Character.eRole.SystemGovernor;
                            civCharList[choice].PlanetLocationID = sData.SystemCapitalID;
                            civCharList[choice].SystemAssignedID = sData.ID;
                            Debug.Log("System governor assigned to " + sData.Name);
                        }
                        else
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }

                // assign a viceroy to each planet
                if (civ.HumanCiv == true)
                {
                    foreach (PlanetData pData in pDataList)
                    {

                    tryAssignCharacter:
                        int houseType = Random.Range(0, 100);
                        int choice = 0;
                        Character viceroy = null;

                        if (houseType < Constants.Constant.ChanceViceroyGreat)
                        {
                            choice = Random.Range(0, civGreatHouseCharacterList.Count);
                            viceroy = civGreatHouseCharacterList[choice];
                        }
                        else if (houseType < Constants.Constant.ChanceViceroyGreat + Constants.Constant.ChanceViceroyMinor)
                        {
                            choice = Random.Range(0, civMinorHouseCharacterList.Count);
                            viceroy = civMinorHouseCharacterList[choice];
                        }
                        else
                        {
                            choice = Random.Range(0, civCommonHouseCharacterList.Count);
                            viceroy = civCommonHouseCharacterList[choice];
                        }

                        if (viceroy.Role == Character.eRole.Pool)
                        {
                            viceroy.Role = Character.eRole.Viceroy;
                            viceroy.PlanetLocationID = pData.ID;
                            viceroy.PlanetAssignedID = pData.ID;
                            pData.OverdriveMultiplier = 1; // initially assigned, test, remove eventually (this will be determined internally by the viceroy build plan)
                            viceroy.TimeInPosition = Random.Range(0, (viceroy.Age - 18));
                            viceroy.History += "In " + (gameDataRef.GameDate - Random.Range(0, (viceroy.Age - 18))).ToString("G0") + ", " + viceroy.GenderPronoun.ToLower() + " was assigned as planetary viceroy of " + pData.Name + ".";
                            Debug.Log("Viceroy assigned to " + pData.Name);
                        }
                        else // if already assigned, try again
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
                else
                {
                    foreach (PlanetData pData in pDataList)
                    {
                    tryAssignCharacter:
                        int choice = Random.Range(0, civCharList.Count);
                        if (civCharList[choice].Role == Character.eRole.Pool)
                        {
                            civCharList[choice].Role = Character.eRole.Viceroy;
                            civCharList[choice].PlanetLocationID = pData.ID;
                            civCharList[choice].PlanetAssignedID = pData.ID;
                            civCharList[choice].History += "In " + (gameDataRef.GameDate - Random.Range(0, (civCharList[choice].Age - 18))).ToString("G0") + ", " + civCharList[choice].GenderPronoun.ToLower() + " was assigned as planetary viceroy of " + pData.Name + ".";
                        }
                        else
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
            }
        }

        void SetPlanetTaxes()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<PlanetData> pDataList = new List<PlanetData>();
                pDataList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ);

                foreach (PlanetData pData in pDataList)
                {
                    pData.PercentGPPForTax = Random.Range(.15f, .25f);
                    pData.PercentGPPForImports = Random.Range(.25f, Constants.Constant.MaxGPPAllocatedForImports);
                    pData.PercentGPPForTrade = Random.Range(.1f, Constants.Constant.MaxGPPAllocatedForTrade);
                    pData.CommerceTax = Random.Range(.3f, .5f);

                    // what percent of a surplus a viceroy is willing to export
                    pData.FoodExportPercentHold = Random.Range(.05f, .5f);
                    pData.EnergyExportPercentHold = Random.Range(.05f, .5f);
                    pData.BasicExportPercentHold = Random.Range(.05f, .4f);
                    pData.HeavyExportPercentHold = Random.Range(.05f, .4f);
                    pData.RareExportPercentHold = Random.Range(.05f, .3f);

                    // what percent of a surplus a viceroy is willing to use for internal commerce
                    pData.FoodCommercePercentHold = Random.Range(.05f, .5f);
                    pData.EnergyCommercePercentHold = Random.Range(.05f, .5f);
                    pData.BasicCommercePercentHold = Random.Range(.05f, .4f);
                    pData.HeavyCommercePercentHold = Random.Range(.05f, .4f);
                    pData.RareCommercePercentHold = Random.Range(.05f, .3f);
                }
            }
        }

        void GenerateInfrastructure()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<PlanetData> pDataList = new List<PlanetData>();
                pDataList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ);

                foreach (PlanetData pData in pDataList)
                {
                    GenerateGameObject.AddDevelopmentToPlanet(pData); // add industrial development
                    GenerateGameObject.GenerateRegionInfrastructure(pData); // now add urban development
                    GenerateGameObject.AddPopsToPlanet(pData, civ); // finally, add pops
                    GenerateGameObject.AddStockpilesToPlanet(pData); // and add stock levels
                    GenerateGameObject.AddTradeInfrastructureToPlanet(pData); // add add starbases and trade infrastructure
                    pData.BuildPlan.Status = BuildPlan.eBuildPlanStatus.Pending; // set build plan status to pending
                }
            }
        }

        void GenerateHumanCiv()
        {
            Civilization newCiv = new Civilization();
            newCiv.Name = "Celestial Empire"; //gameDataRef.PlayerEmpireName;
            newCiv.Color = new Color(0f, .68f, 1f); // the color of the empire's system and planet holdings
            newCiv.Type = Civilization.eCivType.PlayerEmpire;  // default empire
            newCiv.CivMaxProvinceSize = 6;
            newCiv.AdminRating = 35; // rating that determines maximum size of governable provinces
            newCiv.Treasury = Random.Range(1000000f, 5000000f);  // starting treasury
            newCiv.Size = Civilization.eCivSize.Major;
            newCiv.Range = new Vector2(Random.Range(7000, 12000),Random.Range(7000,12000)); //(gameDataRef.GalaxySizeWidth / 1.9f); // determines how far humanity will go from Neo-Sirius (the Empire)
            newCiv.PlanetMinTolerance = 32; // lower since older world
            newCiv.AstronomyRating = Random.Range(6,11) * 1000; // not used
            newCiv.ID = "CIV0"; // use this to reference the player's civ
            newCiv.HumanCiv = true;

            PlanetData pData;
            List<PlanetData> terranSystemPlanetList = gData.GalaxyStarDataList.Find(p => p.Name == "Neo-Sirius").PlanetList;
            pData = terranSystemPlanetList[Random.Range(0, terranSystemPlanetList.Count)];

            // create the human homeworld manually;
            pData.Name = "New Terra";
            pData.ID = "PLANEWT001";
            pData.PlanetSpriteNumber = 0; // the city type
            pData.Type = PlanetData.ePlanetType.Terran;
            pData.Rank = PlanetData.ePlanetRank.ImperialCapital;
            pData.Size = 60;
            pData.Bio = 90;
            pData.Energy = 70;
            pData.HeavyMaterials = 60;
            pData.MaxTiles = 30;
            pData.MaxHabitableTiles = 26;
            pData.RareMaterials = 40;
            pData.BasicMaterials = 88;
            pData.TradeStatus = PlanetData.eTradeStatus.HasTradePort;
            pData.TradeHub = PlanetData.eTradeHubType.CivTradeHub;
            newCiv.CapitalPlanetID = pData.ID;         
            GenerateGameObject.GeneratePlanetRegions(pData); // rework the tiles
            newCiv.PlanetIDList.Add(pData.ID);

            // generate the rest of the planet claims for the player empire
            GenerateGameObject.GenerateResourcePrices(newCiv); 
            gameDataRef.AddNewCivilization(newCiv);
            GenerateGameObject.ClaimPlanetsForCiv(newCiv);

            // determine intel level for systems, scan level for planets
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                foreach (StarData civStar in HelperFunctions.DataRetrivalFunctions.GetCivSystemList(newCiv))
                {
                    float range = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(civStar, star);
                    if (star.IntelValue < (int)(newCiv.AstronomyRating / (range + 1)))
                        star.IntelValue = (int)(newCiv.AstronomyRating / (range + 1));
                }
            }      
        }

        void GeneratePlanetIntelLevels(Civilization humanCiv)
        {
            List<PlanetData> civPlanetList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(humanCiv);
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                foreach (PlanetData planet in star.PlanetList)
                {
                    if (civPlanetList.Exists(p => p.ID == planet.ID))
                    {
                        planet.IntelLevel = 10;
                        planet.ScanLevel = 1f;
                    }
                    else
                    {
                        planet.IntelLevel = star.IntelValue / 2;
                        planet.ScanLevel = (float)(star.IntelValue / 10f) * Random.Range(.2f, 1f);
                    }
                }
            }
        }

        void DetermineSystemCapitals()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<StarData> sDataList = new List<StarData>();
                sDataList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                // cycle through each system and determine if a system capital is available
                foreach (StarData sData in sDataList)
                {
                    int highPlanetValue = 0;
                    PlanetData proposedCapital = new PlanetData();

                    foreach (PlanetData pData in sData.PlanetList)
                    {
                        if (HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ).Exists(p => p.Name == pData.Name))
                        {
                            proposedCapital = pData;
                            if (highPlanetValue < pData.BasePlanetValue)
                            {            
                                proposedCapital = pData;
                                highPlanetValue = pData.BasePlanetValue;                             
                            }
                        }
                    }
                    if (proposedCapital.Rank != PlanetData.ePlanetRank.ImperialCapital && proposedCapital.Rank != PlanetData.ePlanetRank.ProvinceCapital)
                    {
                        proposedCapital.Rank = PlanetData.ePlanetRank.SystemCapital;

                        // coin flip for trade hub (move to another function after capital generation)
                        if (Random.Range(0,100) > 50)
                            proposedCapital.TradeHub = PlanetData.eTradeHubType.SecondaryTradeHub;
                    }
                    sData.SystemCapitalID = proposedCapital.ID; // set as system capital by default

                }
            }
        }

        void GenerateProvinces()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                if (civ.CivMaxProvinceSize > 1)
                {
                    List<StarData> sDataList = new List<StarData>();
                    sDataList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                    // get a size for each province and then create it with provinces in range
                    int provinceSize = Random.Range(2, civ.CivMaxProvinceSize); // vary each province size
                    int maxDist = civ.AdminRating * 150; // max dist between systems (legacy code!)
                    int count = 0;
                    bool provinceValid = false;

                    foreach (StarData sData in sDataList)
                    {
                        Province newProvince = new Province();

                        // generate province name
                        if (DataManager.systemNameList.Count > 0)
                        {
                            var nameIndex = Random.Range(0, DataManager.systemNameList.Count);
                            newProvince.Name = DataManager.systemNameList[nameIndex];
                            DataManager.systemNameList.RemoveAt(nameIndex);
                            DataManager.systemNameList.TrimExcess();
                        }
                        else
                            newProvince.Name = "Generic Province";

                        newProvince.ID = "PRO" + Random.Range(0, 10000);
                        newProvince.OwningCivID = civ.ID;
                        int x = 0;                     
                        bool allSystemsChecked = false;
                        provinceValid = false;

                        do 
                        {                                                 
                            for (int y = count; y < sDataList.Count; y++)
                            {
                                float dist = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(sData,sDataList[y]);
                                if (dist > 0 && dist < maxDist)
                                {
                                    if (sData.AssignedProvinceID == "")
                                    {
                                        sData.AssignedProvinceID = newProvince.ID;
                                        x += 1;
                                        sData.IsProvinceHub = true; // is the central system, will have the province capital installed
                                    }

                                    if (sDataList[y].AssignedProvinceID == "")
                                    {
                                        sDataList[y].AssignedProvinceID = newProvince.ID;
                                        x += 1;
                                        if (x == 1)
                                            sDataList[y].IsProvinceHub = true;               
                                    }
                                    provinceValid = true;
                                }                               
                            }
                         
                            if (provinceValid)
                                gData.AddProvinceToList(newProvince);

                            allSystemsChecked = true; // all systems have been checked; go to next star
                            count += 1; // put after the while statement earlier
                        }                        
                        while (x < provinceSize && !allSystemsChecked);
                        
                    }

                    foreach (StarData sData in sDataList) // if there are any singletons, assign a 'one shot' province AFTER generation has occured
                    {
                        if (sData.AssignedProvinceID == "")
                        {
                            Province nProv = new Province();

                            // generate province name
                            if (DataManager.systemNameList.Count > 0)
                            {
                                var nameIndex = Random.Range(0, DataManager.systemNameList.Count);
                                nProv.Name = DataManager.systemNameList[nameIndex];
                                DataManager.systemNameList.RemoveAt(nameIndex);
                                DataManager.systemNameList.TrimExcess();
                            }
                            else
                                nProv.Name = "Generic Province";

                            nProv.ID = "PRO" + Random.Range(0, 10000);
                            nProv.OwningCivID = civ.ID;
                            sData.AssignedProvinceID = nProv.ID;
                            sData.IsProvinceHub = true;
                            gData.AddProvinceToList(nProv);
                        }
                    }

                    // now that provinces are created, designate the province capital
                    foreach (StarData sData in sDataList)
                    {
                        if (sData.IsProvinceHub)
                        {
                            int highPlanetValue = 0;
                            PlanetData proposedCapital = new PlanetData();
                            foreach (PlanetData pData in sData.PlanetList)
                            {
                                if (HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ).Exists(p => p.Name == pData.Name))
                                {
                                    if (highPlanetValue < pData.BasePlanetValue)
                                    {
                                        if (pData.Rank != PlanetData.ePlanetRank.ImperialCapital) // don't overwrite the imperial capital! If no other capital, the imperial capital serves as province capital also
                                        {
                                            proposedCapital = pData;
                                            highPlanetValue = pData.BasePlanetValue;
                                        }
                                    }
                                }
                            }
                            proposedCapital.Rank = PlanetData.ePlanetRank.ProvinceCapital;
                            proposedCapital.TradeHub = PlanetData.eTradeHubType.ProvinceTradeHub; // set up a trade hub; will move this to its own function
                            proposedCapital.TradeStatus = PlanetData.eTradeStatus.HasTradeHub; // sets up a trade hub status
                            HelperFunctions.DataRetrivalFunctions.GetProvince(sData.AssignedProvinceID).CapitalPlanetID = proposedCapital.ID; // set the capital world ID
                        }
                    }
                }               
            }
        }
    }
}
