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
    [Tooltip("The positions that the player UI can spawn")]
    private Transform[] spawnPositions;

    public List<PlayingCard> playingCards;
    #endregion

    #region References
    [SerializeField]
    private GameObject round1Panel;
    [SerializeField]
    private GameObject round2Panel;
    [SerializeField]
    private GameObject round3Panel;
    [SerializeField]
    private GameObject round4Panel;
    [SerializeField]
    private GameObject winnerPanel;
    [SerializeField]
    private Text statusText;
    [SerializeField]
    private Text winnerText;
    #endregion

    #region Private Fields
    private List<PlayerManager> players = new List<PlayerManager>();
    private int activePlayerIndex;
    private PlayerManager activePlayer;

    private PlayerManager ActivePlayer { 
        get { return activePlayer; }
        set 
        { 
            activePlayer = value;
            activePlayer.MyTurn = true;
        }
    }
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        instance = this; //Singleton assignment

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
                playerManager.Index = id;

                players.Add(playerManager);

                id++;
            }

            CardManager.instance.GenerateCards();
            photonView.RPC("RPC_PopulateCardsList", RpcTarget.Others);

        }

        foreach (Transform spawnPoint in spawnPositions)
        {
            Destroy(spawnPoint.gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(CreateLocalPlayerList(1));
    }
    #endregion

    #region Custom Methods
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

        photonView.RPC("RPC_UpdateTurnUI", RpcTarget.All, ActivePlayer.Player = players[activePlayerIndex].Player);
    }

    private void NextRound()
    {
        //In the first round there isn't an active player yet
        if (ActivePlayer == null)
        {
                int i = players.FindIndex(x => x.Player == PhotonNetwork.MasterClient);
                if (i != -1)
                    ActivePlayer = players[i]; //Set the active player to the master client
        }
        else //If this isn't the first round
        {
            //Set the active player to the first player
            ActivePlayer = players[0];
            activePlayerIndex = 0;

            //Get the current round
            Enums.GameState currentRound = (Enums.GameState)(PhotonNetwork.CurrentRoom.CustomProperties["current round"]);
            currentRound += 1;

            //Increase the value of the round with 1. This will trigger OnRoomPropertiesUpdate
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("current round", currentRound);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }

        //Update the text of whose turn it is
        photonView.RPC("RPC_UpdateTurnUI", RpcTarget.All, ActivePlayer.Player = players[activePlayerIndex].Player);
    }

    private void UpdateCardList()
    {
        foreach(GameObject cardGO in GameObject.FindGameObjectsWithTag("PlayingCard"))
        {
            PlayingCard playingCard = cardGO.GetComponent<PlayingCard>();

            if (playingCard.hasOwner)
                playingCards.Remove(playingCard);
        }
    }

    IEnumerator CreateLocalPlayerList(float time)
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
    #endregion

    #region OnClicks
    public void OnClick_TakeColorCard(string color)
    {
        //This properties also sets muTurn to true
        ActivePlayer = players[activePlayerIndex];

        if (ActivePlayer.Player == PhotonNetwork.LocalPlayer)
        {
            UpdateCardList();
            
            PlayingCard cardToGive = playingCards[UnityEngine.Random.Range(0, playingCards.Count)];
            playingCards.Remove(cardToGive);

            cardToGive.photonView.RequestOwnership();
            cardToGive.AddToHand(ActivePlayer);

            NextMove();
        }
        else
        {
            Debug.Log("Active player: " + ActivePlayer.Player.NickName);
            Debug.Log("Local player: " + PhotonNetwork.LocalPlayer.NickName);
        }
    }
    #endregion

    #region Photon Callbacks
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
    #endregion

    #region RPC's
    [PunRPC]
    private void RPC_UpdateTurnUI(Player nextActivePlayer)
    {
        if (PhotonNetwork.LocalPlayer == nextActivePlayer)
            statusText.text = "Jouw beurd!";
        else
            statusText.text = "Speler: " + nextActivePlayer.NickName + " zijn beurd";
    }    
    
    [PunRPC]
    private void RPC_PopulateCardsList()
    {
        Debug.Log("PopulateCardsList() RPC Callded");

        foreach (GameObject cardGO in GameObject.FindGameObjectsWithTag("PlayingCard"))
        {
            playingCards.Add(cardGO.GetComponent<PlayingCard>());
        }
    }
    #endregion
}