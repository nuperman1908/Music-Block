using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public Gamemodes gamemodes;
    public Speeds speed;
    public bool gravity;
    public int state;

    public void initiatePortal(Movement movement)
    {
        movement.ChangeThroughPortal(gamemodes, speed, gravity ? 1 : -1, state, transform.position.y);
    }
}
