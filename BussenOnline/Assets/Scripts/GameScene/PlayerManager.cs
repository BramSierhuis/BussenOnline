using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour, IPunObservable
{
    public Player Player;
    public bool MyTurn = false;
    public int Index;

    [SerializeField]
    private int totalDrinks = 0;
    public int TotalDrinks
    {
        get { return totalDrinks; }
        set
        {
            totalDrinks = value;
            totalDrinksText.text = value.ToString();
        }
    }

    [SerializeField]
    private Text totalDrinksText;

    private void Awake()
    {
        totalDrinksText.text = TotalDrinks.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Player);
            stream.SendNext(MyTurn);
            stream.SendNext(TotalDrinks);
            stream.SendNext(Index);
        }
        else if (stream.IsReading) 
        {
            Player = (Player)stream.ReceiveNext();
            MyTurn = (bool)stream.ReceiveNext();
            TotalDrinks = (int)stream.ReceiveNext();
            Index = (int)stream.ReceiveNext();
        }
    }
}
