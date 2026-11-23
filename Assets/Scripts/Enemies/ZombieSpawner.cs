using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private ZombieMovement zombiePrefab;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Transform spawnPointsRoot;

    [SerializeField, Min(1)] private int totalZombies = 100;
    [SerializeField, Min(1)] private int maxAlive = 10;
    [SerializeField, Min(0f)] private float spawnRadius = 2f;
    [SerializeField, Min(1f)] private float minPlayerDistance = 6f;

    private readonly List<Health> spawnedZombies = new();

    private Health playerHealth;
    private PlayerShooting playerShooting;
    private LineRenderer playerTracer;
    private Animator playerAnimator;
    private Transform[] spawnPoints;
    private NavMeshQueryFilter navigationFilter;

    private int spawnedCount;
    private int killedCount;
    private int aliveCount;
    private bool hasStarted;
    private float nextSpawnTime;

    public int AliveCount => aliveCount;
    public int KilledCount => killedCount;
    public int RemainingKills => Mathf.Max(0, totalZombies - killedCount);
    public bool IsFinished { get; private set; }
    public bool IsVictory { get; private set; }

    private void Start()
    {
        if (zombiePrefab == null ||
            player == null ||
            spawnPointsRoot == null ||
            spawnPointsRoot.childCount == 0)
        {
            Debug.LogError(
                "Назначь Zombie Prefab, Player и Spawn Points Root.",
                this
            );

            enabled = false;
            return;
        }

        playerHealth = player.GetComponent<Health>();
        playerShooting = player.GetComponent<PlayerShooting>();
        playerTracer = player.GetComponent<LineRenderer>();
        playerAnimator = player.GetComponentInChildren<Animator>();

        if (playerHealth == null)
        {
            Debug.LogError("На Player отсутствует Health.", player);
            enabled = false;
            return;
        }

        spawnPoints = new Transform[spawnPointsRoot.childCount];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i] = spawnPointsRoot.GetChild(i);
        }

        NavMeshAgent prefabAgent =
            zombiePrefab.GetComponent<NavMeshAgent>();

        navigationFilter = new NavMeshQueryFilter
        {
            agentTypeID = prefabAgent.agentTypeID,
            areaMask = NavMesh.AllAreas
        };
    }

    private void Update()
    {
        if (IsFinished ||
            playerHealth.IsDead ||
            !player.isActiveAndEnabled)
        {
            return;
        }

        hasStarted = true;

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        nextSpawnTime = Time.time + 0.25f;

        while (aliveCount < maxAlive && spawnedCount < totalZombies)
        {
            if (!TrySpawnZombie())
            {
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (!hasStarted || IsFinished)
        {
            return;
        }

        if (playerHealth.IsDead)
        {
            Finish(false);
        }
        else if (killedCount >= totalZombies)
        {
            Finish(true);
        }
    }

    private bool TrySpawnZombie()
    {
        Physics.SyncTransforms();

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Transform spawnPoint =
                spawnPoints[Random.Range(0, spawnPoints.Length)];

            Vector2 offset = Random.insideUnitCircle * spawnRadius;

            Vector3 candidate = spawnPoint.position
                + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    1.5f,
                    navigationFilter))
            {
                continue;
            }

            Vector3 direction = player.transform.position - hit.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <
                minPlayerDistance * minPlayerDistance)
            {
                continue;
            }

            if (Physics.CheckSphere(
                    hit.position + Vector3.up * 0.9f,
                    0.6f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            ZombieMovement zombie = Instantiate(
                zombiePrefab,
                hit.position,
                Quaternion.LookRotation(direction),
                transform
            );

            zombie.Initialize(player);

            Health zombieHealth = zombie.GetComponent<Health>();
            zombieHealth.Died += HandleZombieDeath;
            spawnedZombies.Add(zombieHealth);

            spawnedCount++;
            aliveCount++;

            return true;
        }

        return false;
    }

    private void HandleZombieDeath()
    {
        aliveCount--;
        killedCount++;
    }

    private void Finish(bool victory)
    {
        IsFinished = true;
        IsVictory = victory;

        player.enabled = false;

        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }

        if (playerTracer != null)
        {
            playerTracer.enabled = false;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            victory ? "Все зомби уничтожены. Победа!" : "Поражение.",
            this
        );
    }

    private void OnDestroy()
    {
        foreach (Health zombieHealth in spawnedZombies)
        {
            if (zombieHealth != null)
            {
                zombieHealth.Died -= HandleZombieDeath;
            }
        }
    }
}