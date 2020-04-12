using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/GameSettings")]
public class GameSettings : ScriptableObject
{
    [SerializeField]
    private string gameVersion = "0.0.1";
    public string GameVersion { get { return gameVersion; } }

    [SerializeField]
    private string nickName = "Bram";
    public string NickName
    {
        get
        {
            int value = Random.Range(0, 9999);
            return nickName + value.ToString();
        }
    }

    [SerializeField]
    private int minimumPlayers = 1;
    public int MinimumPlayers { get { return minimumPlayers; } }

    [SerializeField]
    private byte maximumPlayers = 6;
    public byte MaximumPlayers { get { return maximumPlayers; } }

    [SerializeField]
    private string[] roomProperties = { "current round" };
    public string[] RoomProperties { get { return roomProperties; } }

    [SerializeField]
    private ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "current round", Enums.GameState.PlayerListingMenu } };
    public ExitGames.Client.Photon.Hashtable CustomRoomProperties { get { return customRoomProperties; } }
}
