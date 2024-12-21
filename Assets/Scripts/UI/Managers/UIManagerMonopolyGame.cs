using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal sealed class UIManagerMonopolyGame : NetworkBehaviour
{
    [Header("Currency")]

    [Space]
    [SerializeField]
    private char currency;

    [Space]
    [Header("Button Roll Dice")]

    [Space]
    [SerializeField]
    private Button buttonRollDice;

    [Space]
    [Header("Images \"Dices\"")]

    [Space]
    [SerializeField]
    private Image imageDiePlaceholder1;

    [Space]
    [SerializeField]
    private Image imageDiePlaceholder2;

    [Space]
    [SerializeField]
    private Sprite[] spriteDieFaces = new Sprite[6];

    [Space]
    [Header("Dice screen time")]

    [Space]
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float diceScreenTime = 1.0f;

    [Space]
    [Header("Button Disconnect")]

    [Space]
    [SerializeField]
    private Button buttonDisconnect;

    [Space]
    [Header("Panels Players List")]

    [Space]
    [SerializeField]
    private Canvas canvasPlayersList;

    [Space]
    [Header("Messages")]

    [Space]
    [SerializeField]
    private string messageWon;

    [Space]
    [SerializeField]
    private string messageBankrupt;

    [Space]
    [SerializeField]
    private string messageSentJail;

    [Space]
    [SerializeField]
    private string messageAlreadyBuilt;

    [Space]
    [SerializeField]
    private string messageDisconnecting;

    [Space]
    [SerializeField]
    private string messageTradeAccepted;

    [Space]
    [SerializeField]
    private string messageTradeDeclined;

    [Space]
    [SerializeField]
    private string messageHostDisconnected;

    [Space]
    [SerializeField]
    private string messageConfirmSurrender;

    [Space]
    [SerializeField]
    private string messageInsufficientFunds;

    [Space]
    [SerializeField]
    private string messageWaitingOtherPlayers;

    [Space]
    [SerializeField]
    private string messagePlayersFailedToLoad;

    [Space]
    [SerializeField]
    private string messageCannotUpgradeMaxLevel;

    [Space]
    [SerializeField]
    private string messageWrongTradeCredentials;

    [Space]
    [SerializeField]
    private string messageCannotDowngradeMinLevel;

    [Space]
    [SerializeField]
    private string messageOnlyEvenBuildingAllowed;

    [Space]
    [SerializeField]
    private string messageCannotUpgradeNotProperty;

    [Space]
    [SerializeField]
    private string messageCompleteMonopolyRequired;

    internal string MessageWon => this.messageWon;
    internal string MessageBankrupt => this.messageBankrupt;
    internal string MessageSentJail => this.messageSentJail;
    internal string MessageAlreadyBuilt => this.messageAlreadyBuilt;
    internal string MessageTradeAccepted => this.messageTradeAccepted;
    internal string MessageTradeDeclined => this.messageTradeDeclined;
    internal string MessageHostDisconnected => this.messageHostDisconnected;
    internal string MessageConfirmSurrender => this.messageConfirmSurrender;
    internal string MessageInsufficientFunds => this.messageInsufficientFunds;
    internal string MessageWaitingOtherPlayers => this.messageWaitingOtherPlayers;
    internal string MessagePlayersFailedToLoad => this.messagePlayersFailedToLoad;
    internal string MessageCannotUpgradeMaxLevel => this.messageCannotUpgradeMaxLevel;
    internal string MessageCannotDowngradeMinLevel => this.messageCannotDowngradeMinLevel;
    internal string MessageOnlyEvenBuildingAllowed => this.messageOnlyEvenBuildingAllowed;
    internal string MessageCannotUpgradeNotProperty => this.messageCannotUpgradeNotProperty;
    internal string MessageCompleteMonopolyRequired => this.messageCompleteMonopolyRequired;

    internal static UIManagerMonopolyGame Instance { get; private set; }

    internal Action ButtonRollDiceClicked;

    internal char Currency => this.currency;
    internal PanelInfoUI PanelInfo => PanelInfoUI.Instance;
    internal Canvas CanvasPlayersList => this.canvasPlayersList;
    internal PanelNodeMenuUI PanelNodeMenu => PanelNodeMenuUI.Instance;
    internal PanelNodeOfferUI PanelNodeOffer => PanelNodeOfferUI.Instance;
    internal PanelSendTradeUI PanelSendTrade => PanelSendTradeUI.Instance;
    internal PanelReceiveTradeUI PanelReceiveTrade => PanelReceiveTradeUI.Instance;
    internal PanelChancePaymentUI PanelPaymentChance => PanelChancePaymentUI.Instance;
    internal PanelNodePaymentUI PanelPaymentProperty => PanelNodePaymentUI.Instance;

    private void Awake()
    {
        if (UIManagerMonopolyGame.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        UIManagerMonopolyGame.Instance = this;
    }

    private void Start()
    {
        GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
    }

    private void OnEnable()
    {
        this.buttonRollDice.onClick.AddListener(this.OnButtonRollDiceClicked);
        this.buttonDisconnect.onClick.AddListener(this.OnButtonDisconnectClickedAsync);
    }

    private void OnDisable()
    {
        this.buttonRollDice.onClick.RemoveListener(this.OnButtonRollDiceClicked);
        this.buttonDisconnect.onClick.RemoveListener(this.OnButtonDisconnectClickedAsync);
    }

    internal void ShowPanelInfo(string descriptionText, Action callback = default)
    {
        this.PanelInfo.DescriptionText = descriptionText;
        this.PanelInfo.Show(callback);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void ShowPanelInfoServerRpc(string descriptionText, ServerRpcParams serverRpcParams)
    {
        this.ShowPanelInfoClientRpc(descriptionText, GameManager.Instance.TargetAllClientsExcludingCurrentPlayer);
    }

    [ClientRpc]
    private void ShowPanelInfoClientRpc(string descriptionText, ClientRpcParams clientRpcParams)
    {
        this.ShowPanelInfo(descriptionText);
    }

    internal void ShowPanelNodeOffer(MonopolyNode monopolyNode, Action callback)
    {
        this.PanelNodeOffer.PictureSprite = monopolyNode.NodeSprite;
        this.PanelNodeOffer.MonopolyTypeColor = monopolyNode.AffiliatedMonopoly.ColorOfSet;
        this.PanelNodeOffer.PriceText = $"{this.Currency} {monopolyNode.PricePurchase}";
        this.PanelNodeOffer.Show(callback);
    }

    internal void HidePanelNodeOffer()
    {
        this.PanelNodeOffer.Hide();
    }

    internal void ShowPanelSendTrade(PawnController sender, PawnController receiver, Action callback)
    {
        this.PanelSendTrade.Sender = sender;
        this.PanelSendTrade.Receiver = receiver;
        this.PanelSendTrade.Show(callback);
    }

    internal void HidePanelSendTrade()
    {
        this.PanelSendTrade.Hide();
    }

    internal void ShowPanelReceiveTrade(TradeCredentials credentials, Action callback)
    {
        this.PanelReceiveTrade.Credentials = credentials;
        this.PanelReceiveTrade.Show(callback);
    }

    internal void HidePanelReceiveTrade()
    {
        this.PanelReceiveTrade.Hide();
    }

    // internal void SendTradeOffer()
    // {
    //     TradeCredentials credentials = this.PanelTradeOffer.GetTradeCredentials();

    //     if (GameManager.Instance.GetPlayerById(credentials.ReceiverId) == null)
    //     {
    //         if (!GameManager.Instance.CurrentPawn.HasRolled)
    //         {
    //             this.ShowButtonRollDice();
    //         }
    //     }

    //     if ((credentials.SenderOffer == 0 && credentials.ReceiverOffer == 0) && (credentials.SenderNodeIndex == -1 && credentials.ReceiverNodeIndex == -1))
    //     {
    //         this.HideTradeOffer();

    //         if (!GameManager.Instance.CurrentPawn.HasRolled)
    //         {
    //             this.ShowButtonRollDice();
    //         }

    //         GameManager.Instance.CurrentPawn.IsTrading = false;

    //         UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageWrongTradeCredentials, PanelMessageBoxUI.Icon.Warning);
    //     }
    //     else if ((credentials.SenderOffer != 0 && credentials.ReceiverOffer != 0) && (credentials.SenderNodeIndex == -1 && credentials.ReceiverNodeIndex == -1))
    //     {
    //         this.HideTradeOffer();

    //         if (!GameManager.Instance.CurrentPawn.HasRolled)
    //         {
    //             this.ShowButtonRollDice();
    //         }

    //         GameManager.Instance.CurrentPawn.IsTrading = false;

    //         UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageWrongTradeCredentials, PanelMessageBoxUI.Icon.Warning);
    //     }
    //     else if ((credentials.SenderOffer != 0 || credentials.ReceiverOffer != 0) && (credentials.SenderNodeIndex == -1 && credentials.ReceiverNodeIndex == -1))
    //     {
    //         this.HideTradeOffer();

    //         if (!GameManager.Instance.CurrentPawn.HasRolled)
    //         {
    //             this.ShowButtonRollDice();
    //         }

    //         GameManager.Instance.CurrentPawn.IsTrading = false;

    //         UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageWrongTradeCredentials, PanelMessageBoxUI.Icon.Warning);
    //     }
    //     else
    //     {
    //         this.HideMonopolyNode();
    //         this.HideTradeOffer();

    //         this.SendTradeOfferServerRpc(credentials, GameManager.Instance.ServerParamsCurrentClient);
    //     }
    // }

    // internal void HideTradeOffer()
    // {
    //     this.PanelTradeOffer.Hide();
    // }

    // internal void ShowTradeOffer(string senderNicknameText, string receiverNicknameText, Action callback)
    // {
    //     this.PanelTradeOffer.SenderNicknameText = senderNicknameText;
    //     this.PanelTradeOffer.ReceiverNicknameText = receiverNicknameText;
    //     this.PanelTradeOffer.Show(callback);
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SendTradeOfferServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
    // {
    //     // if (GameManager.Instance.GetPlayerById(credentials.SenderId) == null)
    //     // {
    //     //     GameManager.Instance.SwitchPlayerForcefullyServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    //     // }

    //     // if (GameManager.Instance.GetPlayerById(credentials.ReceiverId) == null)
    //     // {
    //     //     this.ShowButtonRollDiceClientRpc(GameManager.Instance.ClientParamsCurrentClient);
    //     // }

    //     // this.RecieveTradeOfferClientRpc(credentials, GameManager.Instance.GetRedirectionRpc(credentials.ReceiverId));
    // }

    // internal void HideReceiveTrade()
    // {
    //     this.PanelReceiveTrade.Hide();
    // }

    // [ClientRpc]
    // private void RecieveTradeOfferClientRpc(TradeCredentials credentials, ClientRpcParams clientRpcParams)
    // {
    //     PawnController sender = GameManager.Instance.GetPawn(credentials.SenderId);
    //     PawnController receiver = GameManager.Instance.GetPawn(credentials.ReceiverId);

    //     if (sender == null)
    //     {
    //         return;
    //     }

    //     this.PanelReceiveTrade.Credentials = credentials;
    //     // this.PanelReceiveTrade.Show(GameManager.Instance.GetPlayerById(credentials.ReceiverId).CallbackReceiveTrade);
    // }

    internal void ShowPanelNodeMenu(MonopolyNode monopolyNode, Action callback)
    {
        this.PanelNodeMenu.PictureSprite = monopolyNode.NodeSprite;
        this.PanelNodeMenu.MonopolyColor = monopolyNode.AffiliatedMonopoly.ColorOfSet;

        if (PlayerPawnController.LocalInstance.SelectedNode.NodeType == MonopolyNode.Type.Property)
        {
            if (PlayerPawnController.LocalInstance.SelectedNode.Level == 0)
                this.PanelNodeMenu.PriceText = $"- {this.Currency} {monopolyNode.PricePurchase}";
            else if (PlayerPawnController.LocalInstance.SelectedNode.Level == 1)
                this.PanelNodeMenu.PriceText = $"- {this.Currency} {monopolyNode.PriceUpgrade}\n+ {this.Currency} {monopolyNode.PricePurchase}";
            else
                this.PanelNodeMenu.PriceText = $"+- {this.Currency} {monopolyNode.PriceUpgrade}";
        }
        else
        {
            if (PlayerPawnController.LocalInstance.SelectedNode.Level == 0)
                this.PanelNodeMenu.PriceText = $"- {this.Currency} {monopolyNode.PricePurchase}";
            else
                this.PanelNodeMenu.PriceText = $"+ {this.Currency} {monopolyNode.PricePurchase}";
        }

        this.PanelNodeMenu.Show(callback);
    }

    internal void HidePanelNodeMenu()
    {
        this.PanelNodeMenu.Hide();
    }

    internal void ShowPanelChancePayment(string descriptionText, Action callback)
    {
        this.PanelPaymentChance.DescriptionText = descriptionText;
        this.PanelPaymentChance.Show(callback);
    }

    internal void HidePanelChancePayment()
    {
        this.PanelPaymentChance.Hide();
    }

    internal void ShowPanelPropertyPayment(MonopolyNode monopolyNode, Action callback)
    {
        this.PanelPaymentProperty.PictureSprite = monopolyNode.NodeSprite;
        this.PanelPaymentProperty.MonopolyColor = monopolyNode.AffiliatedMonopoly.ColorOfSet;
        this.PanelPaymentProperty.PriceText = $"- {this.Currency} {monopolyNode.PriceRent}";

        this.PanelPaymentProperty.Show(callback);
    }

    internal void HidePanelNodePayment()
    {
        this.PanelPaymentProperty.Hide();
    }

    internal void ShowButtonRollDice()
    {
        this.buttonRollDice.gameObject.SetActive(true);
    }

    internal void HideButtonRollDice()
    {
        this.buttonRollDice.gameObject.SetActive(false);
    }

    private void OnButtonRollDiceClicked()
    {
        this.buttonRollDice.gameObject.SetActive(false);
        this.ButtonRollDiceClicked?.Invoke();
    }

    [ClientRpc]
    internal void ShowButtonRollDiceClientRpc(ClientRpcParams clientRpcParams)
    {
        this.buttonRollDice.gameObject.SetActive(true);
    }

    [ClientRpc]
    internal void HideButtonRollDiceClientRpc(ClientRpcParams clientRpcParams)
    {
        this.buttonRollDice.gameObject.SetActive(false);
    }

    internal void ShowButtonDisconnect()
    {
        this.buttonDisconnect.gameObject.SetActive(true);
    }

    private async void OnButtonDisconnectClickedAsync()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, this.messageDisconnecting, PanelMessageBoxUI.Icon.Loading);
        await LobbyManager.Instance.DisconnectFromLobbyAsync();
    }

    internal void ShowDiceAnimation()
    {
        this.ShowDiceAnimationAsync();
        this.ShowDiceAnimationServerRpc(GameManager.Instance.SenderLocalClient);
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
    private void ShowDiceAnimationServerRpc(ServerRpcParams serverRpcParams)
    {
        this.ShowDiceAnimationClientRpc(GameManager.Instance.TargetAllClientsExcludingCurrentPlayer);
    }

    [ClientRpc]
    private void ShowDiceAnimationClientRpc(ClientRpcParams clientRpcParams)
    {
        this.ShowDiceAnimationAsync();
    }
}
