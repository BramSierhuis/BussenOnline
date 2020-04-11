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
    #endregion

    #region Private Fields
    private List<PlayerListingElement> listingElements = new List<PlayerListingElement>();
    private RoomsCanvases roomsCanvases;
    private bool ready = false;
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
            listingElements[index].SetPlayerInfo(player);
        }
        else
        {
            PlayerListingElement element = Instantiate(playerListing, content);

            if (element != null)
            {
                element.SetPlayerInfo(player);
                listingElements.Add(element);
            }
        }
    }

    private void SetReadyUp(bool state)
    {
        ready = state;

        if (ready)
            readyUpText.text = "Ready!";
        else
            readyUpText.text = "Ready?";
    }
    #endregion

    #region Custom OnClicks
    public void OnClick_StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < listingElements.Count; i++)
            {
                if(listingElements[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!listingElements[i].ready)
                        return;
                }
            }

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
        }
    }
    #endregion

    #region Photon Callbacks
    public override void OnEnable()
    {
        base.OnEnable();

        SetReadyUp(false);

        GetCurrentRoomPlayers();
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
