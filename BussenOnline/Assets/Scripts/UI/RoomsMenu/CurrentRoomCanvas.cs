using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentRoomCanvas : MonoBehaviour
{
    [SerializeField]
    private LeaveRoomMenu leaveRoomMenu;
    [SerializeField]
    private PlayerListingsMenu playereListingsMenu;
    public LeaveRoomMenu LeaveRoomMenu { get { return leaveRoomMenu; } }

    private RoomsCanvases roomsCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        roomsCanvases = canvases;
        playereListingsMenu.FirstInitialize(canvases);
        leaveRoomMenu.FirstInitialize(canvases);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
