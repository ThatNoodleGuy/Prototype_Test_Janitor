using System;
using System.Collections.Generic;
using UnityEngine;

public class PBLevelLayout : MonoBehaviour
{
    public enum Dir { North, South, East, West } // North=+Z, South=-Z, East=+X, West=-X

    [Serializable]
    public class Room
    {
        public string id = Guid.NewGuid().ToString();
        public string name = "Room";
        public Vector3 center = Vector3.zero;
        public Vector3 size = new Vector3(10f, 3f, 10f);

        [Header("Windows")]
        public bool windowsNorth;
        public bool windowsSouth;
        public bool windowsEast;
        public bool windowsWest;
    }

    [Serializable]
    public class Connection
    {
        public string aRoomId;
        public string bRoomId;
        public Dir aDoorDir; // which wall of A has the door
        public Dir bDoorDir; // which wall of B has the door
    }

    [Header("Layout Data")]
    public List<Room> rooms = new List<Room>();
    public List<Connection> connections = new List<Connection>();

    public Room GetRoom(string id) => rooms.Find(r => r.id == id);

    public bool HasConnection(string roomA, string roomB)
    {
        foreach (var c in connections)
        {
            if ((c.aRoomId == roomA && c.bRoomId == roomB) ||
                (c.aRoomId == roomB && c.bRoomId == roomA))
                return true;
        }
        return false;
    }
}
