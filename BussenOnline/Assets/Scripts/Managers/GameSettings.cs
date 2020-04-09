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
}
