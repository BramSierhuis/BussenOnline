using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    #endregion

    #region Private Fields
    private List<PlayerListingElement> listingElements = new List<PlayerListingElement>();
    private RoomsCanvases roomsCanvases;
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
    #endregion

    #region Photon Callbacks
    public override void OnEnable()
    {
        base.OnEnable();

        GetCurrentRoomPlayers();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        for (int i = 0; i < listingElements.Count; i++)
            Destroy(listingElements[i].gameObject);

        listingElements.Clear();
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
