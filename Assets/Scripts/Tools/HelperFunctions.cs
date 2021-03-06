﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using StellarObjects;
using System.Threading;
using PlanetObjects;
using CharacterObjects;
using CivObjects;
using Projects;

namespace Logging
{
    public static class Logger
    {
        public static void LogThis(string logMessage)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Console.WriteLine(logMessage);

            System.IO.Directory.CreateDirectory("C:/ImperiaLogs"); // create the directory if needed
  
            using (StreamWriter writer = new StreamWriter("C:/ImperiaLogs/" + FileDate() + "_" + gDataRef.GameNumber.ToString("N0") + ".txt", true))
            {
                writer.WriteLine(logMessage);
                //writer.WriteLine();
            }
        }

        private static string FileDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}

namespace HelperFunctions
{
    public static class StringConversions
    {

        public static string ConvertToRomanNumeral(int Number)
        {
            switch (Number)
            {
                case 1:
                    return "I";
                case 2:
                    return "II";
                case 3:
                    return "III";
                case 4:
                    return "IV";
                case 5:
                    return "V";
                case 6:
                    return "VI";
                case 7:
                    return "VII";
                case 8:
                    return "VIII";
                case 9:
                    return "IX";
                case 10:
                    return "X";
                default:
                    return "0";
            }
        }

        public static string ConvertValueToDescription(int number)
        {
            if (number == 0)
                return "Impossible";
            else if (number < 15)
                return "Abysmal";
            else if (number < 30)
                return "Very Low";
            else if (number < 40)
                return "Low";
            else if (number < 55)
                return "Medium";
            else if (number < 70)
                return "High";
            else if (number < 85)
                return "Very High";
            else if (number < 101)
                return "Fantastic";
            else
                return "Extraordinary";
        }

        public static string ConvertIntelValueToDescription(int number)
        {
            if (number == 0)
                return "None";
            else if (number < Constants.Constant.LowIntelLevelMax)
                return "Low Intel";
            else if (number < Constants.Constant.MediumIntelLevelMax)
                return "Moderate Intel";
            else if (number < Constants.Constant.HighIntelMax)
                return "High Intel";
            else 
                return "Max Intel";
        }

        public static string ConvertCharacterValueToDescription(int number, int intelLevel)
        {
            if (intelLevel < Constants.Constant.LowIntelLevelMax)
            {
                if (number < 50)
                    return "Low";
                else 
                    return "High";
            }
            else if (intelLevel < Constants.Constant.MediumIntelLevelMax)
            {
                if (number < 40)
                    return "Low";
                else if (number < 70)
                    return "Average";
                else
                    return "High";
            }
            else if (intelLevel < Constants.Constant.HighIntelMax)
            {
                if (number < 20)
                    return "Poor";
                else if (number < 40)
                    return "Low";
                else if (number < 60)
                    return "Average";
                else if (number < 80)
                    return "High";
                else
                    return "Very High";
            }
            else
            {
                if (number == 0)
                    return "None";
                else if (number < 15)
                    return "Abysmal";
                else if (number < 30)
                    return "Very Low";
                else if (number < 40)
                    return "Low";
                else if (number < 55)
                    return "Medium";
                else if (number < 70)
                    return "High";
                else if (number < 85)
                    return "Very High";
                else
                    return "Exceptional";
            }
        }

