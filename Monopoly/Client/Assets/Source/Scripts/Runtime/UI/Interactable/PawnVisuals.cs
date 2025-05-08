using UnityEngine;

[System.Serializable]
internal sealed class PawnVisuals
{
    [SerializeField]
    private Color pawnTokenColor;

    [SerializeField]
    private Sprite pawnTokenSprite;

    internal Color PawnTokenColor => this.pawnTokenColor;
    internal Sprite PawnTokenSprite => this.pawnTokenSprite;
}
