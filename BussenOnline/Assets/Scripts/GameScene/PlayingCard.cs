using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;

public class PlayingCard : MonoBehaviourPun, IPunObservable, IPunOwnershipCallbacks
{
    public Enums.CardType cardType;
    public Enums.CardColor cardColor;
    public int value;
    public Sprite back;
    public float speed = 5f;
    public bool hasOwner;

    private Sprite front = null;
    private SpriteRenderer sr;
    private bool showFront = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowBack()
    {
        showFront = false;
    }

    public void ShowFront()
    {
        showFront = true;
    }

    public void AddToHand(PlayerManager player)
    {
        player.hand.Add(this);

        StartCoroutine(MoveWithRotation(player.HandPosition));
    }

    IEnumerator MoveWithRotation(Transform to)
    {
        float time = 0;
        float lerpValue;
        bool frontShown = false;
        Transform from = transform; //From y rotation has to be 180

        from.eulerAngles = -transform.eulerAngles;

        float journeyLength = Vector3.Distance(from.position, to.position);

        while (transform.position != to.position)
        {
            time += Time.deltaTime;
            lerpValue = time / (1 / (speed / journeyLength));

            if (transform.eulerAngles.y < 90 && !frontShown)
            {
                showFront = true;
                frontShown = true;
            }

            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, Mathf.SmoothStep(0, 1, lerpValue));
            transform.position = Vector3.Lerp(from.position, to.position, Mathf.SmoothStep(0, 1, lerpValue));

            yield return null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (front == null)
            front = Array.Find(CardManager.instance.cardImages, x => x.name == "card_" + value + "_" + cardType.ToString());

        if (showFront && sr.sprite != front)
            sr.sprite = front;
        else if (!showFront && sr.sprite != back)
            sr.sprite = back;

        if (stream.IsWriting)
        {
            stream.SendNext(cardType);
            stream.SendNext(cardColor);
            stream.SendNext(value);
            stream.SendNext(showFront);
            stream.SendNext(hasOwner);
        }
        else if (stream.IsReading)
        {
            cardType = (Enums.CardType)stream.ReceiveNext();
            cardColor = (Enums.CardColor)stream.ReceiveNext();
            value = (int)stream.ReceiveNext();
            showFront = (bool)stream.ReceiveNext();
            hasOwner = (bool)stream.ReceiveNext();
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        photonView.TransferOwnership(requestingPlayer);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        //Needed for interface
    }
}
