using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayingCard : MonoBehaviour, IPunObservable
{
    public Enums.CardType cardType;
    public Enums.CardColor cardColor;
    public int value;
    public Sprite back;

    private Sprite front = null;
    private SpriteRenderer sr;
    private bool showFront = true;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowBack()
    {
        sr.sprite = back;
    }

    public void ShowFront()
    {
        sr.sprite = front;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (front == null)
            front = Array.Find(CardManager.instance.cardImages, x => x.name == "card_" + value + "_" + cardType.ToString());

        if (showFront && sr.sprite != front)
            sr.sprite = front;

        if (stream.IsWriting)
        {
            stream.SendNext(cardType);
            stream.SendNext(cardColor);
            stream.SendNext(value);
            stream.SendNext(showFront);
        }
        else if (stream.IsReading)
        {
            cardType = (Enums.CardType)stream.ReceiveNext();
            cardColor = (Enums.CardColor)stream.ReceiveNext();
            value = (int)stream.ReceiveNext();
            showFront = (bool)stream.ReceiveNext();
        }
    }
}
