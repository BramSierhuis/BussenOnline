using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalDrinksTextValue : MonoBehaviour, IPunObservable
{
    Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(text.text);
        } else if (stream.IsReading)
        {
            text.text = (string)stream.ReceiveNext();
        }
    }
}
