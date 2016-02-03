using CharacterObjects;
using CivObjects;
using ConversationAI;
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

        public string Name { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public eProjectScope Scope { get; set; }
        public float BaseADMReq { get; set; }
        public float BaseAlphaReq { get; set; }
        public float BaseHeavyReq { get; set; }
        public float BaseRareReq { get; set; }
        public float BaseCostMonth { get; set; }
        public float TyrannicalEffect { get; set; }
        public float BenevolentEffect { get; set; }
    }
}