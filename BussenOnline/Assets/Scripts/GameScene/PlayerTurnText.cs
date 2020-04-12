using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerTurnText : MonoBehaviour, IPunObservable
{
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(text.text);
        } 
        else if (stream.IsReading)
        {
            text.text = (string)stream.ReceiveNext();
        }
    }
}
