using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIMonopolyNodeCard : MonoBehaviour
{
    [SerializeField] private Image imageMonopolyNode;

    [SerializeField] private TMP_Text textDescription;

    public void UpdateUI(Sprite sprite, string text)
    {
        this.textDescription.text = text;
        this.imageMonopolyNode.sprite = sprite;
    }
}