        public static string ConvertPlanetEnum(PlanetData.ePlanetType type)
        {
            switch (type)
            {
                case PlanetData.ePlanetType.AsteroidBelt:
                    return "Asteroid Belt";
                case PlanetData.ePlanetType.Barren:
                    return "Barren Planet";
                case PlanetData.ePlanetType.BrownDwarf:
                    return "Brown Dwarf";
                case PlanetData.ePlanetType.City:
                    return "City Planet";
                case PlanetData.ePlanetType.Desert:
                    return "Desert Planet";
                case PlanetData.ePlanetType.DustRing:
                    return "Dust Ring";
                case PlanetData.ePlanetType.GasGiant:
                    return "Gas Giant";
                case PlanetData.ePlanetType.Greenhouse:
                    return "Greenhouse Planet";
                case PlanetData.ePlanetType.Ice:
                    return "Ice Planet";
                case PlanetData.ePlanetType.IceBelt:
                    return "Ice Belt";
                case PlanetData.ePlanetType.IceGiant:
                    return "Ice Giant";
                case PlanetData.ePlanetType.Irradiated:
                    return "Irradiated Planet";
                case PlanetData.ePlanetType.Lava:
                    return "Lava Planet";
                case PlanetData.ePlanetType.Ocean:
                    return "Ocean Planet";
                case PlanetData.ePlanetType.Organic:
                    return "Organic Planet";
                case PlanetData.ePlanetType.SuperEarth:
                    return "Super Earth Planet";
                case PlanetData.ePlanetType.Terran:
                    return "Terran Planet";
                default:
                    return "Unknown Planet";
            }
        }

        public static string ConvertRoleEnum(Character.eRole role)
        {
            switch (role)
            {
                case Character.eRole.DomesticPrime:
                    return "Domestic Prime";
                case Character.eRole.ProvinceGovernor:
                    return "Provincial Governor";
                case Character.eRole.SystemGovernor:
                    return "System Governor";
                case Character.eRole.FinancePrime:
                    return "Financial Prime";
                case Character.eRole.Viceroy:
                    return "Planetary Viceroy";
                case Character.eRole.Emperor:
                    return "Emperor";
                case Character.eRole.Pool:
                    return "Minor Character";
                default:
                    return "Unknown Role";
            }
        }

        public static string ConvertIntDollarToText(int num)
        {
            string numbStr = "";

            if ((float)(num / 1000f) <= 1)
                numbStr = "$" + num.ToString("N0") + "B";
            else if ((float)(num / 1000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000f)).ToString("N2") + "T";
            else if ((float)(num / 1000000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000000f)).ToString("N2") + "QD";

            return numbStr;
        }

        public static string ConvertFloatDollarToText(float num)
        {
            string numbStr = "";

            if ((float)(num / 1000f) <= 1)
                numbStr = "$" + num.ToString("N0") + "B";
            else if ((float)(num / 1000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000f)).ToString("N2") + "T";
            else if ((float)(num / 1000000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000000f)).ToString("N2") + "QD";

            return numbStr;
        }

        public static string ConvertIntToText(int num)
        {
            string numbStr = "";

            if ((float)(num / 1000f) <= 1)
                numbStr = num.ToString("N0") + " M";
            else if ((float)(num / 1000000f) <= 1)
                numbStr = ((float)(num / 1000f)).ToString("N2") + " B";
            else if ((float)(num / 1000000000f) <= 1)
                numbStr = ((float)(num / 1000000f)).ToString("N2") + " T";

            return numbStr;
        }

        public static string ConvertPlanetRankEnum(PlanetData.ePlanetRank rank)
        {
            switch (rank)
            {
                case PlanetData.ePlanetRank.EstablishedColony:
                    return "Established Colony";
                case PlanetData.ePlanetRank.ImperialCapital:
                    return "Civilization Capital";
                case PlanetData.ePlanetRank.NewColony:
                    return "New Colony";
                case PlanetData.ePlanetRank.ProvinceCapital:
                    return "Provincial Capital";
                case PlanetData.ePlanetRank.SystemCapital:
                    return "System Capital";
                case PlanetData.ePlanetRank.Uninhabited:
                    return "Uninhabited";
                default:
                    return "Unknown Rank";
            }
        }

        public static Color GetTextValueColor(int number)
        {
            if (number < 15)
                return new Color(.48f,.1f,.1f);
            else if (number < 30)
                return Color.red;
            else if (number < 40)
                return new Color(1,.6f,0);
            else if (number < 55)
                return Color.yellow;
            else if (number < 70)
                return Color.cyan;
            else if (number < 85)
                return Color.green;
            else
                return Color.white;
        }       
    
}

