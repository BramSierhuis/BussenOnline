using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Singeltons/MasterManager")]
public class MasterManager : ScriptableObjectSingleton<MasterManager>
{
    [SerializeField]
    private GameSettings gameSettings;
    public static GameSettings GameSettings { get { return Instance.gameSettings; } }
}
