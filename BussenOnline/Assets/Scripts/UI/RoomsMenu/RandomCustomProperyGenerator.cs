using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RandomCustomProperyGenerator : MonoBehaviour
{
    [Tooltip("The text element of the random number")]
    [SerializeField]
    private Text text;

    private ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private void SetCustomNumber()
    {
        System.Random rnd = new System.Random();
        int result = rnd.Next(0, 99);

        text.text = result.ToString();

        myCustomProperties["RandomNumber"] = result; //Float, double, string, int, bool, Vector2/3, Quaternion and player
        PhotonNetwork.SetPlayerCustomProperties(myCustomProperties);
    }

    public void OnClick_Button()
    {
        SetCustomNumber();
    }
}