    public static class Formulas
    {
        public static float MeasureDistanceBetweenSystems(StarData star1, StarData star2)
        {
            float distance = 0f;
            float a = 0f;
            float b = 0f;
            float h = 0f;

            a = Mathf.Abs(star1.WorldLocation.x - star2.WorldLocation.x); // x distance
            b = Mathf.Abs(star1.WorldLocation.y - star2.WorldLocation.y); // y distance
            h = Mathf.Pow(a, 2f) + Mathf.Pow(b, 2f);
            distance = Mathf.Sqrt(h);

            return distance;

        }

        public static float MeasureDistanceBetweenLocations(Vector3 homeObject, Vector3 distantObject)
        {
            float distance = 0f;
            float a = 0f;
            float b = 0f;
            float h = 0f;

            a = Mathf.Abs(homeObject.x - distantObject.x); // x distance
            b = Mathf.Abs(homeObject.y - distantObject.y); // y distance
            h = Mathf.Pow(a, 2f) + Mathf.Pow(b, 2f);
            distance = Mathf.Sqrt(h);

            return distance;

        }

        private static readonly System.Random random = new System.Random();
        //private static readonly object syncLock = new object();

        public static int GetRandomInt(int min, int max)
        {
            //lock(syncLock)
           // {
                return random.Next(min, max);
           // }
        }
    }    

    public static class DataRetrivalFunctions
    {
        public static Color FindPlanetOwnerColor(PlanetData pData)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Color pColor = Color.white; // base unowned color

            foreach (Civilization civ in gameDataRef.CivList)
            {
                for (int x = 0; x < civ.PlanetIDList.Count; x++)
                {
                    if (civ.PlanetIDList[x] == pData.ID) // will need to change
                    {
                        pColor = civ.Color;
                        return pColor;
                    }
                }
            }

            return pColor;
        }

