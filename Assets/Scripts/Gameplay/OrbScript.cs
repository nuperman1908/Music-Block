using UnityEngine;

public enum OrbTypes { JumpLow, JumpNormal, JumpHigh, GravityFlip }

public class OrbScript : MonoBehaviour
{
    public OrbTypes orbType;

    private static readonly float[] jumpForces = { 13f, 19.5269f, 26f };

    private Movement _user;
    private bool _inRange;
    private bool _clicked = false;

    private static bool IsControllable(Collider2D col)
    {
        return col.CompareTag("Player") || col.CompareTag("Bot");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsControllable(col)) return;
        Movement m = col.GetComponent<Movement>();
        if (m == null) return;
        _user = m;
        _inRange = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!IsControllable(col)) return;
        if (_user != null && col.gameObject != _user.gameObject) return;
        _user = null;
        _inRange = false;
    }

    private void Update()
    {
        if (_clicked) return;
        if (!_inRange || _user == null) return;
        if (_user.GetMouseButtonUp(0) || _user.GetMouseButtonDown(0))
        {
            if (_user.clickProcessed) return;

            Activate();
            _clicked = true;
            _user.clickProcessed = true;
        }
    }

    private void Activate()
    {
        switch (orbType)
        {
            case OrbTypes.JumpLow:
            case OrbTypes.JumpNormal:
            case OrbTypes.JumpHigh:
                _user.ApplyOrbJump(jumpForces[(int)orbType]);
                break;

            case OrbTypes.GravityFlip:
                _user.ChangeThroughPortal(
                    _user.CurrentGamemode,
                    _user.CurrentSpeed,
                    _user.gravityDirection == 1 ? -1 : 1,
                    2,
                    _user.yLastPortal
                );
                break;
        }
    }
}
