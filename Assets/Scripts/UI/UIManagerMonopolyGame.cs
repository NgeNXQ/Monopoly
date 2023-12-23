﻿using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal sealed class UIManagerMonopolyGame : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]

    #region Visuals

    #region Dice

    [Space]
    [Header("Button  \"Roll Dice\"")]
    [Space]

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
    [SerializeField] private char currency;

    public char Currency { get => this.currency; }

    #endregion

    #region Panel Players

    [Space]
    [Header("Panel \"Players\"")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasPanelPlayers;

    [Space]
    [SerializeField] private PanelPlayerGameUI panelPlayerGame;

    #endregion

    #endregion

    #region Messages

    [Space]
    [Header("Tex \"Messages\"")]
    [Space]

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

    public event IControlUI.ButtonClickedCallback ButtonRollDiceClicked;

    public PanelInfoUI PanelInfo 
    { 
        get => PanelInfoUI.Instance; 
    }

    public PanelOfferUI PanelOffer 
    { 
        get => PanelOfferUI.Instance; 
    }

    public PanelPaymentUI PanelPayment 
    { 
        get => PanelPaymentUI.Instance; 
    }

    public PanelMonopolyNodeUI PanelMonopolyNode 
    { 
        get => PanelMonopolyNodeUI.Instance;
    }

    #region Setup

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable()
    {
        this.buttonRollDice.onClick.AddListener(this.HandleButtonRollDiceClicked);
    }

    private void OnDisable()
    {
        this.buttonRollDice.onClick.RemoveListener(this.HandleButtonRollDiceClicked);
    }

    #endregion

    public void ShowButtonRollDice()
    {
        this.buttonRollDice.gameObject.SetActive(true);
    }

    public void HideButtonRollDice()
    {
        this.buttonRollDice.gameObject.SetActive(false);
    }

    private void HandleButtonRollDiceClicked()
    {
        this.ButtonRollDiceClicked?.Invoke();
    }

    public void AddPlayerToList(MonopolyPlayer player)
    {
        PanelPlayerGameUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerGame, this.canvasPanelPlayers.transform);
        newPanelPlayer.InitializePanel(player);

        newPanelPlayer.name = player.OwnerClientId.ToString();
    }

    public void RemovePlayerFromList()
    {
        //GameObject.Destroy(this.canvas.transform.GetChild(playerIndex).gameObject);
    }

    #region Dice Animation

    public void ShowDiceAnimation()
    {
        this.ShowDiceAnimationAsync();
        this.ShowDiceAnimationServerRpc();
    }

    private async void ShowDiceAnimationAsync()
    {
        this.imageDiePlaceholder1.gameObject.SetActive(true);
        this.imageDiePlaceholder2.gameObject.SetActive(true);

        this.imageDiePlaceholder1.sprite = this.spriteDieFaces[GameManager.Instance.FirstDieValue - 1];
        this.imageDiePlaceholder2.sprite = this.spriteDieFaces[GameManager.Instance.SecondDieValue - 1];

        await Awaitable.WaitForSecondsAsync(this.diceScreenTime);

        this.imageDiePlaceholder1.gameObject.SetActive(false);
        this.imageDiePlaceholder2.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowDiceAnimationServerRpc(ServerRpcParams serverRpcParams = default)
    {
        this.ShowDiceAnimationClientRpc(GameManager.Instance.ClientParamsOtherClients);
    }

    [ClientRpc]
    private void ShowDiceAnimationClientRpc(ClientRpcParams clientRpcParams)
    {
        this.ShowDiceAnimationAsync();
    }

    #endregion
}