using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FinishZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RunSession session =
            other.GetComponentInParent<RunSession>();

        if (session != null)
        {
            session.CompleteDelivery();
        }
    }
}