        public static float DetermineContributionToProject(Character cData, Project pData)
        {
            float moneyAvail = cData.Wealth; // money available
            Relationship attitude = cData.Relationships[cData.Civ.LeaderID]; // relationship with you 
            float baseContribution = 0f; // the base contribution (25%)
            float adminAdjustment = 0f; // the adjustment made with the type of admin selected

            baseContribution = .25f; // the base contribution of all characters

            // step 1: determine their base contribution based on their traits
            if (cData.Traits.Exists(p => p.Name == "Traditionalist"))
                baseContribution -= .3f;

            if (cData.Traits.Exists(p => p.Name == "Reformist"))
                baseContribution += .7f;

            if (cData.Traits.Exists(p => p.Name == "Egoist"))
                baseContribution += .3f;

            if (cData.Traits.Exists(p => p.Name == "Ascetic"))
                baseContribution -= .3f;

            if (cData.Traits.Exists(p => p.Name == "Avaricious"))
                baseContribution += .3f;

            if (cData.Traits.Exists(p => p.Name == "Generous"))
                if (pData.BenevolentEffect > 5)
                    baseContribution += .7f;
                else if (pData.TyrannicalEffect > 5)
                    baseContribution -= .6f;

            if (cData.Traits.Exists(p => p.Name == "Spender"))
                baseContribution = 1f; // spending to the max

            if (cData.Traits.Exists(p => p.Name == "Populist"))
                if (pData.BenevolentEffect > 5)
                    baseContribution += .7f;
                else if (pData.TyrannicalEffect > 5)
                    baseContribution -= .6f;

            if (cData.Traits.Exists(p => p.Name == "Technophile"))
                if (pData.Type == Project.eProjectType.Demographic)
                    baseContribution += .8f;
                else
                    baseContribution -= .2f;

            if (cData.Traits.Exists(p => p.Name == "Cruel"))
                if (pData.BenevolentEffect > 5)
                    baseContribution -= .7f;
                else if (pData.TyrannicalEffect > 5)
                    baseContribution += .6f;

            if (cData.Traits.Exists(p => p.Name == "Psychopath"))
                if (pData.BenevolentEffect > 5)
                    baseContribution -= 1f;
                else if (pData.TyrannicalEffect > 5)
                    baseContribution += 1f;

            if (cData.Traits.Exists(p => p.Name == "Erratic"))
                baseContribution = UnityEngine.Random.Range(0, 1f);

            if (cData.Traits.Exists(p => p.Name == "Bureaucrat"))
                baseContribution += .5f;

            if (cData.Traits.Exists(p => p.Name == "Sybarite"))
                if (pData.BenevolentEffect > 2)
                    baseContribution -= .3f;
                else if (pData.TyrannicalEffect > 2)
                    baseContribution += .4f;

            // step 2: Now adjust based on their relationship towards you
            float charAdjustment = DetermineMoneyContributionRelationshipAdjustment(attitude);

            // step 3: Now adjust based on their attributes (specifically power)
            baseContribution += (cData.Power - 50) / 100;
            baseContribution += (cData.Drive - 50) / 150;

            // step 4: determine based on the administrator
            if (pData.AdministratorID != null)
            {
                Relationship adminAttitude = cData.Relationships[pData.AdministratorID];
                adminAdjustment = DetermineMoneyContributionRelationshipAdjustment(adminAttitude);
            }

            // step 4: final calculation and return
            baseContribution = baseContribution + charAdjustment + adminAdjustment;

            // normalize for actual wealth
            if (cData.Wealth > 0f)
            {
                if ((baseContribution * pData.BaseCostReq) > cData.Wealth)
                    baseContribution = (cData.Wealth / pData.BaseCostReq);
            }
            else
                baseContribution = 0f;          

            // normalize for below 0 contributions
            if (baseContribution < 0f)
                baseContribution = 0f;

            if (baseContribution > 1f)
                baseContribution = 1f;        

            return baseContribution;

        }

        private static float DetermineMoneyContributionRelationshipAdjustment(Relationship attitude)
        {
            float charAdjustment = 0f;
            switch (attitude.RelationshipState)
            {
                case Relationship.eRelationshipState.None:
                    break;
                case Relationship.eRelationshipState.Allies:
                    charAdjustment += .15f;
                    break;
                case Relationship.eRelationshipState.Friends:
                    charAdjustment += .25f;
                    break;
                case Relationship.eRelationshipState.Superior:
                    charAdjustment += .2f;
                    break;
                case Relationship.eRelationshipState.Inferior:
                    charAdjustment += .8f;
                    break;
                case Relationship.eRelationshipState.Challenger:
                    charAdjustment -= .2f;
                    break;
                case Relationship.eRelationshipState.Challenged:
                    charAdjustment -= .3f;
                    break;
                case Relationship.eRelationshipState.Rival:
                    charAdjustment = -1f;
                    break;
                case Relationship.eRelationshipState.Shunning:
                    charAdjustment = -.5f;
                    break;
                case Relationship.eRelationshipState.Shunned:
                    charAdjustment = .5f;
                    break;
                case Relationship.eRelationshipState.Vengeance:
                    charAdjustment = -1f;
                    break;
                case Relationship.eRelationshipState.ObjectOfVengeance:
                    charAdjustment = -1f;
                    break;
                case Relationship.eRelationshipState.Vendetta:
                    charAdjustment = -1f;
                    break;
                case Relationship.eRelationshipState.Married:
                    charAdjustment = 1f;
                    break;
                case Relationship.eRelationshipState.Lovers:
                    charAdjustment = 1f;
                    break;
                case Relationship.eRelationshipState.HangerOn:
                    charAdjustment += .6f;
                    break;
                case Relationship.eRelationshipState.HungUpon:
                    charAdjustment += .1f;
                    break;
                case Relationship.eRelationshipState.Patron:
                    charAdjustment = .8f;
                    break;
                case Relationship.eRelationshipState.Protegee:
                    charAdjustment = .8f;
                    break;
                case Relationship.eRelationshipState.Predator:
                    charAdjustment = -1f;
                    break;
                case Relationship.eRelationshipState.Prey:
                    charAdjustment = -1f;
                    break;
                default:
                    break;
            }
            return charAdjustment;
        }

