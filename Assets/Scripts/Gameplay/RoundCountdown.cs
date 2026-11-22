using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MusicPlayback), typeof(AudioSource))]
public class RoundCountdown : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject crosshair;

    private const int CountdownSeconds = 5;

    public void BeginCountdown()
    {
        if (!startButton.interactable)
        {
            return;
        }

        startButton.interactable = false;
        countdownText.text = CountdownSeconds.ToString();
        GetComponent<MusicPlayback>().PlayBeat();
        StartCoroutine(ShowCountdown(GetComponent<AudioSource>()));
    }

    private IEnumerator ShowCountdown(AudioSource music)
    {
        yield return null;

        while (music.isPlaying)
        {
            float elapsed = (float)music.timeSamples / music.clip.frequency;
            int remaining = Mathf.CeilToInt(CountdownSeconds - elapsed);

            if (remaining <= 0)
            {
                startButton.gameObject.SetActive(false);
                crosshair.SetActive(true);
                playerMovement.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                yield break;
            }

            countdownText.text = remaining.ToString();
            yield return null;
        }

        countdownText.text = "START";
        startButton.interactable = true;
    }
}