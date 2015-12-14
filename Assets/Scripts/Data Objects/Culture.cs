using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Culture : MonoBehaviour {

	public enum eLocationRing : int
    {
        Inner,
        Middle,
        Outer
    }

    public eLocationRing LocationRing { get; set; }
    public string Name { get; set; }
    public string AssociatedHouseIDs { get; set; }
    public string Description { get; set; }
    public List<string> FirstNameList { get; set; }
    public List<string> LastNameList { get; set; }
    public List<string> PlanetNameList { get; set; }
    public List<string> CustomPlanetNames { get; set; }

	void Start ()
    {
	
	}	
	
}