        public static float DetermineAdminAvailable(Character cData)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            float baseADM = 0f;
            int adminRating = cData.Administration;

            if (cData.Role == Character.eRole.Viceroy)
            {
                baseADM = cData.PlanetAssigned.TotalAdmin * (adminRating / 10f);
            }
            else if (cData.Role == Character.eRole.SystemGovernor)
            {
                foreach (PlanetData pData in cData.SystemAssigned.PlanetList)
                {
                    if (pData.Owner != null)
                    {
                        if (pData.Owner.ID == cData.AssignedHouse.SwornFealtyCivID)
                        {
                            if (!pData.Viceroy.HasActiveProject)
                                baseADM += pData.TotalAdmin;
                        }
                    }
                }

                baseADM = baseADM * (adminRating / 10f);
            }
            else if (cData.Role == Character.eRole.ProvinceGovernor)
            {
                foreach (StarData sData in cData.ProvinceAssigned.SystemList)
                {
                    foreach (PlanetData pData in sData.PlanetList)
                    {
                        if (pData.Owner != null)
                        {
                            if (pData.Owner.ID == cData.AssignedHouse.SwornFealtyCivID)
                            {
                                if (!pData.Viceroy.HasActiveProject)
                                    baseADM += pData.TotalAdmin;
                            }
                        }
                    }
                }

                baseADM = baseADM * (adminRating / 10f);
            }

