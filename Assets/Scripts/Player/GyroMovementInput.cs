using UnityEngine;
using UnityEngine.InputSystem;

public class GyroMovementInput : MonoBehaviour
{
    [SerializeField, Range(1f, 60f)]
    private float maxTiltDegrees = 20f;

    [SerializeField, Range(0f, 0.5f)]
    private float deadZone = 0.1f;

    [SerializeField, Min(0.1f)]
    private float smoothing = 10f;

    private AttitudeSensor sensor;
    private Quaternion neutralAttitude;
    private ScreenOrientation calibratedOrientation;

    private bool enabledSensorHere;
    private bool isCalibrated;
    private Vector2 currentInput;

    private void OnEnable()
    {
        TryEnableSensor();
    }

    private void OnDisable()
    {
        if (enabledSensorHere &&
            sensor != null &&
            sensor.added &&
            sensor.enabled)
        {
            InputSystem.DisableDevice(sensor);
        }

        sensor = null;
        enabledSensorHere = false;
        isCalibrated = false;
        currentInput = Vector2.zero;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            isCalibrated = false;
            currentInput = Vector2.zero;
        }
    }

    private void TryEnableSensor()
    {
        sensor = AttitudeSensor.current;

        if (sensor == null)
        {
            return;
        }

        enabledSensorHere = !sensor.enabled;

        if (enabledSensorHere)
        {
            InputSystem.EnableDevice(sensor);
        }

        isCalibrated = false;
        currentInput = Vector2.zero;
    }

    public Vector2 ReadInput()
    {
        if (sensor == null || !sensor.added || !sensor.enabled)
        {
            TryEnableSensor();
        }

        if (sensor == null || !sensor.enabled)
        {
            return Vector2.zero;
        }

        Quaternion attitude = sensor.attitude.ReadValue();

        if (Quaternion.Dot(attitude, attitude) < 0.5f)
        {
            return Vector2.zero;
        }

        attitude = attitude.normalized;

        if (!isCalibrated ||
            calibratedOrientation != Screen.orientation)
        {
            neutralAttitude = attitude;
            calibratedOrientation = Screen.orientation;
            isCalibrated = true;
            currentInput = Vector2.zero;

            return Vector2.zero;
        }

        Quaternion relativeRotation =
            Quaternion.Inverse(neutralAttitude) * attitude;

        Vector3 direction =
            relativeRotation * Vector3.forward;

        float tiltScale =
            Mathf.Sin(maxTiltDegrees * Mathf.Deg2Rad);

        Vector2 input = new Vector2(
            direction.x,
            direction.y
        ) / tiltScale;

        input = Vector2.ClampMagnitude(input, 1f);

        float magnitude = input.magnitude;

        if (magnitude <= deadZone)
        {
            input = Vector2.zero;
        }
        else
        {
            input = input.normalized *
                ((magnitude - deadZone) / (1f - deadZone));
        }

        float blend = 1f - Mathf.Exp(-smoothing * Time.deltaTime);

        currentInput = Vector2.Lerp(currentInput, input, blend);

        return currentInput;
    }
}