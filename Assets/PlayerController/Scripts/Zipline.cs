using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using EasyPeasyFirstPersonController;

public class Zipline : MonoBehaviour
{
    [Header("Zipline Targets")]
    public Transform endPoint;

    [Header("Zipline Speed")]
    public float speed = 12f;

    [Header("Player Position")]
    public float sideOffset = 0.4f;
    public float hangDistance = 1.8f;

    [Header("Detach")]
    public float detachForwardBoost = 5f;

    private bool isZipping = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isZipping)
            return;

        if (!other.CompareTag("Player"))
            return;

        FirstPersonController controller =
            other.GetComponent<FirstPersonController>();

        if (controller == null)
            return;

        StartCoroutine(
            RideZipline(
                other.gameObject,
                controller
            )
        );
    }

    private IEnumerator RideZipline(
        GameObject player,
        FirstPersonController controller)
    {
        isZipping = true;

        CharacterController characterController =
            controller.characterController;

        if (characterController == null)
        {
            characterController =
                player.GetComponent<CharacterController>();
        }

        if (characterController == null)
        {
            isZipping = false;
            yield break;
        }

        // =====================================================
        // ENTER ZIPLINE
        // =====================================================

        controller.isZiplining = true;

        // Remove any old movement/falling velocity.
        controller.currentVelocity =
            Vector3.zero;

        // =====================================================
        // CALCULATE ZIPLINE
        // =====================================================

        Vector3 heading =
            endPoint.position -
            transform.position;

        float lineLength =
            heading.magnitude;

        if (lineLength <= 0.001f)
        {
            controller.isZiplining = false;
            isZipping = false;
            yield break;
        }

        Vector3 lineDir =
            heading.normalized;

        Vector3 flatHeading =
            new Vector3(
                heading.x,
                0f,
                heading.z
            ).normalized;

        Vector3 rightDirection =
            new Vector3(
                flatHeading.z,
                0f,
                -flatHeading.x
            );

        Vector3 offset =
            (rightDirection * sideOffset) -
            (Vector3.up * hangDistance);

        Vector3 startPosition =
            transform.position +
            offset;

        Vector3 endPosition =
            endPoint.position +
            offset;

        // =====================================================
        // ATTACH PLAYER
        // =====================================================

        Vector3 playerToStart =
            player.transform.position -
            startPosition;

        float distanceAlongLine =
            Vector3.Dot(
                playerToStart,
                lineDir
            );

        distanceAlongLine =
            Mathf.Clamp(
                distanceAlongLine,
                0f,
                lineLength
            );

        Vector3 attachPosition =
            startPosition +
            lineDir *
            distanceAlongLine;

        characterController.Move(
            attachPosition -
            player.transform.position
        );

        // =====================================================
        // FACE ZIPLINE
        // =====================================================

        if (flatHeading.sqrMagnitude > 0.001f)
        {
            player.transform.rotation =
                Quaternion.LookRotation(
                    flatHeading,
                    Vector3.up
                );
        }

        // =====================================================
        // RIDE
        // =====================================================

        while (true)
        {
            // SPACE = DETACH
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Detach(
                    player,
                    controller
                );

                yield break;
            }

            float distanceToEnd =
                Vector3.Distance(
                    player.transform.position,
                    endPosition
                );

            if (distanceToEnd <= 0.05f)
                break;

            Vector3 nextPosition =
                Vector3.MoveTowards(
                    player.transform.position,
                    endPosition,
                    speed * Time.deltaTime
                );

            Vector3 movement =
                nextPosition -
                player.transform.position;

            characterController.Move(
                movement
            );

            yield return null;
        }

        // =====================================================
        // REACHED END
        // =====================================================

        controller.isZiplining = false;

        controller.currentVelocity =
            Vector3.zero;

        isZipping = false;
    }

    private void Detach(
        GameObject player,
        FirstPersonController controller)
    {
        // Stop zipline mode
        controller.isZiplining = false;

        // -----------------------------------------------------
        // Give the player forward momentum
        // -----------------------------------------------------

        Vector3 forwardBoost =
            player.transform.forward *
            detachForwardBoost;

        controller.currentVelocity =
            forwardBoost;

        isZipping = false;
    }
}