using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private ZombieMovement zombiePrefab;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Transform spawnPointsRoot;
    [SerializeField] private SwingGate exitGate;

    [SerializeField, Min(1)] private int totalZombies = 50;
    [SerializeField, Min(1)] private int maxAlive = 10;
    [SerializeField, Min(0f)] private float spawnRadius = 1.5f;
    [SerializeField, Min(1f)] private float minPlayerDistance = 6f;
    [SerializeField, Min(1f)] private float minZombieDistance = 5f;

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
    private int nextSpawnPointIndex;
    private bool hasStarted;
    private float nextSpawnTime;

    public int AliveCount => aliveCount;
    public int KilledCount => killedCount;
    public int RemainingKills => Mathf.Max(0, totalZombies - killedCount);
    public bool IsCleared { get; private set; }
    public bool IsFinished { get; private set; }

    private void Start()
    {
        if (zombiePrefab == null ||
            player == null ||
            spawnPointsRoot == null ||
            spawnPointsRoot.childCount == 0 ||
            exitGate == null)
        {
            Debug.LogError(
                "Назначь Zombie Prefab, Player, Spawn Points Root и Exit Gate.",
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

        NavMeshAgent prefabAgent =
            zombiePrefab.GetComponent<NavMeshAgent>();

        if (prefabAgent == null ||
            zombiePrefab.GetComponent<Health>() == null)
        {
            Debug.LogError(
                "На префабе зомби нужны NavMeshAgent и Health.",
                zombiePrefab
            );

            enabled = false;
            return;
        }

        spawnPoints = new Transform[spawnPointsRoot.childCount];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i] = spawnPointsRoot.GetChild(i);
        }

        ShuffleSpawnPoints();

        navigationFilter = new NavMeshQueryFilter
        {
            agentTypeID = prefabAgent.agentTypeID,
            areaMask = NavMesh.AllAreas
        };
    }

    private void ShuffleSpawnPoints()
    {
        for (int i = spawnPoints.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Transform temporary = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randomIndex];
            spawnPoints[randomIndex] = temporary;
        }
    }

    private void Update()
    {
        if (IsFinished ||
            IsCleared ||
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
            FinishDefeat();
        }
        else if (!IsCleared && killedCount >= totalZombies)
        {
            CompleteWarehouse();
        }
    }

    private bool TrySpawnZombie()
    {
        Physics.SyncTransforms();

        int maxAttempts = Mathf.Max(30, spawnPoints.Length * 3);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Transform spawnPoint = spawnPoints[nextSpawnPointIndex];

            nextSpawnPointIndex =
                (nextSpawnPointIndex + 1) % spawnPoints.Length;

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

            if (!IsFarFromOtherZombies(hit.position))
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

    private bool IsFarFromOtherZombies(Vector3 position)
    {
        float minimumDistanceSquared =
            minZombieDistance * minZombieDistance;

        foreach (Health zombieHealth in spawnedZombies)
        {
            if (zombieHealth == null || zombieHealth.IsDead)
            {
                continue;
            }

            Vector3 difference =
                zombieHealth.transform.position - position;

            difference.y = 0f;

            if (difference.sqrMagnitude < minimumDistanceSquared)
            {
                return false;
            }
        }

        return true;
    }

    private void HandleZombieDeath()
    {
        aliveCount--;
        killedCount++;
    }

    private void CompleteWarehouse()
    {
        IsCleared = true;
        exitGate.Open();

        Debug.Log("Склад зачищен. Выход во двор открыт.", this);
    }

    private void FinishDefeat()
    {
        IsFinished = true;

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

        Debug.Log("Поражение.", this);
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