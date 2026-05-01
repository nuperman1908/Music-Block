using UnityEngine;

public enum OrbTypes { JumpLow, JumpNormal, JumpHigh, GravityFlip }

public class OrbScript : MonoBehaviour
{
    public OrbTypes orbType;

    private static readonly float[] jumpForces = { 13f, 19.5269f, 26f };

    private Movement _player;
    private bool _inRange;
    private bool _clicked = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        _player = col.GetComponent<Movement>();
        _inRange = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        _player = null;
        _inRange = false;
    }

    private void Update()
    {
        if (_clicked) return;
        if (!_inRange || _player == null) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (_player.clickProcessed) return;

        Activate();
        _clicked = true;
        _player.clickProcessed = true;
    }

    private void Activate()
    {
        switch (orbType)
        {
            case OrbTypes.JumpLow:
            case OrbTypes.JumpNormal:
            case OrbTypes.JumpHigh:
                _player.ApplyOrbJump(jumpForces[(int)orbType]);
                break;

            case OrbTypes.GravityFlip:
                _player.ChangeThroughPortal(
                    _player.CurrentGamemode,
                    _player.CurrentSpeed,
                    _player.gravityDirection == 1 ? -1 : 1,
                    2,
                    _player.yLastPortal 
                );
                break;
        }
    }
}