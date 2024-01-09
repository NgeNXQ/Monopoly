using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal sealed class UIManagerMonopolyGame : NetworkBehaviour
{
    #region Setup

    #region Currency

    [Header("Currency")]

    [Space]
    [SerializeField] private char currency;

    #endregion

    #region Dice Setup

    [Space]
    [Header("Button Roll Dice")]

    [Space]
    [SerializeField] private Button buttonRollDice;

    [Space]
    [Header("Images \"Dices\"")]

    [Space]
    [SerializeField] private Image imageDiePlaceholder1;

    [Space]
    [SerializeField] private Image imageDiePlaceholder2;

    [Space]
    [SerializeField] private Sprite[] spriteDieFaces = new Sprite[6];

    [Space]
    [Header("Dice screen time")]

    [Space]
    [SerializeField][Range(0.0f, 10.0f)] private float diceScreenTime = 1.0f;

    #endregion

    #region Button Disconnect

    [Space]
    [Header("Button Disconnect")]

    [Space]
    [SerializeField] private Button buttonDisconnect;

    #endregion

    #region Panel Players List

    [Space]
    [Header("Panels Players List")]

    [Space]
    [SerializeField] private Canvas canvasPlayersList;

    #endregion
    
    #region Messages

    [Space]
    [Header("Messages")]

    [Space]
    [SerializeField] private string messageWon;

    [Space]
    [SerializeField] private string messageSentJail;

    [Space]
    [SerializeField] private string messageAlreadyBuilt;

    [Space]
    [SerializeField] private string messageDisconnecting;

    [Space]
    [SerializeField] private string messageTradeAccepted;

    [Space]
    [SerializeField] private string messageTradeDeclined;

    [Space]
    [SerializeField] private string messageConfirmSurrender;

    [Space]
    [SerializeField] private string messageInsufficientFunds;

    [Space]
    [SerializeField] private string messageWaitingOtherPlayers;

    [Space]
    [SerializeField] private string messagePlayersFailedToLoad;

    [Space]
    [SerializeField] private string messageCannotUpgradeMaxLevel;

    [Space]
    [SerializeField] private string messageWrongTradeCredentials;

    [Space]
    [SerializeField] private string messageCannotDowngradeMinLevel;

    [Space]
    [SerializeField] private string messageOnlyEvenBuildingAllowed;

    [Space]
    [SerializeField] private string messageCannotUpgradeNotProperty;

    [Space]
    [SerializeField] private string messageCompleteMonopolyRequired;

    #endregion

    #endregion

    #region Messages

    public string MessageWon 
    {
        get => this.messageWon;
    }

    public string MessageSentJail 
    {
        get => this.messageSentJail;
    }

    public string MessageAlreadyBuilt 
    {
        get => this.messageAlreadyBuilt;
    }

    public string MessageTradeAccepted 
    {
        get => this.messageTradeAccepted;
    }

    public string MessageTradeDeclined 
    {
        get => this.messageTradeDeclined;
    }

    public string MessageConfirmSurrender 
    {
        get => this.messageConfirmSurrender;
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

    public string MessageCannotUpgradeNotProperty 
    {
        get => this.messageCannotUpgradeNotProperty;
    }

    public string MessageCompleteMonopolyRequired 
    {
        get => this.messageCompleteMonopolyRequired;
    }

    #endregion

    public static UIManagerMonopolyGame Instance { get; private set; }

    public char Currency { get => this.currency; }

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

    public PanelTradeOfferUI PanelTradeOffer 
    {
        get => PanelTradeOfferUI.Instance;
    }

    public PanelMonopolyNodeUI PanelMonopolyNode 
    {
        get => PanelMonopolyNodeUI.Instance;
    }

    public PanelReceiveTradeUI PanelReceiveTrade 
    {
        get => PanelReceiveTradeUI.Instance;
    }

    public PanelPaymentChanceUI PanelPaymentChance 
    {
        get => PanelPaymentChanceUI.Instance;
    }

    public PanelPaymentPropertyUI PanelPaymentProperty 
    {
        get => PanelPaymentPropertyUI.Instance;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
    }

    private void OnEnable()
    {
        this.buttonRollDice.onClick.AddListener(this.HandleButtonRollDiceClicked);
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonDisconnectClickedAsync);
    }

    private void OnDisable()
    {
        this.buttonRollDice.onClick.RemoveListener(this.HandleButtonRollDiceClicked);
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonDisconnectClickedAsync);
    }

    #region Panels 

    #region Panel Info

    public void ShowInfo(string descriptionText, Action callback = default)
    {
        this.PanelInfo.DescriptionText = descriptionText;

        this.PanelInfo.Show(callback);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowInfoServerRpc(string descriptionText, ServerRpcParams serverRpcParams)
    {
        this.ShowInfoClientRpc(descriptionText, GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void ShowInfoClientRpc(string descriptionText, ClientRpcParams clientRpcParams)
    {
        this.ShowInfo(descriptionText);
    }

    #endregion

    #region Panel Offer

    public void ShowOffer(Sprite pictureSprite, Color monopolyColor, int price, Action callback)
    {
        this.PanelOffer.PictureSprite = pictureSprite;
        this.PanelOffer.MonopolyTypeColor = monopolyColor;
        this.PanelOffer.PriceText = $"{this.Currency} {price}";

        this.PanelOffer.Show(callback);
    }

    public void HideOffer()
    {
        this.PanelOffer.Hide();
    }

    #endregion

    #region Panel Trade Offer

    public void SendTradeOffer()
    {
        ulong otherClientId = GameManager.Instance.CurrentPlayer.PlayerTradingWith.OwnerClientId;

        MonopolyPlayer otherPlayer = GameManager.Instance.GetPlayerById(otherClientId);
        MonopolyPlayer thisPlayer = GameManager.Instance.GetPlayerById(GameManager.Instance.CurrentPlayer.OwnerClientId);

        if (otherPlayer == null || thisPlayer == null)
        {
            this.ShowButtonRollDice();

            this.HideTradeOffer();

            GameManager.Instance.CurrentPlayer.IsTrading = false;

            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageWrongTradeCredentials, PanelMessageBoxUI.Icon.Warning);

            return;
        }

        int thisBalance = this.PanelTradeOffer.ThisOffer;
        int otherBalance = this.PanelTradeOffer.OtherOffer;

        int thisNodeIndex = this.PanelTradeOffer.ThisNodeIndex;
        int otherNodeIndex = this.PanelTradeOffer.OtherNodeIndex;

        if (thisBalance < 0)
        {
            thisBalance = 0;
        }

        if (otherBalance < 0)
        {
            otherBalance = 0;
        }

        if (thisBalance > thisPlayer.Balance.Value)
        {
            thisBalance = thisPlayer.Balance.Value;
        }

        if (otherBalance > otherPlayer.Balance.Value)
        {
            otherBalance = otherPlayer.Balance.Value;
        }

        if ((thisBalance == 0 && otherBalance == 0) && (thisNodeIndex == -1 && otherNodeIndex == -1))
        {
            this.HideTradeOffer();

            this.ShowButtonRollDice();

            GameManager.Instance.CurrentPlayer.IsTrading = false;

            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageWrongTradeCredentials, PanelMessageBoxUI.Icon.Warning);
        }
        else if ((thisBalance != 0 && otherBalance != 0) && (thisNodeIndex == -1 && otherNodeIndex == -1))
        {
            this.HideTradeOffer();

            this.ShowButtonRollDice();

            GameManager.Instance.CurrentPlayer.IsTrading = false;

            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageWrongTradeCredentials, PanelMessageBoxUI.Icon.Warning);
        }
        else
        {
            this.SendTradeOfferServerRpc(thisBalance, otherBalance, thisNodeIndex, otherNodeIndex, otherClientId, GameManager.Instance.ServerParamsCurrentClient);
        }
    }

    public void ShowTradeOffer(string thisNickname, string otherNickname, Action callback)
    {
        this.PanelTradeOffer.ThisNicknameText = thisNickname;
        this.PanelTradeOffer.OtherThisNicknameText = otherNickname;

        this.PanelTradeOffer.Show(callback);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendTradeOfferServerRpc(int thisBalance, int otherBalance, int thisNodeIndex, int otherNodeIndex, ulong otherClientId, ServerRpcParams serverRpcParams)
    {
        if (GameManager.Instance.GetPlayerById(serverRpcParams.Receive.SenderClientId) == null)
        {
            GameManager.Instance.SwitchPlayerForcefullyServerRpc(GameManager.Instance.ServerParamsCurrentClient);
        }

        if (GameManager.Instance.GetPlayerById(otherClientId) == null)
        {
            this.ShowButtonRollDiceClientRpc(GameManager.Instance.ClientParamsCurrentClient);
        }

        this.RecieveTradeOfferClientRpc(thisBalance, otherBalance, thisNodeIndex, otherNodeIndex, serverRpcParams.Receive.SenderClientId, otherClientId, GameManager.Instance.GetRedirectionRpc(otherClientId));
    }

    [ClientRpc]
    private void RecieveTradeOfferClientRpc(int thisBalance, int otherBalance, int thisNodeIndex, int otherNodeIndex, ulong senderClientId, ulong receiverClientId, ClientRpcParams clientRpcParams) 
    {
        MonopolyPlayer otherPlayer = GameManager.Instance.GetPlayerById(senderClientId);
        MonopolyPlayer thisPlayer = GameManager.Instance.GetPlayerById(receiverClientId);

        if (thisPlayer == null || otherPlayer == null)
        {
            return;
        }

        this.PanelReceiveTrade.ThisNicknameText = thisPlayer.Nickname;
        this.PanelReceiveTrade.OtherThisNicknameText = otherPlayer.Nickname;

        this.PanelReceiveTrade.ThisOffer = thisBalance;
        this.PanelReceiveTrade.OtherOffer = otherBalance;

        if (thisNodeIndex != -1)
        {
            this.PanelReceiveTrade.ThisNodeIndex = thisNodeIndex;
            this.PanelReceiveTrade.ThisSprite = MonopolyBoard.Instance[thisNodeIndex].NodeSprite;
        }

        if (otherNodeIndex != -1)
        {
            this.PanelReceiveTrade.OtherNodeIndex = otherNodeIndex;
            this.PanelReceiveTrade.OtherSprite = MonopolyBoard.Instance[otherNodeIndex].NodeSprite;
        }

        this.PanelReceiveTrade.Show(GameManager.Instance.GetPlayerById(receiverClientId).CallbackReceiveTrade);
    }

    public void HideTradeOffer()
    {
        this.PanelTradeOffer.Hide();
    }

    #endregion

    #region Panel Receive Trade

    public void HideReceiveTrade()
    {
        this.PanelReceiveTrade.Hide();
    }

    #endregion

    #region Panel Monopoly Node

    public void ShowMonopolyNode(Sprite pictureSprite, Color monopolyColor, Action callback)
    {
        this.PanelMonopolyNode.PictureSprite = pictureSprite;
        this.PanelMonopolyNode.MonopolyColor = monopolyColor;

        MonopolyNode selectedNode = GameManager.Instance.CurrentPlayer.SelectedNode;

        switch (GameManager.Instance.CurrentPlayer.SelectedNode.NodeType)
        {
            case MonopolyNode.Type.Property:
                {
                    if (GameManager.Instance.CurrentPlayer.SelectedNode.LocalLevel == 0)
                    {
                        this.PanelMonopolyNode.PriceText = $"- {this.Currency} {selectedNode.PricePurchase}";
                    }
                    else if (GameManager.Instance.CurrentPlayer.SelectedNode.LocalLevel == 1)
                    {
                        this.PanelMonopolyNode.PriceText = $"- {this.Currency} {selectedNode.PriceUpgrade}\n+ {this.Currency} {selectedNode.PricePurchase}";
                    }
                    else
                    {
                        this.PanelMonopolyNode.PriceText = $"+- {this.Currency} {selectedNode.PriceUpgrade}";
                    }
                }
                break;
            default:
                {
                    if (GameManager.Instance.CurrentPlayer.SelectedNode.LocalLevel == 0)
                    {
                        this.PanelMonopolyNode.PriceText = $"- {this.Currency} {selectedNode.PricePurchase}";
                    }
                    else
                    {
                        this.PanelMonopolyNode.PriceText = $"+ {this.Currency} {selectedNode.PricePurchase}";
                    }
                }
                break;
        }

        this.PanelMonopolyNode.Show(callback);
    }

    public void HideMonopolyNode()
    {
        this.PanelMonopolyNode.Hide();
    }

    #endregion

    #region Panel Payment Chance

    public void ShowPaymentChance(string descriptionText, Action callback)
    {
        this.PanelPaymentChance.DescriptionText = descriptionText;

        this.PanelPaymentChance.Show(callback);
    }

    public void HidePaymentChance()
    {
        this.PanelPaymentChance.Hide();
    }

    #endregion

    #region Panel Payment Property

    public void ShowPaymentProperty(Sprite pictureSprite, Color monopolyColor, int price, Action callback)
    {
        this.PanelPaymentProperty.PictureSprite = pictureSprite;
        this.PanelPaymentProperty.MonopolyColor = monopolyColor;
        this.PanelPaymentProperty.PriceText = $"- {this.Currency} {price}";

        this.PanelPaymentProperty.Show(callback);
    }

    public void HidePaymentProperty()
    {
        this.PanelPaymentProperty.Hide();
    }

    #endregion

    #endregion

    #region Button Roll Dice

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
        if (NetworkManager.Singleton.LocalClientId == GameManager.Instance.CurrentPlayer.OwnerClientId)
        {
            this.buttonRollDice.gameObject.SetActive(false);

            this.ButtonRollDiceClicked?.Invoke();
        }
    }

    [ClientRpc]
    public void ShowButtonRollDiceClientRpc(ClientRpcParams clientRpcParams)
    {
        this.buttonRollDice.gameObject.SetActive(true);
    }

    [ClientRpc]
    public void HideButtonRollDiceClientRpc(ClientRpcParams clientRpcParams)
    {
        this.buttonRollDice.gameObject.SetActive(false);
    }

    #endregion

    #region Button Disconnect

    public void ShowButtonDisconnect()
    {
        this.buttonDisconnect.gameObject.SetActive(true);
    }

    private async void HandleButtonDisconnectClickedAsync()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, this.messageDisconnecting, PanelMessageBoxUI.Icon.Loading);

        await LobbyManager.Instance.DisconnectFromLobbyAsync();
    }

    #endregion

    #region Dice Animation & Syncing

    public void ShowDiceAnimation()
    {
        this.ShowDiceAnimationAsync();
        this.ShowDiceAnimationServerRpc(GameManager.Instance.ServerParamsCurrentClient);
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
        this.ShowDiceAnimationClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void ShowDiceAnimationClientRpc(ClientRpcParams clientRpcParams)
    {
        this.ShowDiceAnimationAsync();
    }

    #endregion
}
