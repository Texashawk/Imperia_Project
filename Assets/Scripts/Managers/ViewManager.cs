using UnityEngine;

namespace Managers
{
    public class ViewManager : MonoBehaviour
    {
        public enum eSecondaryView : int
        {
            None,
            Trade,
            Military,
            Sovereignity,
            Intel,
            Financial,
            Morale,
            Unrest,
            Biological,
            Diplomatic
        }

        public enum ePrimaryView : int
        {
            Economic,
            Political,
            Military,
            Pops
        }

        public enum eViewLevel : int
        {
            Galaxy,
            Province,
            System,
            Planet
        }

        public eSecondaryView SecondaryViewMode { get; set; } // which 'filter' is active in the galaxy view
        public ePrimaryView PrimaryViewMode { get; set; } // which 'sub mode' is active on the buttons (lower-right)
        public eViewLevel ViewLevel { get; set; } // which level of 'zoom' the game is at (galaxy, province, system, planet)
    }
}
