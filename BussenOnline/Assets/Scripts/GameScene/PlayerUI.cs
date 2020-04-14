using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerUI : MonoBehaviourPun, IPunObservable
{
    [SerializeField]
    private Vector3 canvasOffset;

    [SerializeField]
    private Player player;
    public Player Player { 
        get { return player; } 
        set
        {
            player = value;
            nicknameText.text = value.NickName;
        }
    }

    public Transform handPosition;

    [SerializeField]
    private Text nicknameText;

    [SerializeField]
    private Canvas canvas;

    private void Start()
    {
        if (gameObject.transform.position.x > 0) //If we are on the right side
        {
            canvas.transform.position += canvasOffset;
            handPosition.position += canvasOffset;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(Player);
        else if (stream.IsReading)
            Player = (Player) stream.ReceiveNext();
    }
}
