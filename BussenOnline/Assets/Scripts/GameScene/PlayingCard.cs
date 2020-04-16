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
    public float cardsInHandOffset = 0.4f;
    public float cardsZOffset = 0.1f;
    public Transform stackPosition;
    public bool isDouble = false;

    private Sprite front = null;
    private SpriteRenderer sr;
    private bool showFront = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        stackPosition = GameObject.FindWithTag("StackPosition").transform;
    }

    public void AddToHand(PlayerManager player)
    {
        player.hand.Add(this);

        Vector3 toPosition = player.HandPosition.position;
        toPosition.x += cardsInHandOffset;
        toPosition.z -= cardsZOffset;

        Transform handPosition = player.HandPosition;
        handPosition.position = toPosition;

        StartCoroutine(MoveWithRotation(handPosition));
    }

    public void AddToPyramid(int position)
    {
        Transform pyramidPosition = GameManager.instance.pyramidSpawnPositions[position];

        StartCoroutine(MoveWithoutRotation(pyramidPosition));
    }

    public void MoveToStack()
    {
        StartCoroutine(MoveWithoutRotation(stackPosition));
    }

    public void Flip()
    {
        StartCoroutine(FlipCard());
    }

    public void MakeDouble()
    {
        isDouble = true;
        StartCoroutine(Rotate90());
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

    IEnumerator MoveWithoutRotation(Transform to)
    {
        float time = 0;
        float lerpValue;
        Transform from = transform; //From y rotation has to be 180

        float journeyLength = Vector3.Distance(from.position, to.position);

        while (transform.position != to.position)
        {
            time += Time.deltaTime;
            lerpValue = time / (1 / (speed / journeyLength));

            transform.position = Vector3.Lerp(from.position, to.position, Mathf.SmoothStep(0, 1, lerpValue));

            yield return null;
        }
    }

    IEnumerator FlipCard()
    {
        float time = 0;
        bool frontShown = false;
        Transform from = transform; //From y rotation has to be 180
        Transform to = transform;

        to.eulerAngles = new Vector3(from.eulerAngles.x, 0, from.eulerAngles.y);

        while (transform.eulerAngles.y > 0)
        {
            time += Time.deltaTime;

            if (transform.eulerAngles.y < 90 && !frontShown)
            {
                showFront = true;
                frontShown = true;
            }

            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, Mathf.SmoothStep(0, 1, .5f));

            yield return null;
        }
    }

    IEnumerator Rotate90()
    {
        float time = 0;
        Transform from = transform; //From x rotation has to be 0
        Transform to = transform;

        to.eulerAngles = new Vector3(from.eulerAngles.x, from.eulerAngles.y, 90);

        while (transform.eulerAngles.z < 90)
        {
            time += Time.deltaTime;

            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, Mathf.SmoothStep(0, 1, .5f));

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
            stream.SendNext(isDouble);
        }
        else if (stream.IsReading)
        {
            cardType = (Enums.CardType)stream.ReceiveNext();
            cardColor = (Enums.CardColor)stream.ReceiveNext();
            value = (int)stream.ReceiveNext();
            showFront = (bool)stream.ReceiveNext();
            isDouble = (bool)stream.ReceiveNext();
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
