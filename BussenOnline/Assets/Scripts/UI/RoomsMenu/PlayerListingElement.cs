using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;

/// <summary>
/// Helps with changing the values of a player listing element
/// </summary>
public class PlayerListingElement : MonoBehaviourPunCallbacks
{
    #region Public Fields
    [Tooltip("The text of the PlayerListing Element")]
    [SerializeField]
    private Text text;

    public Player Player { get; private set; }
    public bool ready = false;
    public RawImage background { get; private set; }
    #endregion

    #region Private Fields
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        background = GetComponent<RawImage>();
    }
    #endregion

    #region Custom Methods
    public void SetPlayerInfo(Player player)
    {
        Player = player;

        SetPlayerText(player);
    }

    private void SetPlayerText(Player player)
    {
        int result = -1;
        if (player.CustomProperties.ContainsKey("RandomNumber"))
            result = (int)player.CustomProperties["RandomNumber"];

        text.text = result.ToString() + ", " + player.NickName;
    }

    public void SetBackgroundColor(Color color)
    {
        background.color = color;
    }
    #endregion

    #region Photon Callbacks
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(targetPlayer != null && targetPlayer == Player)
        {
            if (changedProps.ContainsKey("RandomNumber"))
            {
                SetPlayerText(targetPlayer);
            }
        }
    }
    #endregion
}
