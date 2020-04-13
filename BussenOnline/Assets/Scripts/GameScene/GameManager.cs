using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields
    //Singleton
    public static GameManager instance;

    [SerializeField]
    private Transform[] spawnPositions;
    #endregion

    #region References
    public GameObject round1Panel;
    public GameObject round2Panel;
    public GameObject round3Panel;
    public GameObject round4Panel;
    public GameObject winnerPanel;
    public GameObject statusPanel;
    public Text statusText;
    public Text winnerText;
    #endregion

    #region Private Fields
    public List<PlayerManager> players = new List<PlayerManager>();

    PlayerManager activePlayer;
    PlayerManager ActivePlayer { 
        get { return activePlayer; }
        set 
        { 
            activePlayer = value;
            activePlayer.MyTurn = true;
        }
    }
    int activePlayerIndex;
    #endregion

    private void Awake()
    {
        instance = this;

        if(!PhotonNetwork.IsConnected)
            return;

        Debug.LogWarning(PhotonNetwork.LocalPlayer.UserId);

        if (PhotonNetwork.IsMasterClient)
        {
            int id = 0;
            foreach (KeyValuePair<int, Player> playerKVP in PhotonNetwork.CurrentRoom.Players)
            {
                GameObject uiGO = PhotonNetwork.InstantiateSceneObject("PlayerUI", spawnPositions[id].position, Quaternion.identity);
                PlayerUI playerUI = uiGO.GetComponent<PlayerUI>();
                PlayerManager playerManager = uiGO.GetComponent<PlayerManager>();

                playerUI.Player = playerKVP.Value;
                playerManager.Player = playerKVP.Value;
                playerManager.Index = id;

                players.Add(playerManager);

                id++;
            }

            for(int i = id; i < spawnPositions.Length; i++)
            {
                Destroy(spawnPositions[i].gameObject);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(ExecuteAfterTime(1));
    }


    public void NextMove()
    {
        if (activePlayerIndex + 1 == players.Count)
            NextRound();
        else //If there is another player that hasn't been this round
        {
            ActivePlayer.MyTurn = false;

            activePlayerIndex++;
            ActivePlayer = players[activePlayerIndex];
        }

        statusText.text = "Speler: " + ActivePlayer.Player.NickName + " zijn beurd";
    }

    private void NextRound()
    {
        //In the first round there isn't an active player yet
        if (ActivePlayer == null)
        {
                int i = players.FindIndex(x => x.Player == PhotonNetwork.MasterClient);
                if (i != -1)
                    ActivePlayer = players[i]; //Set the active player to the local player

                statusText.text = "Speler: " + ActivePlayer.Player.NickName + " zijn beurd";
        }
        else //If this isn't the first round
        {
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();

            ActivePlayer = players[0];
            activePlayerIndex = 0;

            Enums.GameState currentRound = (Enums.GameState)(PhotonNetwork.CurrentRoom.CustomProperties["current round"]);
            currentRound += 1;

            hash.Add("current round", currentRound);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    public void OnClick_TakeColorCard(string color)
    {
        ActivePlayer = players[activePlayerIndex];

        if (activePlayer.Player == PhotonNetwork.LocalPlayer)
        {
            Debug.Log(color);
            NextMove();
        }
        else
        {
            Debug.Log("Active player: " + activePlayer.Player.NickName);
            Debug.Log("Local player: " + PhotonNetwork.LocalPlayer.NickName);
        }
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        List<PlayerManager> tempPlayers = new List<PlayerManager>();

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            tempPlayers.Add(gameObject.GetComponent<PlayerManager>());
            players.Add(gameObject.GetComponent<PlayerManager>()); //is just temporary placeholder
        }

        foreach (PlayerManager player in tempPlayers)
        {
            players[player.Index] = player;
        }

        NextRound();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(activePlayerIndex);
        }else if (stream.IsReading)
        {
            activePlayerIndex = (int)stream.ReceiveNext();
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach(var key in propertiesThatChanged.Keys)
        {
            if(key.ToString() == "current round")
            {
                Debug.LogError("Round chaged: " + (int)PhotonNetwork.CurrentRoom.CustomProperties["current round"]);

                switch ((int)PhotonNetwork.CurrentRoom.CustomProperties["current round"])
                {
                    case 20: //Init
                        round1Panel.SetActive(false);
                        break;
                    case 0: //RedBlack
                        round1Panel.SetActive(true);
                        break;
                    case 1: //HigherLower
                        round1Panel.SetActive(false);
                        round2Panel.SetActive(true);
                        break;
                    case 3: //InsideOut
                        round2Panel.SetActive(false);
                        round3Panel.SetActive(true);
                        break;
                    case 4: //Disco
                        round3Panel.SetActive(false);
                        round4Panel.SetActive(true);
                        break;
                    case 5: //Pyramid
                        round4Panel.SetActive(false);
                        winnerPanel.SetActive(true);

                        int min = 9999;
                        PlayerManager winner = null;
                        foreach (PlayerManager player in players)
                        {
                            if (player.TotalDrinks < min)
                            {
                                min = player.TotalDrinks;
                                winner = player;
                            }
                        }

                        winnerText.text = "The winner is: " + winner.name;
                        break;
                }
            }
        }
    }
}
