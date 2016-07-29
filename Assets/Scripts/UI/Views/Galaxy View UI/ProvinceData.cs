using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProvinceData : MonoBehaviour
{

    public float ObjectRotation { get; set; }
    public Province ProvinceInfo { get; set; }
    public string OwnerName { get; set; }
    public Rect ProvinceBounds { get; set; }
    public Vector3 ProvinceObjectLocation { get; set; }
    public Color OwnerColor { get; set; }

    public GameObject GovernorPic;
    Image governorPic;

    GraphicAssets gAssetRef;
    GameData gDataRef;

	// Use this for initialization
	void Awake ()
    {
        gAssetRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        governorPic = GovernorPic.GetComponent<Image>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (ProvinceInfo != null)
        {
            if (ProvinceInfo.Governor != null)
            {
                governorPic.sprite = gAssetRef.CharacterList.Find(p => p.name == ProvinceInfo.Governor.PictureID);
            }
        }
	}
}
