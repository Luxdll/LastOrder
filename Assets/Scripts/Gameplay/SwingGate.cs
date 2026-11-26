using System.Collections;
using UnityEngine;

public class SwingGate : MonoBehaviour
{
    [SerializeField] private Transform leftHinge;
    [SerializeField] private Transform rightHinge;
    [SerializeField, Range(1f, 120f)] private float openingAngle = 90f;
    [SerializeField, Min(0.01f)] private float openingDuration = 1.2f;

    private bool isOpening;

    public bool IsOpen { get; private set; }

    [ContextMenu("Open Gate (Play Mode)")]
    public void Open()
    {
        if (!Application.isPlaying ||
            !isActiveAndEnabled ||
            IsOpen ||
            isOpening)
        {
            return;
        }

        if (leftHinge == null || rightHinge == null)
        {
            Debug.LogError("Не назначены петли ворот.", this);
            return;
        }

        isOpening = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Quaternion leftStart = leftHinge.localRotation;
        Quaternion rightStart = rightHinge.localRotation;

        Quaternion leftEnd =
            leftStart * Quaternion.Euler(0f, openingAngle, 0f);

        Quaternion rightEnd =
            rightStart * Quaternion.Euler(0f, -openingAngle, 0f);

        float duration = Mathf.Max(0.01f, openingDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            leftHinge.localRotation =
                Quaternion.Slerp(leftStart, leftEnd, progress);

            rightHinge.localRotation =
                Quaternion.Slerp(rightStart, rightEnd, progress);

            yield return null;
        }

        leftHinge.localRotation = leftEnd;
        rightHinge.localRotation = rightEnd;

        IsOpen = true;
        isOpening = false;
    }
}