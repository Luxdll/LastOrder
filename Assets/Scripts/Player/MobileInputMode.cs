using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MobileInputMode : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject mobileControls;
    [SerializeField] private bool previewInEditor = true;

    public static bool IsActive { get; private set; }

    private void Awake()
    {
        if (player == null || mobileControls == null)
        {
            Debug.LogError(
                "Назначь Player и Mobile Controls в MobileInputMode.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        IsActive = Application.isMobilePlatform ||
            (Application.isEditor && previewInEditor);

        SetControlsVisible(false);
    }

    private void Update()
    {
        SetControlsVisible(
            IsActive && player.isActiveAndEnabled
        );
    }

    private void OnDisable()
    {
        IsActive = false;
        SetControlsVisible(false);
    }

    private void SetControlsVisible(bool visible)
    {
        if (mobileControls != null &&
            mobileControls.activeSelf != visible)
        {
            mobileControls.SetActive(visible);
        }
    }
}