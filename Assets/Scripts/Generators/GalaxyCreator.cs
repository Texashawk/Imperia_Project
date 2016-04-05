using UnityEngine;
using System;

// object namespaces
using StellarObjects;

namespace GalaxyCreator
{
    public class GalaxyCreator : MonoBehaviour
    {      
        private GalaxyData gData;
        private GameData gameDataRef;       
        private int galaxySizeWidth;
        private int galaxySizeHeight;
        private static readonly System.Random rand = new System.Random();

        private const int SpaceBetweenStars = 300; // the minimum space between star objects

        void Awake()
        {
            //gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            //gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        }

        void Start()
        {
            // data structure references
            gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            galaxySizeWidth = gameDataRef.GalaxySizeWidth; // these will be selected in new game screen
            galaxySizeHeight = gameDataRef.GalaxySizeHeight;
            GenerateNebulas();
            GenerateStars();
            GeneratePlanets();
        }

        void GenerateStars()
        {
            GenerateHumanHomeStar(); // generate the human home system

            int starCount = gameDataRef.TotalSystems;
            for (int i = 0; i < starCount; i++)
            {
                Vector3 starLoc = DetermineStarLocation();
                if (starLoc == new Vector3(-1, -1, -1)) // if the location is invalid, don't create the star, continue the loop
                    continue;

                StarData newStar = new StarData();
                newStar = GenerateGameObject.CreateNewStar(); // this creates the star and adds the data/accessor components

                // if the star type is 'no star', break this loop, and if it is too high (no specials or wolf rayet yet) no star either (yet)               
                if ((int)newStar.SpectralClass >= 12)
                    newStar.SpectralClass = StarData.eSpectralClass.NoStar;

                if (newStar.SpectralClass == 0)
                {
                    continue;
                }
                newStar.SetWorldLocation(starLoc);
                gData.AddStarDataToList(newStar);
            }

            gData.galaxyIsGenerated = true; // add more checks here
        }

        Vector3 DetermineStarLocation()
        {
            bool locIsValid = true; // flag to show whether location is valid
            int placementTries = 0; // after 100 tries, place and move on
            Vector3 proposedLocation = new Vector3();

            if (gData.GalaxyStarDataList.Count == 0) // if nothing in the list, obviously any location will work! 
                proposedLocation = GenerateLocation();           
                
            else
            {
            restart:
                placementTries += 1;
                proposedLocation = GenerateLocation(); // generate a new tentative location

                foreach (StarData starLoc in gData.GalaxyStarDataList)
                {                  
                    locIsValid = CheckLocation(starLoc.WorldLocation, proposedLocation);

                    if (!locIsValid && placementTries < 10000) // if an overlap is found      
                        goto restart; // restarts the loop from the beginning once a valid location is generated
                    else if (placementTries == 10000)
                    {
                        Debug.Log("Error; could not find suitable location for star within parameters. Star not valid.");
                        return new Vector3(-1,-1,-1); // destroy the star by error coordinates
                    }
                }
            }
           
            return proposedLocation;
        }

        Vector3 GenerateLocation()
        {          
            Vector3 pLoc;
            pLoc.x = rand.Next(-galaxySizeWidth, galaxySizeWidth);
            pLoc.y = rand.Next(-galaxySizeHeight, galaxySizeHeight);
            pLoc.z = 0;

            return pLoc;
        }

        bool CheckLocation(Vector2 sLoc, Vector2 pLoc)
        {
            if (Math.Abs(pLoc.x - sLoc.x) < SpaceBetweenStars) //&& pLoc.x > sLoc.x - SpaceBetweenStars) // x too close? return false
                return false;

            if (Math.Abs(pLoc.y - sLoc.y) < SpaceBetweenStars) // && pLoc.y > sLoc.y - SpaceBetweenStars) // y too close? return true
                return false;

            return true; // true if all checks pass
        }
            

        void GenerateHumanHomeStar()
        {
            // create New Terra
            Vector3 starLoc = new Vector3(0, 0, 0);
            StarData newStar = new StarData();

            newStar.SpectralClass = StarData.eSpectralClass.G;
            newStar.Age = 7;
            newStar.starMultipleType = StarData.eStarMultiple.Single;
            newStar.Size = 50;
            newStar.Metallicity = 8.0;
            newStar.Name = "Neo-Sirius";
            newStar.ID = "STANEOS001";

            newStar.SetWorldLocation(starLoc);
            gData.AddStarDataToList(newStar);
        }

        void GeneratePlanets()
        {
            // populate the planet tables
            GenerateGameObject.PopulatePlanetGenerationTables();
            GenerateGameObject.PopulateRegionTypeTables();

            // populate the planet trait and region data lists
            gData.PopulatePlanetTraitList(DataManager.planetTraitDataList);  // move the static version to a public version? may not need this
            gData.PopulateRegionDataList(GenerateGameObject.regionTypeDataList);

            // run the planet generation routines for each star
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                GenerateGameObject.CreateSystemPlanets(star);
            }
        }

        void GenerateNebulas()
        {
            int nebulaCount = rand.Next(0, 3);
            for (int x = 0; x < nebulaCount; x++)
            {
                GenerateGameObject.GenerateNebula();
            }
        }

        void AddPlanetDataToGalaxyManager()
        {
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                foreach (PlanetData planet in star.PlanetList)
                    gData.AddPlanetDataToList(planet);
            }
        }
    }
}
