using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields
    #endregion

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.InstantiateSceneObject("Card", new Vector3(0, 2, 0), Quaternion.identity);
    }
}
