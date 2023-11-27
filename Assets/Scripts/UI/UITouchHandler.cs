using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UITouchHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("UI Element Touched!");
    }
}
