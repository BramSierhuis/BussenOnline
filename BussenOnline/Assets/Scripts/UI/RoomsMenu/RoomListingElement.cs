using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// Helps with changing the values of a room listing element
/// </summary>
public class RoomListingElement : MonoBehaviour
{
    #region Public Fields
    [Tooltip("The text of the RoomListing Element")]
    [SerializeField]
    private Text text;
    public RoomInfo RoomInfo { get; private set; }
    #endregion

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        text.text = roomInfo.MaxPlayers + ", " + roomInfo.Name;
    }

    public void OnClick_Button()
    {
        PhotonNetwork.JoinRoom(RoomInfo.Name);
    }
}
