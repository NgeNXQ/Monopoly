using UnityEngine;
using UnityEngine.UI;

public sealed class UIHandler : MonoBehaviour
{
    [SerializeField] private Button buttonRollDices;

    [SerializeField] private Transform panelPlayers;

    [SerializeField] private Transform panelPlayer;

    public static UIHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        this.buttonRollDices.onClick.AddListener(GameManager.Instance.RollDices);
    }

    public void ActivateButtonRollDices()
    {
        this.buttonRollDices.interactable = true;
    }

    public void DeactivateButtonRollDices()
    {
        this.buttonRollDices.interactable = false;
    }

    public void ShowButtonRollDices()
    { 
        this.buttonRollDices.gameObject.SetActive(true);
        this.buttonRollDices.interactable = true;
    }

    public void HideButtonRollDices()
    {
        this.buttonRollDices.gameObject.SetActive(false);
        this.buttonRollDices.interactable = false;
    }

    private void OnDestroy()
    {
        buttonRollDices.onClick.RemoveListener(GameManager.Instance.RollDices);
    }
}
