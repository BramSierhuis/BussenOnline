using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text roomName;

    private RoomsCanvases roomsCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        roomsCanvases = canvases;
    }

    public void OnClick_CreateRoom()
    {
        if (!PhotonNetwork.IsConnected) 
        {
            Debug.LogError("CreateRoom.OnClick_CreateRoom(): You can't create a room when not connected");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.MaxPlayers = 6;

        if(roomName.text != string.Empty)
            PhotonNetwork.JoinOrCreateRoom(roomName.text, options, null);
    }

    #region Pun Callbacks
    public override void OnCreatedRoom()
    {
        Debug.Log("Created room succesfully.", this);

        roomsCanvases.CurrentRoomCanvas.Show();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed " + message, this);
    }
    #endregion
}
