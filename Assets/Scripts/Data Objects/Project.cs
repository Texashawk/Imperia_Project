using CharacterObjects;
using CivObjects;
using ConversationAI;
using Managers;
using System.Collections.Generic;

namespace Projects
{
    public class Project
    {
        public enum eProjectScope : int
        {
            Planet,
            System,
            Province,
            Empire
        }

        public enum eProjectType : int
        {
            Economic,
            Military,
            Political,
            Demographic
        }

        public string Name { get; set; }
        public eProjectType Type { get; set; } // the type of Project (military, economic, astrographic, demographic)
        public string ID { get; set; }
        public string Description { get; set; }
        public string IconName { get; set; } // the icon name used for this Project
        public string ProjectHouseID { get; set; } // which house the project is being created for
        public eProjectScope Scope { get; set; } // planet, system, province, empire
        public float BaseADMReq { get; set; } // base total ADM required
        public float BaseEnergyReq { get; set; } // base energy requirement from House coffer
        public float BaseBasicReq { get; set; } // etc.
        public float BaseHeavyReq { get; set; }
        public float BaseRareReq { get; set; }
        public float BaseCostReq { get; set; } // base total cost of the Project that must be contributed
        public float BasePrestige { get; set; } // how prestigious this project is; translates to power gain
        public float TyrannicalEffect { get; set; }
        public float BenevolentEffect { get; set; }
        public Dictionary<string, float> CharactersInProject = new Dictionary<string, float>();

        public bool IsActionValid(ViewManager.eViewLevel viewMode)
        {
            if (viewMode == ViewManager.eViewLevel.System && Scope == eProjectScope.System)
                return true;
            if (viewMode == ViewManager.eViewLevel.Planet && Scope == eProjectScope.Planet)
                return true;
            if (viewMode == ViewManager.eViewLevel.Province && Scope == eProjectScope.Province)
                return true;

            return false;
        }

    }
}