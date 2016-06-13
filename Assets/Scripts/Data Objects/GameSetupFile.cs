using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



public class GameSetupFile
{
    public enum eQuadrantSize : int
    {
        Small,
        Medium,
        Large
    };

    public string SetUpFileID { get; set; }
    public string EmpireName { get; set; }
    public int EmpireWidth { get; set; }
    public eQuadrantSize QuadrantSizeSelect { get; set; }
    public string EmpireCrestID { get; set; }
}
