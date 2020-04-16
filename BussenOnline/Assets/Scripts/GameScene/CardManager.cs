using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    public Vector3 cardStackOffset;

    public GameObject cardTemplate;
    public Sprite[] cardImages;

    private void Awake()
    {
        instance = this;
    }

    public void GenerateCards()
    {
        if (cardTemplate != null)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 14; j++)
                {
                    GameObject card = PhotonNetwork.Instantiate(cardTemplate.name, cardTemplate.transform.position, cardTemplate.transform.rotation, 0);
                    card.name = j + " of " + ((Enums.CardType)i).ToString();
                    PlayingCard playingCard = card.GetComponent<PlayingCard>();

                    GameManager.instance.cardsInStack.Add(playingCard);

                    playingCard.value = j;
                    playingCard.cardType = (Enums.CardType)i;

                    if (i < 2) //The card is red
                        playingCard.cardColor = Enums.CardColor.Red;
                    else //The card is black
                        playingCard.cardColor = Enums.CardColor.Black;
                }
            }
        }
    }
}
