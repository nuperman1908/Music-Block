using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Speeds { Slow = 0, Normal = 1, Fast = 2, Faster = 3, Fastest = 4 }
public enum Gamemodes { Cube = 0, Ship = 1, Ball = 2, UFO = 3, Wave = 4, Spider = 5 }


public class Movement : MonoBehaviour
{
    public Speeds CurrentSpeed;
    public Gamemodes CurrentGamemode;

    private float[] _speedValues = { 8.6f, 10.4f, 12.96f, 15.6f, 19.27f };

    public float GroundCheckRadius;
    public LayerMask GroundMask;
    public Transform sprite;
    public int gravityDirection = 1;
    public bool clickProcessed = false;

    Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        transform.position += Vector3.right * _speedValues[(int)CurrentSpeed] * Time.fixedDeltaTime;
        Invoke(CurrentGamemode.ToString(), 0);
    }

    private void Jump()
    {

        _rb.velocity = Vector2.zero;
        _rb.AddForce(Vector2.up * 26.6581f * gravityDirection, ForceMode2D.Impulse);

    }

    public bool OnGround()
    {
        return Physics2D.OverlapBox(transform.position + Vector3.down * gravityDirection * 0.5f, Vector2.right * 1.1f + Vector2.up * GroundCheckRadius, 0, GroundMask);

    }
    void Cube()
    {
        generic.CreateGamemode(_rb, this, true, 19.5269f, 9.057f, true, false, 409.1f);
    }

    void Ship()
    {
        transform.rotation = Quaternion.Euler(0, 0, _rb.velocity.y * 2f);
        if (Input.GetButton("Fire1"))
        {
            _rb.gravityScale = -4.314969f;
        }
        else
        {
            _rb.gravityScale = 4.314969f;
        }
        _rb.gravityScale *= gravityDirection;
    }
    void Ball()
    {
        generic.CreateGamemode(_rb, this, true, 0, 6.2f, false, true);
    }
    void UFO()
    {
        generic.CreateGamemode(_rb, this, false, 10.841f, 4.1483f, false, false, 0, 10.841f);
    }
    void Wave()
    {
        _rb.gravityScale = 0;
        _rb.velocity = new Vector2(0, _speedValues[(int)CurrentSpeed] * (Input.GetMouseButton(0) ? 1 : -1) * gravityDirection);
    }
    void Spider()
    {
        generic.CreateGamemode(_rb, this, true, 238.29f, 6.2f, false, true, 0, 238.29f);
    }

    public void ChangeThroughPortal(Gamemodes gamemodes, Speeds speed, int gravity, int State)
    {
        switch (State)
        {
            case 0:
                CurrentSpeed = speed;
                break;
            case 1:
                CurrentGamemode = gamemodes;
                break;
            case 2:
                gravityDirection = gravity;
                _rb.gravityScale = MathF.Abs(_rb.gravityScale) * (int)gravity;
                break;
        }
    }
}
