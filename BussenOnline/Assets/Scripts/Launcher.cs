using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Public Vars
    [Tooltip("The loglevel of Photon")]
    public PunLogLevel Loglevel = PunLogLevel.Informational;
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte MaxPlayersPerRoom = 4;
    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;
    #endregion

    #region Private Vars
    string gameVersion = "1";
    bool isConnecting;
    #endregion

    #region MonoBehaviour Callbacks
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Make sure the same scene is loaded
        PhotonNetwork.LogLevel = Loglevel;
    }

    void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    void Update()
    {

    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        Debug.Log("The start game button has been pressed");

        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    #endregion

    #region Photon.PunBehaviour CallBacks
    /// <summary>
    /// Gets called upon connecting to the master server
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");

        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Launcher: OnDisconnected() was called by PUN with error: " + cause);

        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        string roomName = null;
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = MaxPlayersPerRoom };
        TypedLobby typedLobby = null;

        PhotonNetwork.CreateRoom(roomName, roomOptions, typedLobby);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room with nickname: " + PhotonNetwork.NickName);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("We load the GameScene");

            PhotonNetwork.LoadLevel("GameScene");
        }
    }
    #endregion
}
