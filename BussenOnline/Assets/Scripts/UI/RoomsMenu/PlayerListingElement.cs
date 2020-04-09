using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helps with changing the values of a player listing element
/// </summary>
public class PlayerListingElement : MonoBehaviour
{
    #region Public Fields
    [Tooltip("The text of the RoomListing Element")]
    [SerializeField]
    private Text text;

    public Player Player { get; private set; }
    #endregion


    public void SetPlayerInfo(Player player)
    {
        Player = player;
        text.text = player.NickName;
    }
}
