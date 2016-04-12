using UnityEngine;
using StellarObjects;

public class TradeViewSystemData : MonoBehaviour

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
    [HideInInspector]
    public GameObject planetCountObject; // future UI object for additional information
    [HideInInspector]
    public Vector3 objectLocation; // where the block is
    public Color ownerColor; // civ ownership color   
    public string ownerName; // civ name 
    public string ownerTolerance; // civ planet tolerance
    public float objectRotation; // rotation of the block (for constellations and nebulas)
}
