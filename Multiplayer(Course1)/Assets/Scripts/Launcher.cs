using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    public GameObject loadingScreen;

    public GameObject menuButtons;

    public TMP_Text loadingText;


    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;



    public GameObject roomScreen;
    public TMP_Text roomNameText, playerNameLabel;
    private List<TMP_Text> allPlayersName = new List<TMP_Text>();


    public GameObject errorScreen;
    public TMP_Text errorText;


    public GameObject roomBrowserScreen;
    public RoomButton theRoomButton;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();


    private void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        CloseMenu();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Network...";


        PhotonNetwork.ConnectUsingSettings();
    }

    void CloseMenu()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        loadingText.text = "Joing Lobyy...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenu();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
    }

    public void OpenRoomCreate()
    {
        CloseMenu();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;

            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            CloseMenu();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);
        }
        
    }
    public override void OnJoinedRoom()
    {
        CloseMenu();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in allPlayersName)
        {
            Destroy(player.gameObject);
        }
        allPlayersName.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayersName.Add(newPlayerLabel);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create Room: " + message;
        CloseMenu();
        errorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenu();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

        CloseMenu();
        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }


    public override void OnLeftRoom()
    {
        CloseMenu();
        menuButtons.SetActive(true);
    }

    public void openRoomBrowser()
    {
        CloseMenu();
        roomBrowserScreen.SetActive(true);
    }

    public void closeRoomBrowser()
    {
        CloseMenu();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();
        theRoomButton.gameObject.SetActive(false);

        for(int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenu();
        loadingText.text = "Join Room";
        loadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
