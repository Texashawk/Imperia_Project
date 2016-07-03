using UnityEngine;
using UnityEngine.EventSystems;
using HelperFunctions;

public class DestroyCharacterCardOnRightClick : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && (GetComponent<CharacterCard>().isAdminstrator == true || GetComponent<CharacterCard>().isContributor == true))
        {           
            DataRetrivalFunctions.GetCharacter(GetComponent<CharacterCard>().CharID).HasActiveProject = false;
            if (GetComponent<CharacterCard>().isAdminstrator == true)
            {
                GetComponent<CharacterCard>().isAdminstrator = false;
                GetComponent<CharacterCard>().isContributor = false;
                ExecuteEvents.ExecuteHierarchy<IHasBeenDeleted>(gameObject, null, (x, y) => x.HasBeenDeleted("Administrator", GetComponent<CharacterCard>().CharID));
            }
            else
            {
                GetComponent<CharacterCard>().isContributor = false;
                GetComponent<CharacterCard>().isAdminstrator = false;
                ExecuteEvents.ExecuteHierarchy<IHasBeenDeleted>(gameObject, null, (x, y) => x.HasBeenDeleted("Contributor", GetComponent<CharacterCard>().CharID));
            }

            Destroy(gameObject);
        }
    }
}
