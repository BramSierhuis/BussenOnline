using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helps with changing the values of a room listing element
/// </summary>
public class RoomListingElement : MonoBehaviour
{
    #region References
    [Tooltip("The text of the RoomListing Element")]
    [SerializeField]
    private Text text;
    #endregion

    public RoomInfo RoomInfo {get; private set;}

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        text.text = roomInfo.MaxPlayers + ", " + roomInfo.Name;
    }
}
