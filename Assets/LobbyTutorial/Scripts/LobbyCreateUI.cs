//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class LobbyCreateUI : MonoBehaviour {


//    public static LobbyCreateUI Instance { get; private set; }


//    [SerializeField] private Button createButton;
//    [SerializeField] private Button lobbyNameButton;
//    [SerializeField] private Button publicPrivateButton;
//    [SerializeField] private Button maxPlayersButton;
//    [SerializeField] private Button gameModeButton;
//    [SerializeField] private TextMeshProUGUI lobbyNameText;
//    [SerializeField] private TextMeshProUGUI publicPrivateText;
//    [SerializeField] private TextMeshProUGUI maxPlayersText;
//    [SerializeField] private TextMeshProUGUI gameModeText;


//    private string lobbyName;
//    private bool isPrivate;
//    private int maxPlayers;
//    private LobbyManager.GameMode gameMode;

//    private void Awake() {
//        Instance = this;

//        createButton.onClick.AddListener(() => {
//            LobbyManager.Instance.CreateLobby(
//                lobbyName,
//                maxPlayers,
//                isPrivate,
//                gameMode
//            );
//            Hide();
//        });

//        lobbyNameButton.onClick.AddListener(() => {
//            UI_InputWindow.Show_Static("Lobby Name", lobbyName, "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ .,-", 20,
//            () => {
//                // Cancel
//            },
//            (string lobbyName) => {
//                this.lobbyName = lobbyName;
//                UpdateText();
//            });
//        });

//        publicPrivateButton.onClick.AddListener(() => {
//            isPrivate = !isPrivate;
//            UpdateText();
//        });

//        maxPlayersButton.onClick.AddListener(() => {
//            UI_InputWindow.Show_Static("Max Players", maxPlayers,
//            () => {
//                // Cancel
//            },
//            (int maxPlayers) => {
//                this.maxPlayers = maxPlayers;
//                UpdateText();
//            });
//        });

//        gameModeButton.onClick.AddListener(() => {
//            switch (gameMode) {
//                default:
//                case LobbyManager.GameMode.CaptureTheFlag:
//                    gameMode = LobbyManager.GameMode.Conquest;
//                    break;
//                case LobbyManager.GameMode.Conquest:
//                    gameMode = LobbyManager.GameMode.CaptureTheFlag;
//                    break;
//            }
//            UpdateText();
//        });

//        Hide();
//    }

//    private void UpdateText() {
//        lobbyNameText.text = lobbyName;
//        publicPrivateText.text = isPrivate ? "Private" : "Public";
//        maxPlayersText.text = maxPlayers.ToString();
//        gameModeText.text = gameMode.ToString();
//    }

//    private void Hide() {
//        gameObject.SetActive(false);
//    }

//    public void Show() {
//        gameObject.SetActive(true);

//        lobbyName = "MyLobby";
//        isPrivate = false;
//        maxPlayers = 4;
//        gameMode = LobbyManager.GameMode.CaptureTheFlag;

//        UpdateText();
//    }

//}