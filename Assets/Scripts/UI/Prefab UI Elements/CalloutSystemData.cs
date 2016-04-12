using UnityEngine;
using StellarObjects;

public class CalloutSystemData : MonoBehaviour

{
    public enum eBlockType : int
    {
        Star,
        Nebula,
        Province,
        Lower
    }

    public eBlockType blockType = eBlockType.Star;
    public string starID = "";
    public StarData starData;
    public string starDataName;
    public Transform starTransform;
    public Vector3 objectLocation; // where the block is
    public Color ownerColor; // civ ownership color   
    public string ownerName; // civ name 
   
}
