using UnityEngine;
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

    [Tooltip("The spawnpositions of the card pyramid")]
    public Transform[] pyramidSpawnPositions;

    [SerializeField]
    [Tooltip("The chance of making a card in the pyramid be a double in percent")]
    private float doubleChance = 10f;

    [SerializeField]
    private float pyramidGap = 0.4f;
    [SerializeField]
    private float pyramidLevels = 5f; //Max is 5
    private float TotalPyramidCards { //The total cards of the pyramid, calculated based on the levels
        get
        {
            int total = 0;

            for (int i = 1; i <= pyramidLevels; i++)
            {
                total += i;
            }

            return total;
        }
    }

    public List<PlayingCard> cardsInStack;
    public List<PlayingCard> cardsInhand;
    public List<PlayingCard> cardsInPyramid;
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
    private GameObject masterPyramidLoadPanel;
    [SerializeField]
    private GameObject clientPyramidLoadPanel;
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

    #region Private Methods
    private void NextMove()
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

    private void SetActivePlayerIndex(int index)
    {
        ExitGames.Client.Photon.Hashtable activePlayerHash = new ExitGames.Client.Photon.Hashtable();
        activePlayerHash.Add("active player index", index);
        PhotonNetwork.CurrentRoom.SetCustomProperties(activePlayerHash);
    }

    private PlayingCard GiveCard(PlayerManager player)
    {
        PlayingCard cardToGive = cardsInStack[UnityEngine.Random.Range(0, cardsInStack.Count)];
        photonView.RPC("RPC_AddCardToHandList", RpcTarget.All, cardToGive.photonView.ViewID);

        cardToGive.photonView.RequestOwnership();
        cardToGive.AddToHand(player);

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
    #endregion

    #region ShowRoundUI
    private void ShowRedBlack()
    {
        round1Panel.SetActive(true);
    }

    private void ShowHigherLower()
    {
        round1Panel.SetActive(false);
        round2Panel.SetActive(true);
    }

    private void ShowInsideOut()
    {
        round2Panel.SetActive(false);
        round3Panel.SetActive(true);
    }

    private void ShowPickCardType()
    {
        round3Panel.SetActive(false);
        round4Panel.SetActive(true);
    }

    private void ShowPyramid()
    {
        round4Panel.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            masterPyramidLoadPanel.SetActive(true);
        }
        else
        {
            clientPyramidLoadPanel.SetActive(true);
        }
    }

    private void CreatePyramid()
    {
        for (int i = 0; i < TotalPyramidCards; i++)
        {
            PlayingCard cardToMove = cardsInStack[UnityEngine.Random.Range(0, cardsInStack.Count)];
            photonView.RPC("RPC_AddCardToPyramidList", RpcTarget.All, cardToMove.photonView.ViewID);

            cardToMove.photonView.RequestOwnership();
            cardToMove.AddToPyramid(i);

            if (UnityEngine.Random.Range(0, 100) <= doubleChance)
                cardToMove.MakeDouble();
        }

        foreach (PlayingCard card in cardsInStack)
        {
            card.photonView.RequestOwnership();
            card.MoveToStack();       
        }
    }
    #endregion

    #region Coroutines
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
                    //Make all players drink
                    foreach (PlayerManager player in players)
                    {
                        if (player != activePlayer)
                        {
                            player.photonView.RequestOwnership();
                            player.TotalDrinks += 4;
                        }
                    }
                }
                else
                {
                    CorrectAnswer();
                }
            }

            NextMove();
        }
    }

    public void OnClick_StartPyramid()
    {
        CreatePyramid();

        clientPyramidLoadPanel.SetActive(false);
        masterPyramidLoadPanel.SetActive(false);

       // foreach (PlayingCard card in TotalPyramidCards;)
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
                        ShowRedBlack();
                        break;
                    case 1: //HigherLower
                        ShowHigherLower();
                        break;
                    case 2: //InsideOut
                        ShowInsideOut();
                        break;
                    case 3: //Disco
                        ShowPickCardType();
                        break;
                    case 4: //Pyramid
                        ShowPyramid();
                            break;
                    case 99: //Winnerpanel
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
        foreach (GameObject cardGO in GameObject.FindGameObjectsWithTag("PlayingCard"))
        {
            cardsInStack.Add(cardGO.GetComponent<PlayingCard>());
        }
    }

    [PunRPC]
    private void RPC_AddCardToPyramidList(int id)
    {
        PlayingCard cardToAddToPyramid = null;

        foreach (PlayingCard card in cardsInStack)
        {

            if (card.photonView.ViewID == id)
            {
                cardToAddToPyramid = card;
                break;
            }
        }

        cardsInStack.Remove(cardToAddToPyramid);
        cardsInPyramid.Add(cardToAddToPyramid);
    }

    [PunRPC]
    private void RPC_AddCardToHandList(int id)
    {
        PlayingCard cardToAddTohand = null;

        foreach (PlayingCard card in cardsInStack)
        {
            if (card.photonView.ViewID == id)
            {
                cardToAddTohand = card;
                break;
            }
        }

        cardsInStack.Remove(cardToAddTohand);
        cardsInhand.Add(cardToAddTohand);
    }
    #endregion
}