
using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Transform background;

    public float offsetX = -3f;
    public float fixedZ = -10f;
    public float screenOffsetY = -2 ;

    [Header("Y Follow")]
    public float lerpSpeedY = 4f;
    // Dead zone: camera chỉ chạy khi player ra ngoài vùng này (tính theo world unit)
    public float deadZoneY = 1.5f;

    [Header("Background Parallax")]
    [Range(0f, 1f)] public float parallaxX = 0.3f;
    [Range(0f, 1f)] public float parallaxY = 0.5f;

    private float targetY;
    private Vector3 _lastCamPos;

    void Start()
    {
        targetY = transform.position.y;
        _lastCamPos = transform.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float newX = player.position.x + offsetX;

        // Vị trí Y camera "lý tưởng" để player nằm ở nửa dưới
        float desiredY = player.position.y - screenOffsetY;

        // Dead zone: chỉ lerp khi lệch quá ngưỡng
        float diffY = desiredY - transform.position.y;
        float newY;
        if (Mathf.Abs(diffY) > deadZoneY)
            newY = Mathf.Lerp(transform.position.y, desiredY, lerpSpeedY * Time.deltaTime);
        else
            newY = transform.position.y;

        transform.position = new Vector3(newX, newY, fixedZ);

        if (background != null)
        {
            Vector3 delta = transform.position - _lastCamPos;
            background.position += new Vector3(delta.x * parallaxX, delta.y * parallaxY, 0f);
        }
        _lastCamPos = transform.position;
    }

    public void SnapToPlayer()
    {
        if (player == null) return;
        float x = player.position.x + offsetX;
        float y = player.position.y - screenOffsetY;
        transform.position = new Vector3(x, y, fixedZ);
        targetY = y;
        _lastCamPos = transform.position;
    }
}
