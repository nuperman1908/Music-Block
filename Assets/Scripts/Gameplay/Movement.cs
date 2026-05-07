using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Speeds { Slow = 0, Normal = 1, Fast = 2, Faster = 3, Fastest = 4 }
public enum Gamemodes { Cube = 0, Ship = 1, Ball = 2, UFO = 3, Wave = 4, Robot = 5, Spider = 6 }


public class Movement : MonoBehaviour
{
    public Speeds CurrentSpeed;
    public Gamemodes CurrentGamemode;

    private float[] _speedValues = { 8.6f, 10.4f, 12.96f, 15.6f, 19.27f };

    [System.NonSerialized] public int[] screenHeightValues = { 11, 10, 8, 10, 10, 11, 9 };
    [System.NonSerialized] public float yLastPortal= -2.3f;

    public float GroundCheckRadius;
    public LayerMask GroundMask;
    public Transform sprite;
    public int gravityDirection = 1;
    public bool clickProcessed = false;
    public Transform groundPlatform;

    Rigidbody2D _rb;
    TrailRenderer _trail;

    private float timer = 0f;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _trail =  GetComponent<TrailRenderer>();
    }
    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        groundPlatform.position = new Vector3(transform.position.x, groundPlatform.position.y, groundPlatform.position.z);
        /*
        transform.position += Vector3.right * _speedValues[(int)CurrentSpeed] * Time.fixedDeltaTime;
        */
        if (!_onSlope)
            transform.position += Vector3.right * _speedValues[(int)CurrentSpeed] * Time.fixedDeltaTime;
        else
        {
            float targetSpeed = _speedValues[(int)CurrentSpeed];
            _rb.velocity = new Vector2(targetSpeed, targetSpeed * gravityDirection);
        }

        Invoke(CurrentGamemode.ToString(), 0);
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
        _rb.gravityScale = 2.93f * (Input.GetMouseButton(0) ? -1 : 1) * gravityDirection;
        generic.LimitYVelocity(9.95f, _rb);
        transform.rotation = Quaternion.Euler(0, 0, _rb.velocity.y * 2f);
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
        float speed = _speedValues[(int)CurrentSpeed];
        float verticalDir = (Input.GetMouseButton(0) ? 1 : -1) * gravityDirection;
        _rb.velocity = new Vector2(0, speed * verticalDir);

        Vector2 moveDir = new Vector2(speed, speed * verticalDir).normalized;
        sprite.right = moveDir;
    }
    float _robotXstart = -100;
    bool _onGroundProcessed;
    bool _gravityFlipped;
    void Robot()
    {
        if (!Input.GetMouseButton(0))
        {
            clickProcessed = false;
        }
        if (OnGround() && !clickProcessed && Input.GetMouseButton(0))
        {
            _gravityFlipped = false;
            clickProcessed = true;
            _robotXstart = transform.position.x;
            _onGroundProcessed = true;
        }
        if (Mathf.Abs(_robotXstart - transform.position.x) <= 3)
        {
            if (Input.GetMouseButton(0) && !_gravityFlipped && _onGroundProcessed)
            {
                _rb.gravityScale = 0;
                _rb.velocity = Vector2.up * 10.4f * gravityDirection;
                return;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            _onGroundProcessed = false;
        }
        _rb.gravityScale = 8.62f * gravityDirection;
        generic.LimitYVelocity(23.66f, _rb);
    }
    void Spider()
    {
        generic.CreateGamemode(_rb, this, true, 238.29f, 6.2f, false, true, 0, 238.29f);
    }

    public void ChangeThroughPortal(Gamemodes gamemodes, Speeds speed, int gravity, int State, float yPortal)
    {
        switch (State)
        {
            case 0:
                CurrentSpeed = speed;
                break;
            case 1:
                yLastPortal = yPortal;
                CurrentGamemode = gamemodes;
                break;
            case 2:
                gravityDirection = gravity;
                _rb.velocity = new Vector2(_rb.velocity.x, 0f);
                _rb.gravityScale = MathF.Abs(_rb.gravityScale) * (int)gravity;
                _gravityFlipped = true;
                break;
        }
    }
    public void ApplyOrbJump(float force)
    {
        _rb.velocity = Vector2.up * force * gravityDirection;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Portal":
                PortalScript portal = collision.GetComponent<PortalScript>();
                if (portal != null)
                {
                    portal.initiatePortal(this);
                }
                break;
        }
    }
    float safeAngleThreshold = 45f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DamageZone"))
        {
            Die();
            return;
        }

        if (!collision.gameObject.CompareTag("Block")) return;
        if (CurrentGamemode == Gamemodes.Wave)
        {
            Die();
            return;
        }
        foreach (ContactPoint2D contact in collision.contacts)
        {
            
            Vector2 safeNormal = Vector2.up * gravityDirection;
            float angle = Vector2.Angle(contact.normal, safeNormal);

            if (angle < safeAngleThreshold)
            {
                return;
            }
            else
            {
                Die();
                return;
            }
        }
    }
    
    private bool _onSlope;
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Slope")) return;
        _onSlope = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            if (Mathf.Abs(normal.x) > 0.01f && Mathf.Abs(normal.y) > 0.01f)
            {
                _onSlope = true;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Slope")) return;
        if (_onSlope)
        {
            _onSlope = false;
            _rb.velocity = new Vector2(0f, _rb.velocity.y);
        }
    }
    private void Die()
    {
        Debug.Log(timer);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (_trail != null)
        {
            _trail.Clear();
            _trail = null;
        }
    }
}
