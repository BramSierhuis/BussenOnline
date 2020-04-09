using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    #region Public Fields
    [Tooltip("The content of the Rooms ScrollView object")]
    [SerializeField]
    private Transform content;

    [Tooltip("Room list element prefab")]
    [SerializeField]
    private RoomListingElement roomListing;
    #endregion

    #region Private Fields
    private List<RoomListingElement> listingElements = new List<RoomListingElement>();
    #endregion

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) //Room gets removed from list
            {
                int index = listingElements.FindIndex(x => x.RoomInfo.Name == roomInfo.Name); //Get the corresponding room in the local list

                if(index != -1) //-1 is returned when not found
                {
                    Destroy(listingElements[index].gameObject);
                    listingElements.RemoveAt(index);
                }
            }
            else //Room gets added to list
            {
                RoomListingElement element = Instantiate(roomListing, content);

                if (element != null)
                {
                    element.SetRoomInfo(roomInfo);
                    listingElements.Add(element);
                }
            }
        }
    }
}