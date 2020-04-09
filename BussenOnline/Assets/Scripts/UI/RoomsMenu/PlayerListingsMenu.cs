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
    #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerListingElement element = Instantiate(playerListing, content);

        if (element != null)
        {
            element.SetPlayerInfo(newPlayer);
            listingElements.Add(element);
        }
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
}
