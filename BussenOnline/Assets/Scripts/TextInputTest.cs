using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TextInputTest : MonoBehaviourPunCallbacks, IPunObservable
{
    private Vector2 pos;
    private Text text;

    void Awake() 
    {
        text = gameObject.GetComponent<Text>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("SerializeView");
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(text.text);
        }
        else if (stream.IsReading)
        {
            transform.position = (Vector3)stream.ReceiveNext();
            text.text = (string)stream.ReceiveNext();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            pos = transform.position;
            pos.x += 2;

            gameObject.GetComponent<Text>().text = "Test";

            transform.position = pos;
        }
    }


}
