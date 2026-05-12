using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewLine : MonoBehaviour
{
    private float[] _speedValues = { 8.6f, 10.4f, 12.96f, 15.6f, 19.27f };
    public float speed = 10.4f;
    private void OnEnable()
    {
        transform.position = new Vector3(-.5f, 0, 0);
    }
    private void FixedUpdate()
    {
        transform.position += Vector3.right * speed * Time.fixedDeltaTime;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            PortalScript portal = collision.GetComponent<PortalScript>();
            if (!portal.isSpeedPortal) return;
            speed = _speedValues[(int)portal.speed];
        }
        if (collision.CompareTag("End"))
        {
            LevelEditorManager.Instance.StopMusicPreview();
        }
    }
}
