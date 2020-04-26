using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Public Vars
    [Tooltip("The loglevel of Photon")]
    public PunLogLevel Loglevel = PunLogLevel.Informational;
    [Tooltip("The Input field to enter the nickname")]
    [SerializeField]
    private InputField nickNameInputField;
    #endregion

    #region Private Vars
    string gameVersion = "1";
    bool isConnecting;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Make sure the same scene is loaded
        PhotonNetwork.LogLevel = Loglevel;

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Public Methods
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected && nickNameInputField.text != "")
        {
            PhotonNetwork.NickName = nickNameInputField.text;
            PhotonNetwork.GameVersion = gameVersion;

            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("Nickname: " + nickNameInputField.text);
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

        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();

        PhotonNetwork.LoadLevel("RoomLobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Launcher: OnDisconnected() was called by PUN with error: " + cause);
    }
    #endregion
}
