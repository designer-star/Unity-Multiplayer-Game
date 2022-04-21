﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManeger : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManeger instance;



    private void Awake()
    {
        instance = this;
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;


    public List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>();

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }


    // Start is called before the first frame update
    void Start()
    {
        
        if (!PhotonNetwork.IsConnected)
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowLeaderboard();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            UIController.instance.leaderboard.SetActive(false);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            //Debug.Log("Received event " + theEvent);

            switch (theEvent)
            {
                case EventCodes.NewPlayer:

                    NewPlayerReceive(data);

                    break;

                case EventCodes.ListPlayers:

                    ListPlayersReceive(data);

                    break;

                case EventCodes.UpdateStat:

                    UpdateStatReceive(data);

                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(                   //For Sending Event
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] dataRecive)
    {
        PlayerInfo player = new PlayerInfo((string)dataRecive[0], (int)dataRecive[1], (int)dataRecive[2], (int)dataRecive[3]);

        allPlayers.Add(player);

        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count];

        for(int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i] = piece;
        }

        PhotonNetwork.RaiseEvent(                   //For Sending Event
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ListPlayersReceive(object[] dataRecive)
    {
        allPlayers.Clear();

        for (int i = 0; i < dataRecive.Length; i++)
        {
            object[] piece = (object[])dataRecive[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );

            allPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
    }

    public void UpdateStatSend(int actorSending, int statToChange, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToChange, amountToChange };

        PhotonNetwork.RaiseEvent(                   //For Sending Event
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdateStatReceive(object[] dataRecive)  // for all players
    {
        int actor = (int)dataRecive[0];
        int statType = (int)dataRecive[1];
        int amount = (int)dataRecive[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if(allPlayers[i].actor == actor)
            {
                switch(statType)  //Ctr + 2r change all dependences
                {
                    case 0: //kills
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : kills" + allPlayers[i].kills);
                        break;
                    case 1: //death
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : deaths" + allPlayers[i].deaths);
                        break;
                }

                if(i == index)
                {
                    UpdateStatDisplay();
                }

                if (UIController.instance.leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }
    }



    public void UpdateStatDisplay()
    {
        if (allPlayers.Count > index)
        {
            UIController.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
        else
        {
            UIController.instance.killsText.text = "Kills: 0";
            UIController.instance.deathsText.text = "Deaths: 0";
        }
    }


    void ShowLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);
        foreach(LeaderboardPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();
        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach(PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderboardPlayerDisplay, UIController.instance.leaderboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);
            newPlayerDisplay.gameObject.SetActive(true);

            lboardPlayers.Add(newPlayerDisplay);
        }
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while(sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach(PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if(player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }

                    sorted.Add(player);
                }
            }
        }


        return sorted;
    }
}
[System.Serializable]   //for seeing variabels in unity 
public class PlayerInfo  // new Class
{
    public string name;
    public int actor, kills, deaths; //values    actor - specific number in the network

    public PlayerInfo(string _name, int _actors, int _kills, int _death) //Constructor
    {
        name = _name;
        actor = _actors;
        kills = _kills;
        deaths = _death;
    }     
}
