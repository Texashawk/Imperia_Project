using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// Use for any slot where a droppable object will be 'dropped' into
public class ContributorSlot : MonoBehaviour, IDropHandler {
    public GameObject item {
        get {
            if (transform.tag == "Filled Slot")
            {             
                transform.tag = "Empty Slot";
                return transform.GetChild(0).gameObject;
            }           
            return null;
        }

        
    }

    public GameObject PowerBar;

    public void Awake()
    {
        
    }

    public void Update()
    {
        if (transform.tag == "Empty Slot")
        {
            transform.Find("Portrait_Container").gameObject.SetActive(true);
            PowerBar.SetActive(false);
        }

        if (transform.tag == "Filled Slot")
        {
            PowerBar.SetActive(true);
            transform.Find("Portrait_Container").gameObject.SetActive(false);         
        }

        if (transform.childCount < 2) // needs the child for the card and one for the empty container
        {
            transform.tag = "Empty Slot";
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!item)
        {
            DragHandler.itemBeingDragged.transform.SetParent(transform); // assign the item being dragged to the slot
            DragHandler.itemBeingDragged.transform.Find("Portrait_Container").gameObject.SetActive(true);
            DragHandler.itemBeingDragged.transform.Find("Stats_Container").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.Find("Outline").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.Find("Name").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.Find("Portrait_Container/Icon_House").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.Find("Portrait_Container/Icon_Relationship").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transform.GetComponent<RectTransform>().rect.width);
            DragHandler.itemBeingDragged.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transform.GetComponent<RectTransform>().rect.height);
            HelperFunctions.DataRetrivalFunctions.GetCharacter(DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().CharID).HasActiveProject = true;
            DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().isContributor = true; // set the card status to contributor
            DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().isAdminstrator = false; // but not an administrator!
            DragHandler.itemBeingDragged.transform.Find("Portrait_Container").transform.localPosition = new Vector2(0,0);
            DragHandler.itemBeingDragged.transform.localPosition = new Vector2(0, 0);
            DragHandler.itemBeingDragged.transform.localScale = new Vector2(.7f, .7f);
            transform.tag = "Filled Slot";
            ExecuteEvents.ExecuteHierarchy<IContributorUpdated>(gameObject, null, (x, y) => x.UpdateContributor(DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().CharID));
            PowerBar.SetActive(true);
            PowerBar.transform.Find("Counter_Funding").GetComponent<TextMeshProUGUI>().text = "+5"; // test
        }
    }

}

