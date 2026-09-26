using UnityEngine;
using EasyPeasyFirstPersonController;

public class BouncePad : MonoBehaviour
{
    public float launchForce = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        FirstPersonController player =
            other.GetComponent<FirstPersonController>();

        if (player != null)
        {
            player.moveDirection.y = launchForce;
        }
    }
}