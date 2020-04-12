using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
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
    int activePlayerInt;
    #endregion

    private void Awake()
    {
        instance = this;

        if(!PhotonNetwork.IsConnected)
            return;

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

                players.Add(playerManager);

                id++;
            }

            for(int i = id; i < spawnPositions.Length; i++)
            {
                Destroy(spawnPositions[i].gameObject);
            }
        }

        NextRound();
    }

    private void Start()
    {
    }


    public void NextMove()
    {
        if (activePlayerInt + 1 == players.Count)
        {
            NextRound();
        }
        else //If there is another player that hasn't been this round
        {
            activePlayerInt++;
            ActivePlayer = players[activePlayerInt];
        }

        statusText.text = "Speler: " + ActivePlayer.Player.NickName + " zijn beurd";
    }

    private void NextRound()
    {
        //In the first round there isn't an active player yet
        if(ActivePlayer == null)
        {
            int i = players.FindIndex(x => x.Player == PhotonNetwork.LocalPlayer);
            if (i != -1)
            {
                ActivePlayer = players[i]; //Set the active player to the local player
            }
            else
            {
                Debug.LogError("Not again");
            }

            statusText.text = "Speler: " + ActivePlayer.Player.NickName + " zijn beurd";
        }
        else //If this isn't the first round
        {
            ActivePlayer = players[0];
            activePlayerInt = 0;

            Enums.GameState currentRound = (Enums.GameState)(PhotonNetwork.CurrentRoom.CustomProperties["current round"]);
            PhotonNetwork.CurrentRoom.CustomProperties["current round"] = currentRound + 1;

            switch ((int)currentRound)
            {
                case 1: //RedBlack
                    round1Panel.SetActive(true);
                    break;
                case 2: //HigherLower
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

    public void OnClick_TakeColorCard(string color)
    {
        if (activePlayer.Player == PhotonNetwork.LocalPlayer)
        {
            Debug.Log(color);
            NextMove();
        }
    }
}
