﻿using System.Collections;
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
    private RoomsCanvases roomsCanvases;
    #endregion

    #region First Initialization
    public void FirstInitialize(RoomsCanvases canvases)
    {
        roomsCanvases = canvases;
    }
    #endregion

    #region Photon Callbacks
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) //Room gets removed from list
            {
                int index = listingElements.FindIndex(x => x.RoomInfo.Name == roomInfo.Name); //Get the corresponding room in the local list

                if(index != -1) //-1 is returned when not found
                {
                    if(listingElements[index].gameObject != null)
                    {
                        Destroy(listingElements[index].gameObject);
                        listingElements.RemoveAt(index);
                    }
                }
            }
            else //Room gets added to list
            {
                int index = listingElements.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);
                if(index == -1)
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

    public override void OnJoinedRoom() 
    {
        roomsCanvases.CurrentRoomCanvas.Show();
        content.DestroyChildren();
        listingElements.Clear();
    }
    #endregion
}