
using UnityEngine;

public enum JumpPadType { JumpLow, JumpNormal, JumpHigh, GravityFlip }

public class JumpPadScript : MonoBehaviour
{
    public JumpPadType orbType;

    private static readonly float[] jumpForces = { 13f, 19.5269f, 26f };

    private Movement _player;
    

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        _player = col.GetComponent<Movement>();
        switch (orbType)
        {
            case JumpPadType.JumpLow:
            case JumpPadType.JumpNormal:
            case JumpPadType.JumpHigh:
                _player.ApplyOrbJump(jumpForces[(int)orbType]);
                break;

            case JumpPadType.GravityFlip:
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