            return baseADM;
        }

        public static Project GetProjectData(string pID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Project pData = new Project();

            pData = gameDataRef.ProjectDataList.Find(p => p.ID == pID);
            return pData;
        }

        public static Color FindProvinceOwnerColor(Province pData)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            GalaxyData galDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            Color pColor = Color.white; // base unowned color

            foreach (Civilization civ in gameDataRef.CivList)
            {           
                if (pData.OwningCivID == civ.ID)
                {
                    pColor = civ.Color;
                    return pColor;
                }               
            }

            return pColor;
        }

        public static Character GetCharacter(string cID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Character cData = new Character();

            cData = gameDataRef.CharacterList.Find(p => p.ID == cID);
            return cData;
        }

        public static Character GetPrime(Character.eRole charRole)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == charRole)
                {
                    return charData;
                }
            }

            return null; 
        }

        public static List<House> GetHouseList()
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            return gameDataRef.HouseList;
        }

        public static House GetHouse(string hID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            return gameDataRef.HouseList.Find(p => p.ID == hID);
        }

        public static string GetProvinceGovernorID(string sID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.ProvinceGovernor && charData.ProvinceAssignedID == sID)
                {
                    return charData.ID;
                }
            }

            return "none"; 
        }

        public static Character GetCivLeader(string cID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.Emperor && charData.CivID == cID)
                {
                    return charData;
                }
            }

            return null;
        }

        public static List<Pops> GetCivPopList(string cID)
        {          
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Civilization civ = gameDataRef.CivList.Find(p => p.ID == cID);
            return civ.PopList;           
        }

        public static string GetSystemGovernorID(string sID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
         
            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.SystemGovernor && charData.SystemAssignedID == sID)
                {         
                    return charData.ID;
                }
            }

            return "none"; // returns the first character if can't find the governor, thus error         
        }

        public static string GetPlanetViceroyID(string pID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.Viceroy && charData.PlanetAssignedID == pID)
                {
                    return charData.ID;
                }
            }

            return "none"; ; // returns the first character if can't find the governor, thus error         
        }

        public static String FindOwnerName(PlanetData pData)
        {
            string oName; // base unowned name
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();

            foreach (Civilization civ in gameDataRef.CivList)
            {
                for (int x = 0; x < civ.PlanetIDList.Count; x++)
                {
                    if (civ.PlanetIDList[x] == pData.ID) // will need to change
                    {
                        oName = civ.Name;
                        return oName.ToUpper();
                    }
                }
            }

            return "UNKNOWN DESIGNATION";
        }

        public static List<StarData> GetCivSystemList(Civilization civ)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            List<StarData> civStarList = new List<StarData>();

            foreach (PlanetData planet in galaxyDataRef.GalaxyPlanetDataList)
            {
                if (civ.PlanetIDList.Exists(p => p == planet.ID))
                {
                    StarData star = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == planet.SystemID);

                    if (!civStarList.Contains(star))
                        civStarList.Add(star);
                }
            }

            return civStarList;
        }

        public static GameData GetGameDataObject()
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            return gameDataRef;
        }

        public static Civilization GetCivilization(string ID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            if (gameDataRef.CivList.Exists(p => p.ID == ID))
                return gameDataRef.CivList.Find(p => p.ID == ID);
            else
                return null;
        }

        public static List<PlanetData> GetCivPlanetList(Civilization civ)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            List<PlanetData> civPlanetList = new List<PlanetData>();

            foreach (PlanetData planet in galaxyDataRef.GalaxyPlanetDataList)
            {
                if (civ.PlanetIDList.Exists(p => p == planet.ID))
                {                 
                    civPlanetList.Add(planet);
                }
            }

            return civPlanetList;
        }

        public static List<Province> GetCivProvinceList(Civilization civ)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            List<Province> civProvinceList = new List<Province>();

            foreach (Province prov in galaxyDataRef.ProvinceList)
            {
                if (prov.OwningCivID == civ.ID)
                {
                    civProvinceList.Add(prov);
                }
            }

            return civProvinceList;
        }

        public static StarData GetCivHomeSystem(Civilization civ)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            PlanetData pData = new PlanetData();
            StarData sData = new StarData();

            pData = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == civ.CapitalPlanetID);
            sData = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == pData.SystemID);

            return sData;
        }

        public static List<StarData> GetCivSystems(Civilization civ)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            PlanetData pData = new PlanetData();
            List<PlanetData> pDataList = new List<PlanetData>();
            List<StarData> sDataList = new List<StarData>();

            foreach (string ID in civ.PlanetIDList)
            {
                pData = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == ID);
                if (!sDataList.Exists(p => p.ID == pData.SystemID))
                    sDataList.Add(galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == pData.SystemID));
            }
            
            return sDataList;
        }

        public static List<StarData> GetProvinceSystems(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            StarData sData = new StarData();
            List<StarData> sDataList = new List<StarData>();

            sDataList = galaxyDataRef.GalaxyStarDataList.FindAll(p => p.AssignedProvinceID == ID);
            return sDataList;
        }

        public static PlanetData GetPlanet(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            PlanetData pData = new PlanetData();

            pData = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == ID);
            return pData;
        }

        public static Challenge GetChallenge(string ID)
        {
            GameData gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Challenge cData = new Challenge();

            cData = gameDataRef.ChallengeList.Find(p => p.ChallengeID == ID);
            return cData;
        }

        public static StarData GetSystem(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            StarData sData = new StarData();

            sData = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == ID);
            return sData;
        }

        public static Province GetProvince(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            Province prov = new Province();

            prov = galaxyDataRef.ProvinceList.Find(p => p.ID == ID);
            return prov;
        }

        public static Region GetRegion(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            Region rData = new Region();

            rData = galaxyDataRef.GalaxyRegionDataList.Find(p => p.ID == ID);
            return rData;
        }
    }

    
}

