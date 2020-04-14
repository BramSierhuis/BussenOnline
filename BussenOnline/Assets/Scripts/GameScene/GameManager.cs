﻿using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
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
        {
            photonView.RPC("RPC_UpdateTurnUI", RpcTarget.All, players[0].Player);
            NextRound();
        }
        else //If there is another player that hasn't been this round
        {
            photonView.RPC("RPC_UpdateTurnUI", RpcTarget.All, players[activePlayerIndex + 1].Player);

            SetActivePlayerIndex(activePlayerIndex + 1);
            ActivePlayer = players[activePlayerIndex];
        }
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

            SetActivePlayerIndex(0);

            //Get the current round
            Enums.GameState currentRound = (Enums.GameState)(PhotonNetwork.CurrentRoom.CustomProperties["current round"]);
            currentRound += 1;

            //Increase the value of the round with 1. This will trigger OnRoomPropertiesUpdate
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("current round", currentRound);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
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

    private void SetActivePlayerIndex(int index)
    {
        ExitGames.Client.Photon.Hashtable activePlayerHash = new ExitGames.Client.Photon.Hashtable();
        activePlayerHash.Add("active player index", index);
        PhotonNetwork.CurrentRoom.SetCustomProperties(activePlayerHash);
    }

    private PlayingCard GiveCard(PlayerManager player)
    {
        PlayingCard cardToGive = playingCards[UnityEngine.Random.Range(0, playingCards.Count)];
        playingCards.Remove(cardToGive);

        cardToGive.photonView.RequestOwnership();
        cardToGive.AddToHand(player);
        cardToGive.hasOwner = true;

        return cardToGive;
    }

    private void CorrectAnswer()
    {

    }

    private void WrongAnswer(int drinks)
    {
        ActivePlayer.photonView.RequestOwnership();
        ActivePlayer.TotalDrinks+= drinks;
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

        photonView.RPC("RPC_UpdateTurnUI", RpcTarget.All, players[0].Player);
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
            Enum.TryParse(color, out Enums.CardColor cardColor);

            UpdateCardList();

            PlayingCard cardToGive = GiveCard(ActivePlayer);

            if (cardToGive.cardColor != cardColor)
                WrongAnswer(1);
            else
                CorrectAnswer();

            NextMove();
        }
    }

    public void OnClick_TakeHigherCard(bool higher)
    {
        ActivePlayer = players[activePlayerIndex];
        if(ActivePlayer.Player == PhotonNetwork.LocalPlayer)
        {
            PlayingCard cardToGive = GiveCard(ActivePlayer);

            if (cardToGive.value == activePlayer.hand[0].value)
                WrongAnswer(2);
            else
            {
                if (higher)
                {
                    if (cardToGive.value < activePlayer.hand[0].value)
                        WrongAnswer(1);
                    else
                        CorrectAnswer();
                }
                else
                {
                    if (cardToGive.value > activePlayer.hand[0].value)
                        WrongAnswer(1);
                    else
                        CorrectAnswer();
                }
            }

            NextMove();
        }
    }

    public void OnClick_TakeInsideCard(bool inside)
    {
        ActivePlayer = players[activePlayerIndex];
        if (ActivePlayer.Player == PhotonNetwork.LocalPlayer)
        {
            PlayingCard cardToGive = GiveCard(ActivePlayer);

            //Check if the card value is the same as one in the hand
            if (cardToGive.value == activePlayer.hand[0].value || cardToGive.value == activePlayer.hand[1].value)
                WrongAnswer(2);
            else
            {
                //Find the highest and lowest card in hand
                int min, max;
                if (activePlayer.hand[0].value > activePlayer.hand[1].value)
                {
                    min = activePlayer.hand[1].value;
                    max = activePlayer.hand[0].value;
                }
                else
                {
                    min = activePlayer.hand[0].value;
                    max = activePlayer.hand[1].value;
                }

                if (inside)
                {
                    if (!(cardToGive.value > min && cardToGive.value < max))
                        WrongAnswer(1);
                    else
                        CorrectAnswer();
                }
                else
                {
                    if (cardToGive.value > min && cardToGive.value < max)
                        WrongAnswer(1);
                    else
                        CorrectAnswer();
                }
            }

            NextMove();
        }
    }

    public void OnClick_TakeCardType(string type)
    {
        ActivePlayer = players[activePlayerIndex];
        if (ActivePlayer.Player == PhotonNetwork.LocalPlayer)
        {
            Enum.TryParse(type, out Enums.CardType cardType);

            bool disco = true;
            foreach (PlayingCard card in activePlayer.hand)
            {
                if (card.cardType == cardType)
                    disco = false;
            }

            //Has to be called after checking for disco because calling this removes a card from the list
            PlayingCard cardToGive = GiveCard(ActivePlayer);

            if (cardToGive.cardType != cardType)
                WrongAnswer(1);
            else
            {
                if (disco)
                {
                    activePlayer.photonView.RPC("RPC_AddDrink", RpcTarget.Others, 1);
                }
                else
                {
                    CorrectAnswer();
                }
            }

            NextMove();
        }
    }
    #endregion

    #region Photon Callbacks
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach(var key in propertiesThatChanged.Keys)
        {
            if(key.ToString() == "current round")
            {
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
                    case 2: //InsideOut
                        round2Panel.SetActive(false);
                        round3Panel.SetActive(true);
                        break;
                    case 3: //Disco
                        round3Panel.SetActive(false);
                        round4Panel.SetActive(true);
                        break;
                    case 4: //Winnerpanel
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
            if(key.ToString() == "active player index")
            {
                activePlayerIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["active player index"];
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