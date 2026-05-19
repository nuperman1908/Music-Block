using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Gamemodes gamemode = Gamemodes.Cube;
    public Speeds speed = Speeds.Normal;
    [Tooltip("1 = trọng lực hướng xuống (mặc định), -1 = lật ngược")]
    public int gravityDirection = 1;
    public float yLastPortal = -2.3f;

    public int NormalizedGravity => gravityDirection >= 0 ? 1 : -1;

    private void OnDrawGizmos()
    {
        Gizmos.color = gravityDirection >= 0 ? Color.green : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Vector3 arrow = Vector3.up * 0.6f * NormalizedGravity;
        Gizmos.DrawLine(transform.position, transform.position + arrow);
    }
}
