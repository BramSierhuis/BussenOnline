using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields
    [SerializeField]
    private Text roundText;
    #endregion

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.InstantiateSceneObject("Card", new Vector3(0, 2, 0), Quaternion.identity);
    }

    public void NextRound()
    {
        Enums.GameState round = (Enums.GameState)(PhotonNetwork.CurrentRoom.CustomProperties["current round"]);
        PhotonNetwork.CurrentRoom.CustomProperties["current round"] = round + 1;

        roundText.text = (round + 1).ToString();
    }
}
