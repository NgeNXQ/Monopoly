using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal sealed class UIManagerMonopolyGame : MonoBehaviour
{
    #region Setup

    #region Visuals

    #region Dice

    [Header("Button Roll Dice")]

    [Space]
    [SerializeField] private Button buttonRollDice;

    [Space]
    [Header("Images \"Dices\"")]
    [Space]

    [Space]
    [SerializeField] private Image imageDiePlaceholder1;

    [Space]
    [SerializeField] private Image imageDiePlaceholder2;

    [Space]
    [SerializeField] private Sprite[] spriteDieFaces = new Sprite[6];

    [Header("Dice screen time")]

    [Space]
    [SerializeField][Range(0.0f, 10.0f)] private float diceScreenTime = 1.0f;

    #endregion

    #region Currency

    [Space]
    [Header("Currency")]

    [Space]
    [SerializeField] private char currency;

    public char Currency { get => this.currency; }

    #endregion

    #region Panel Players

    [Space]
    [Header("Panel Players")]

    [Space]
    [SerializeField] private Canvas canvasPlayersList;

    #endregion

    #endregion

    #region Messages

    [Space]
    [Header("Messages")]

    [Space]
    [SerializeField] private string messageWon;

    [Space]
    [SerializeField] private string messageAlreadyBuilt;

    [Space]
    [SerializeField] private string messageInsufficientFunds;

    [Space]
    [SerializeField] private string messageWaitingOtherPlayers;

    [Space]
    [SerializeField] private string messagePlayersFailedToLoad;

    [Space]
    [SerializeField] private string messageCannotUpgradeMaxLevel;

    [Space]
    [SerializeField] private string messageCannotDowngradeMinLevel;

    [Space]
    [SerializeField] private string messageOnlyEvenBuildingAllowed;

    [Space]
    [SerializeField] private string messageCompleteMonopolyRequired;

    public string MessageWon 
    {
        get => this.messageWon;
    }

    public string MessageAlreadyBuilt 
    { 
        get => this.messageAlreadyBuilt; 
    }

    public string MessageInsufficientFunds 
    { 
        get => this.messageInsufficientFunds; 
    }

    public string MessageWaitingOtherPlayers 
    { 
        get => this.messageWaitingOtherPlayers; 
    }

    public string MessagePlayersFailedToLoad 
    { 
        get => this.messagePlayersFailedToLoad; 
    }

    public string MessageCannotUpgradeMaxLevel 
    { 
        get => this.messageCannotUpgradeMaxLevel; 
    }

    public string MessageCannotDowngradeMinLevel 
    { 
        get => this.messageCannotDowngradeMinLevel; 
    }

    public string MessageOnlyEvenBuildingAllowed 
    { 
        get => this.messageOnlyEvenBuildingAllowed; 
    }

    public string MessageCompleteMonopolyRequired 
    { 
        get => this.messageCompleteMonopolyRequired; 
    }

    #endregion

    #endregion

    public static UIManagerMonopolyGame Instance { get; private set; }

    public Action ButtonRollDiceClicked;

    public PanelInfoUI PanelInfo 
    { 
        get => PanelInfoUI.Instance; 
    }

    public PanelOfferUI PanelOffer 
    { 
        get => PanelOfferUI.Instance; 
    }

    public Canvas CanvasPlayersList 
    {
        get => this.canvasPlayersList;
    }

    public PanelPaymentUI PanelPayment 
    { 
        get => PanelPaymentUI.Instance; 
    }

    public PanelMonopolyNodeUI PanelMonopolyNode 
    { 
        get => PanelMonopolyNodeUI.Instance;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        GameCoordinator.Instance?.UpdateInitializedObjects(this.gameObject);
    }

    private void OnEnable()
    {
        this.buttonRollDice.onClick.AddListener(this.HandleButtonRollDiceClicked);
    }

    private void OnDisable()
    {
        this.buttonRollDice.onClick.RemoveListener(this.HandleButtonRollDiceClicked);
    }

    #region Roll Dice

    public void ShowButtonRollDice()
    {
        this.buttonRollDice.gameObject.SetActive(true);
    }

    private void HandleButtonRollDiceClicked()
    {
        this.buttonRollDice.gameObject.SetActive(false);

        this.ButtonRollDiceClicked?.Invoke();
    }

    #endregion

    #region Dice Animation

    public void ShowDiceAnimation()
    {
        Debug.Log(nameof(ShowDiceAnimation));

        //this.ShowDiceAnimationAsync();
        //this.ShowDiceAnimationClientRpc(GameManager.Instance.ClientParamsOtherClients);

        this.ShowDiceAnimationServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    [ServerRpc]
    private void ShowDiceAnimationServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log(nameof(ShowDiceAnimationServerRpc));

        this.ShowDiceAnimationClientRpc(GameManager.Instance.ClientParamsOtherClients);
    }

    private async void ShowDiceAnimationAsync()
    {
        Debug.Log(nameof(ShowDiceAnimationAsync));

        this.imageDiePlaceholder1.gameObject.SetActive(true);
        this.imageDiePlaceholder2.gameObject.SetActive(true);

        this.imageDiePlaceholder1.sprite = this.spriteDieFaces[GameManager.Instance.FirstDieValue - 1];
        this.imageDiePlaceholder2.sprite = this.spriteDieFaces[GameManager.Instance.SecondDieValue - 1];

        await Awaitable.WaitForSecondsAsync(this.diceScreenTime);

        this.imageDiePlaceholder1.gameObject.SetActive(false);
        this.imageDiePlaceholder2.gameObject.SetActive(false);
    }
    
    [ClientRpc]
    private void ShowDiceAnimationClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log(nameof(ShowDiceAnimationClientRpc));

        //this.ShowDiceAnimationAsync();
    }

    #endregion
}
