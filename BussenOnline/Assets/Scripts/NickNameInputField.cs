using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(InputField))]
public class NickNameInputField : MonoBehaviour
{
    #region Const Variables
    const string playerNamePrefKey = "PlayerName"; //The name of the key for playername
    #endregion

    #region MonoBehaviour CallBacks
    void Start()
    {
        string defaultName = string.Empty;
        InputField inputField = GetComponent<InputField>();

        if (inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the player</param>
    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        PhotonNetwork.NickName = value;

        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
    #endregion
}
