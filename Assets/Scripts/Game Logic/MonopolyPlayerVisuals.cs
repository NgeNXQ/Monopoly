using UnityEngine;

[System.Serializable]
public sealed class MonopolyPlayerVisuals
{
    [SerializeField] private Color colorPlayerToken;

    [SerializeField] private Sprite spritePlayerToken;

    public Color ColorPlayerToken 
    {
        get => this.colorPlayerToken;
    }

    public Sprite SpritePlayerToken 
    {
        get => this.spritePlayerToken;
    }
}
