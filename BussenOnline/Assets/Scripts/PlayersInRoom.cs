using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Photon.Pun;
using Photon.Realtime;

public class PlayersInRoom : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields
    public GameObject roomPlayersText;
    public GameObject scrollViewContent;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //If we are the first player connecting OnPlayerEnteredRoom wont be called
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            AddToPlayerList(PhotonNetwork.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddToPlayerList(string nickName)
    {
        GameObject playerTextObj = PhotonNetwork.Instantiate(roomPlayersText.name, Vector3.zero, Quaternion.identity);
        Text playerText = playerTextObj.GetComponent<Text>();

        playerTextObj.name = nickName;
        playerText.text = nickName;

        playerTextObj.transform.SetParent(scrollViewContent.transform, false);
    }

    private void RemoveFromPlayerList(string nickName)
    {
        Destroy(scrollViewContent.transform.Find(nickName).gameObject);
    }

    #region Photon Callbacks
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        AddToPlayerList(other.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        RemoveFromPlayerList(other.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        } else if (stream.IsReading)
        {

        }
    }
    #endregion
}
