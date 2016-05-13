using System.Collections.Generic;

public class Culture
{ 

	public enum eLocationRing : int
    {
        Inner,
        Middle,
        Outer
    }

    public eLocationRing LocationRing { get; set; }
    public string Name { get; set; }
    public string AssociatedHouseIDs { get; set; }
    public string ImageID { get; set; } // the ID of the image/icon/glyph for this culture
    public string Description { get; set; }
    public List<string> FirstNameList { get; set; }
    public List<string> LastNameList { get; set; }
    public List<string> PlanetNameList { get; set; }
    public List<string> CustomPlanetNames { get; set; }
    public Dictionary<Idea, int> IdeaLevels = new Dictionary<Idea, int>();
	
}
