using UnityEngine;
using Managers;
using UnityEngine.EventSystems;

public class ProjectScreenScript : MonoBehaviour, IPointerClickHandler

{
    UIManager uiManagerRef;
	// Use this for initialization
	void Awake ()
    {
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
    }
	
	// Update is called once per frame
	void Update ()
    {
	   
	}

    public void OnPointerClick(PointerEventData pEvent)
    {
        if (pEvent.button == PointerEventData.InputButton.Right)
        {
            uiManagerRef.ModalIsActive = false; // reset modal set
            Destroy(gameObject);
        }
            
    }
}
