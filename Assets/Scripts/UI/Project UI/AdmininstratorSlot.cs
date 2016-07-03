using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class AdmininstratorSlot : MonoBehaviour, IDropHandler
{
    public GameObject item
    {
        get
        {
            if (transform.tag == "Filled Slot")
            {             
                transform.tag = "Empty Slot";
                return transform.GetChild(1).gameObject;
            }
            return null;
        }
    }

    public void Update()
    {
        if (transform.tag == "Empty Slot")
            transform.Find("Empty_Admin_Container").gameObject.SetActive(true);

        if (transform.tag == "Filled Slot")
            transform.Find("Empty_Admin_Container").gameObject.SetActive(false);

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
            DragHandler.itemBeingDragged.transform.Find("Portrait_Container/Icon_House").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.Find("Portrait_Container/Icon_Relationship").gameObject.SetActive(false);
            DragHandler.itemBeingDragged.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transform.GetComponent<RectTransform>().rect.width);
            DragHandler.itemBeingDragged.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transform.GetComponent<RectTransform>().rect.height);
            DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().isAdminstrator = true; // set the card status to contributor
            DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().isContributor = true; // set the card status to contributor
            HelperFunctions.DataRetrivalFunctions.GetCharacter(DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().CharID).HasActiveProject = true;
            DragHandler.itemBeingDragged.transform.localPosition = new Vector2(0, 0);           
            transform.tag = "Filled Slot";
            ExecuteEvents.ExecuteHierarchy<IAdminUpdated>(gameObject, null, (x, y) => x.UpdateAdministrator(DragHandler.itemBeingDragged.transform.GetComponent<CharacterCard>().CharID));
        }
    }
}
  
