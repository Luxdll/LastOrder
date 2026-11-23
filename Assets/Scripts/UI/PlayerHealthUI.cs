using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private UnityEngine.UI.Image heartTemplate;
    [SerializeField, Min(1f)] private float spacing = 36f;

    [SerializeField]
    private Color emptyColor =
        new Color(0.25f, 0.28f, 0.32f, 1f);

    private UnityEngine.UI.Image[] hearts;
    private Color fullColor;
    private int displayedHealth = -1;

    private void Start()
    {
        if (playerHealth == null || heartTemplate == null)
        {
            Debug.LogError(
                "Назначь Player Health и Heart Template в PlayerHealthUI.",
                this
            );

            enabled = false;
            return;
        }

        fullColor = heartTemplate.color;
        hearts = new UnityEngine.UI.Image[playerHealth.MaxHealth];

        Vector2 firstPosition =
            heartTemplate.rectTransform.anchoredPosition;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i == 0)
            {
                hearts[i] = heartTemplate;
            }
            else
            {
                hearts[i] = Instantiate(
                    heartTemplate,
                    heartTemplate.transform.parent
                );

                hearts[i].gameObject.name = $"Heart{i + 1:00}";
            }

            hearts[i].rectTransform.anchoredPosition =
                firstPosition + Vector2.right * spacing * i;
        }

        Refresh();
    }

    private void LateUpdate()
    {
        if (playerHealth != null &&
            playerHealth.CurrentHealth != displayedHealth)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        displayedHealth = playerHealth.CurrentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].color =
                i < displayedHealth ? fullColor : emptyColor;
        }
    }
}