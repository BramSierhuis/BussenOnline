using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    #region Public Fields
    [Tooltip("The content of the Players ScrollView object")]
    [SerializeField]
    private Transform content;

    [Tooltip("Player list element prefab")]
    [SerializeField]
    private PlayerListingElement playerListing;

    [Tooltip("The reference for the ready up button")]
    [SerializeField]
    private Text readyUpText;

    [Tooltip("The reference for the start game button")]
    [SerializeField]
    private GameObject startButton;

    [Tooltip("The reference for the ready button")]
    [SerializeField]
    private GameObject readyButton;
    #endregion

    #region Private Fields
    private List<PlayerListingElement> listingElements = new List<PlayerListingElement>();
    private RoomsCanvases roomsCanvases;
    private bool ready = false;
    private bool IsEveryoneReady
    {
        get
        {
            for (int i = 0; i < listingElements.Count; i++)
            {
                if (listingElements[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!listingElements[i].ready)
                        return false;
                }
            }

            return true;
        }
    }
    #endregion

    #region First Initialize
    public  void FirstInitialize(RoomsCanvases canvases)
    {
        roomsCanvases = canvases;
    }
    #endregion

    #region CustomMethods
    private void GetCurrentRoomPlayers()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
            return;

        foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value);
        }
    }

    private void AddPlayerListing(Player player)
    {
        int index = listingElements.FindIndex(x => x.Player == player);
        if(index != -1)
        {
            listingElements[index].SetPlayerInfo(player); //Player already exists, so just update info
        }
        else
        {
            PlayerListingElement element = Instantiate(playerListing, content);

            if (element != null)
            {
                element.SetPlayerInfo(player);
                listingElements.Add(element);
            }

            if (player.IsMasterClient)
            {
                element.background.color = Color.green;
                element.ready = true;
            }
        }
    }

    private void SetReadyUp(bool state)
    {
        int index = listingElements.FindIndex(x => x.Player == PhotonNetwork.LocalPlayer);
        ready = state;

        if (ready)
        {
            readyUpText.text = "Ready!";
            readyButton.GetComponent<RawImage>().color = Color.green;

            if (index != -1)
            {
                listingElements[index].background.color = Color.green;
            }
        }
        else
        {
            readyUpText.text = "Ready?";
            readyButton.GetComponent<RawImage>().color = Color.red;

            if (index != -1)
            {
                listingElements[index].background.color = Color.red;
            }
        }
    }
    #endregion

    #region Custom OnClicks
    public void OnClick_StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!IsEveryoneReady)
                return;

            PhotonNetwork.CurrentRoom.IsOpen = false; //Not everyone can join
            PhotonNetwork.CurrentRoom.IsVisible = false; //Not everyone can see it in the room list
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void OnClick_ReadyUp()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            SetReadyUp(!ready);

            photonView.RPC("RPC_ChangeReadyState", RpcTarget.MasterClient,PhotonNetwork.LocalPlayer, ready);
        }
    }
    #endregion

    #region RPC's
    [PunRPC]
    private void RPC_ChangeReadyState(Player player, bool ready)
    {
        int index = listingElements.FindIndex(x => x.Player == player); //Get the corresponding player in the local list

        if (index != -1) //-1 is returned when not found
        {
            listingElements[index].ready = ready;

            if (ready)
                listingElements[index].background.color = Color.green;
            else
                listingElements[index].background.color = Color.red;

            if (IsEveryoneReady)
                startButton.GetComponent<RawImage>().color = Color.green;
            else
                startButton.GetComponent<RawImage>().color = Color.red;
        }
    }
    #endregion

    #region Photon Callbacks
    public override void OnEnable()
    {
        base.OnEnable();

        SetReadyUp(false);

        GetCurrentRoomPlayers();

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.SetActive(true);
            readyButton.SetActive(false);

            startButton.GetComponent<RawImage>().color = Color.red;
        }
        else
        {
            startButton.SetActive(false);
            readyButton.SetActive(true);

            readyButton.GetComponent<RawImage>().color = Color.red;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        for (int i = 0; i < listingElements.Count; i++)
            Destroy(listingElements[i].gameObject);

        listingElements.Clear();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomsCanvases.CurrentRoomCanvas.LeaveRoomMenu.OnClick_LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = listingElements.FindIndex(x => x.Player == otherPlayer); //Get the corresponding player in the local list

        if (index != -1) //-1 is returned when not found
        {
            Destroy(listingElements[index].gameObject);
            listingElements.RemoveAt(index);
        }
    }
    #endregion
}
