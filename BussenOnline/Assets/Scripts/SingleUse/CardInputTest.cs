using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardInputTest : MonoBehaviourPun
{
    private Vector2 pos;

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                pos = transform.position;
                pos.x += 2;

                transform.position = pos;
            }
        }
    }
}
