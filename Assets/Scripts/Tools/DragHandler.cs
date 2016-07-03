using UnityEngine;
using UnityEngine.EventSystems;

// this script should be attached to anything that will be dragged and dropped into a Slot (Slot needs a Slot script attached)

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemBeingDragged; // static because only one item will be dragged
    private RectTransform draggingPlane;
    Vector3 startPosition; // start of the object
    public bool IsEnabled = true; // is the handler enabled?
    Transform startParent; // what's the original parent of the object? compare

    public void OnDrag(PointerEventData eventData)
    {
        if (IsEnabled)
        {
            SetDraggedPosition(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEnabled)
        {
            itemBeingDragged = gameObject; // only select if draggable
            startPosition = transform.position;
            startParent = transform.parent;
            startParent.tag = "Empty Slot";
            draggingPlane = transform as RectTransform;

            GetComponent<CanvasGroup>().blocksRaycasts = false; // add a Canvas Group to the object being dragged!
            transform.SetAsLastSibling(); // brings to front
        }    
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            draggingPlane = data.pointerEnter.transform as RectTransform;

        var rt = itemBeingDragged.GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = draggingPlane.rotation;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsEnabled)
        {
            itemBeingDragged = null;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            transform.position = startPosition;

            if (transform.parent != startParent)
            {
                transform.position = startPosition;
                IsEnabled = false;
            }
            else
            {
                startParent.tag = "Filled Slot";              
            }           
        }
    }
}



