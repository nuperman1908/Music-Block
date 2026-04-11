using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public Gamemodes gamemodes;
    public Speeds speed;
    public bool gravity;
    public int state;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        try
        {
            Movement movement = collision.gameObject.GetComponent<Movement>();

            movement.ChangeThroughPortal(gamemodes, speed, gravity ? 1 : -1, state);
        }
        catch
        {
            return;
        }
    }
}
