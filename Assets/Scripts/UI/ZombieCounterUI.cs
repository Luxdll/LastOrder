using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ZombieCounterUI : MonoBehaviour
{
    [SerializeField] private ZombieSpawner spawner;

    private TMP_Text counterText;
    private int displayedRemaining = -1;

    private void Awake()
    {
        counterText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (spawner == null)
        {
            Debug.LogError(
                "Назначь Spawner в ZombieCounterUI.",
                this
            );

            enabled = false;
            return;
        }

        Refresh();
    }

    private void LateUpdate()
    {
        if (spawner != null &&
            spawner.RemainingKills != displayedRemaining)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        displayedRemaining = spawner.RemainingKills;
        counterText.text = $"Осталось: {displayedRemaining}";
    }
}