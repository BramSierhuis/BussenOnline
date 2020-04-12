using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields
    [SerializeField]
    private Transform[] spawnPositions;

    [SerializeField]
    private GameObject playerUIPrefab;
    #endregion

    #region Private Fields
    List<PlayerUI> playerUIs = new List<PlayerUI>();
    #endregion

    private void Awake()
    {
        if(!PhotonNetwork.IsConnected)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            int id = 0;
            foreach (KeyValuePair<int, Player> playerKVP in PhotonNetwork.CurrentRoom.Players)
            {
                GameObject uiGO = PhotonNetwork.InstantiateSceneObject("PlayerUI", spawnPositions[id].position, Quaternion.identity);
                Debug.Log("Instantiated + " + uiGO.name);
                PlayerUI playerUI = uiGO.GetComponent<PlayerUI>();

                playerUI.Player = playerKVP.Value;

                id++;
            }
        }
    }

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

        //roundText.text = (round + 1).ToString();
    }
}
