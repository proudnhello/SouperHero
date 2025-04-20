using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private RoomType _blockType;
    public bool connected = false;

    public bool west;
    public bool east;
    public bool south;
    public bool north;

    // ONLY INTERMEDIATES SHOULD HAVE DOORS. EVERYTHING ELSE (CONNECTORS, START/END) SHOULD BE DOOR-LESS
    public GameObject northDoor;
    public GameObject southDoor;
    public GameObject eastDoor;
    public GameObject westDoor;

    public GameObject northDoorOpen;
    public GameObject southDoorOpen;
    public GameObject eastDoorOpen;
    public GameObject westDoorOpen;

    public RoomType BlockType()
    {
        return _blockType;
    }

    public bool CompareType(RoomType roomType)
    {
        return (roomType & _blockType) != 0;
    }

    public void SetDirections(bool north, bool south, bool east, bool west)
    {
        this.north = north;
        this.south = south;
        this.east = east;
        this.west = west;
    }

    // Directions needed to be opened from the block's point of view
    public void SetDoors(bool north, bool south, bool east, bool west)
    {
        if (westDoor) westDoor.SetActive(!west);
        if (westDoorOpen) westDoorOpen.SetActive(west);
        if (eastDoor) eastDoor.SetActive(!east);
        if (eastDoorOpen) eastDoorOpen.SetActive(east);
        if (northDoor) northDoor.SetActive(!north);
        if (northDoorOpen) northDoorOpen.SetActive(north);
        if (southDoor) southDoor.SetActive(!south);
        if (southDoorOpen) southDoorOpen.SetActive(south);
    }

    public void SetDoors(GenSequence s)
    {
        SetDoors(s.north, s.south, s.east, s.west);
    }
}
