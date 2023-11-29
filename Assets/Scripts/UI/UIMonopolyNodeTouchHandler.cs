using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIMonopolyNodeTouchHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData = null)
    {
        //eventData.
        Debug.Log("UI Element Touched!");
    }
}
