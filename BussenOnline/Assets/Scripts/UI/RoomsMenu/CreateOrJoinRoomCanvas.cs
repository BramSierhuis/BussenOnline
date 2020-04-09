using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOrJoinRoomCanvas : MonoBehaviour
{
    [SerializeField]
    private RoomListingsMenu roomListingsMenu;
    [SerializeField]
    private CreateRoomMenu createRoomMenu;

    private RoomsCanvases roomsCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        roomsCanvases = canvases;
        createRoomMenu.FirstInitialize(canvases);
        roomListingsMenu.FirstInitialize(canvases);
    }
}
