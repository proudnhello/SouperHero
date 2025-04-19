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

    public bool compareType(RoomType roomType)
    {
        return (roomType & _blockType) != 0;
    }

    public void setDirections(bool north, bool south, bool east, bool west)
    {
        this.north = north;
        this.south = south;
        this.east = east;
        this.west = west;
    }

    public void setDoors(bool north, bool south, bool east, bool west)
    {
        if(westDoor) westDoor.SetActive(!east);
        if (westDoorOpen) westDoorOpen.SetActive(east);
        if (eastDoor) eastDoor.SetActive(!west);
        if (eastDoorOpen) eastDoorOpen.SetActive(west);
        if (northDoor) northDoor.SetActive(!south);
        if (northDoorOpen) northDoorOpen.SetActive(south);
        if (southDoor) southDoor.SetActive(!north);
        if (southDoorOpen) southDoorOpen.SetActive(north);
    }
}
