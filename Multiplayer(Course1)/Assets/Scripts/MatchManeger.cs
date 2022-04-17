using System.Collections;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

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

    public void NewPlayerSend( )
    {

    }

    public void NewPlayerReceive(object[] dataRecive)
    {

    }

    public void ListPlayersSend()
    {

    }

    public void ListPlayersReceive(object[] dataRecive)
    {

    }

    public void UpdateStatSend()
    {

    }

    public void UpdateStatReceive(object[] dataRecive)
    {

    }

}
[System.Serializable]   //for seeing variabels in unity 
public class PlayerInfo  // new Class
{
    public string name;
    public int actor, kills, death; //values

    public PlayerInfo(string _name, int _actors, int _kills, int _death) //Constructor
    {
        name = _name;
        actor = _actors;
        kills = _kills;
        death = _death;
    }     
}
