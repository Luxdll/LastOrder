using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunResultUI : MonoBehaviour
{
    [SerializeField] private RunSession session;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultStats;
    [SerializeField] private Button retryButton;
    [SerializeField] private GameObject crosshair;

    private void Awake()
    {
        if (session == null ||
            resultPanel == null ||
            resultTitle == null ||
            resultStats == null ||
            retryButton == null)
        {
            Debug.LogError(
                "Заполни ссылки в компоненте RunResultUI.",
                this
            );

            enabled = false;
            return;
        }

        resultPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (session != null)
        {
            session.Finished += ShowResult;
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RestartRun);
        }
    }

    private void Start()
    {
        if (session.IsFinished)
        {
            ShowResult();
        }
    }

    private void OnDisable()
    {
        if (session != null)
        {
            session.Finished -= ShowResult;
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RestartRun);
        }
    }

    private void ShowResult()
    {
        resultTitle.text = session.IsDelivered
            ? "Заказ доставлен"
            : "Заказ не доставлен";

        int totalSeconds = Mathf.FloorToInt(session.ElapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        resultStats.text =
            $"Время: {minutes:00}:{seconds:00}\n" +
            $"Уничтожено зомби: {session.KilledCount}\n" +
            $"Осталось сердец: {session.RemainingHealth}";

        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }

        resultPanel.transform.SetAsLastSibling();
        resultPanel.SetActive(true);
    }

    private void RestartRun()
    {
        if (!session.IsFinished)
        {
            return;
        }

        retryButton.interactable = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.path);
    }
}