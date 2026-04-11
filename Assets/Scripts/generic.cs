
using UnityEngine;

static public class generic
{
    static public void LimitYVelocity(float limit, Rigidbody2D rb)
    {
        int gravityMultiplier = (int)(Mathf.Abs(rb.velocity.y) / rb.velocity.y);
        if (rb.velocity.y * -gravityMultiplier > limit)
        {
            rb.velocity = Vector2.up * -limit * gravityMultiplier;
        }
    }
    static public void CreateGamemode(Rigidbody2D rb, Movement host, bool onGroundRequired, float initalVelocity, float gravityScale, bool canHold = false, bool flipOnClick = false, float rotationMod = 0, float yVelocityLimit = Mathf.Infinity)
    {
        if (!Input.GetMouseButton(0) || canHold && host.OnGround())
        {
            host.clickProcessed = false;
        }
        rb.gravityScale = gravityScale * host.gravityDirection;
        LimitYVelocity(yVelocityLimit, rb);
        if (Input.GetMouseButton(0))
        {
            if (host.OnGround() && !host.clickProcessed || !onGroundRequired && !host.clickProcessed)
            {
                host.clickProcessed = true;
                rb.velocity = Vector2.up * initalVelocity * host.gravityDirection;
                host.gravityDirection *= flipOnClick ? -1 : 1;
            }
        } 
        if(host.OnGround() || !onGroundRequired)
        {
            host.sprite.rotation = Quaternion.Euler(0, 0, Mathf.Round(host.sprite.rotation.z / 90) * 90);
        }
        else
        {
            host.sprite.Rotate(Vector3.back, rotationMod * Time.deltaTime * host.gravityDirection);
        }
    }
}
