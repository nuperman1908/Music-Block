using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DecisionRequester))]
public class BotControl : Agent
{
    [Header("Spawn")]
    public SpawnPoint[] spawnPoints;
    public bool randomizeSpawn = true;

    [Header("Models")]
    public NNModel EasyModel;
    public NNModel HardModel;

    [Header("Reward")]
    public float distanceRewardScale = 0.001f;
    public float distanceRewardGrowth = 0.05f;
    public float jumpPenalty = -0.0005f;
    public float deathPenalty = -1f;
    public float winReward = 1f;

    private float _lastX;
    private float _episodeSpawnX;

    private Movement _movement;
    private Rigidbody2D _rb;

    private Vector3 _spawnPosition;
    private Gamemodes _spawnGamemode;
    private Speeds _spawnSpeed;
    private int _spawnGravity;
    private float _spawnYLastPortal;

    private Vector3 _checkpointPos;
    private Gamemodes _checkpointGamemode;
    private Speeds _checkpointSpeed;
    private bool _hasCheckpoint;

    public void SetCheckpoint(Vector3 pos, Gamemodes gamemode, Speeds speed)
    {
        _checkpointPos = pos;
        _checkpointGamemode = gamemode;
        _checkpointSpeed = speed;
        _hasCheckpoint = true;
    }

    BehaviorParameters _behaviorParameters;

    private void Start()
    {
        _behaviorParameters = GetComponent<BehaviorParameters>();
        if (PlayerPrefs.GetInt("ChallengeMode") == 1)
        {
            _behaviorParameters.Model = HardModel;
        }
        else
        {
            _behaviorParameters.Model = EasyModel;
        }
    }

    public override void Initialize()
    {
        _movement = GetComponent<Movement>();
        _rb = GetComponent<Rigidbody2D>();

        _movement.isBot = true;

        _spawnPosition = transform.position;
        _spawnGamemode = _movement.CurrentGamemode;
        _spawnSpeed = _movement.CurrentSpeed;
        _spawnGravity = _movement.gravityDirection;
        _spawnYLastPortal = _movement.yLastPortal;

        _movement.OnDied += HandleDeath;
        _movement.OnWon += HandleWin;
    }

    public override void OnEpisodeBegin()
    {
        Vector3 spawnPos = _spawnPosition;
        Gamemodes gamemode = _spawnGamemode;
        Speeds speed = _spawnSpeed;
        int gravity = _spawnGravity;
        float yLastPortal = _spawnYLastPortal;

        if (randomizeSpawn && spawnPoints != null && spawnPoints.Length > 0)
        {
            SpawnPoint sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (sp != null)
            {
                spawnPos = sp.transform.position;
                gamemode = sp.gamemode;
                speed = sp.speed;
                gravity = sp.NormalizedGravity;
                yLastPortal = sp.yLastPortal;
            }
        }

        if (_hasCheckpoint)
        {
            spawnPos = _checkpointPos;
            gamemode = _checkpointGamemode;
            speed = _checkpointSpeed;
        }

        transform.position = spawnPos;
        transform.rotation = Quaternion.identity;

        _movement.CurrentGamemode = gamemode;
        _movement.CurrentSpeed = speed;
        _movement.gravityDirection = gravity;
        _movement.yLastPortal = yLastPortal;
        _movement.clickProcessed = false;
        _movement.enabled = true;

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.simulated = true;
            _rb.gravityScale = Mathf.Abs(_rb.gravityScale) * gravity;
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        _movement.SetBotAction(Movement.ACT_NONE);

        _episodeSpawnX = spawnPos.x;
        _lastX = spawnPos.x;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.y);
        sensor.AddObservation(_rb != null ? _rb.velocity.x : 0f);
        sensor.AddObservation(_rb != null ? _rb.velocity.y : 0f);
        sensor.AddObservation((int)_movement.CurrentGamemode);
        sensor.AddObservation(_movement.gravityDirection);
        sensor.AddObservation(_movement.OnGround() ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int act = actions.DiscreteActions[0];
        int prev = _movement.BotAction;

        if (act == 1)
        {
            bool newPress = prev == Movement.ACT_NONE || prev == Movement.ACT_UP;
            _movement.SetBotAction(newPress ? Movement.ACT_DOWN : Movement.ACT_HOLD);
            if (newPress) AddReward(jumpPenalty);
        }
        else
        {
            _movement.SetBotAction(prev == Movement.ACT_DOWN || prev == Movement.ACT_HOLD
                ? Movement.ACT_UP
                : Movement.ACT_NONE);
        }

        float curX = transform.position.x;
        float dx = curX - _lastX;
        _lastX = curX;
        if (dx > 0f)
        {
            float distance = curX - _episodeSpawnX;
            AddReward(distanceRewardScale * dx * (1f + Mathf.Max(0f, distance) * distanceRewardGrowth));
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        d[0] = Input.GetMouseButton(0) ? 1 : 0;
    }

    private void HandleDeath()
    {
        Debug.Log("botdie");
        AddReward(deathPenalty);
        EndEpisode();
    }

    private void HandleWin()
    {
        AddReward(winReward);
        EndEpisode();
    }

    private void OnDestroy()
    {
        if (_movement != null)
        {
            _movement.OnDied -= HandleDeath;
            _movement.OnWon -= HandleWin;
        }
    }

}
