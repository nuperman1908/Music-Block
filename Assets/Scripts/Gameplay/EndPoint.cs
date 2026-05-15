using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    private void FixedUpdate()
    {
        if ( GameManager.Instance.player)
        {
            transform.position = new Vector3(transform.position.x,  GameManager.Instance.cameraFollow.transform.position.y, 0);
        }
    }
}
