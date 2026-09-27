using System.Collections;
using UnityEngine;

public class SimpleZipline : MonoBehaviour
{
    [Header("Zipline Targets")]
    public Transform endPoint;
    public float speed = 12f;

    [Header("Visibility Offsets")]
    public float sideOffset = 0.4f;
    public float hangDistance = 1.8f;

    private bool isZipping = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isZipping)
        {
            StartCoroutine(RideZipline(other.gameObject));
        }
    }

    IEnumerator RideZipline(GameObject player)
    {
        isZipping = true;

        CharacterController controller = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (controller != null) controller.enabled = false;
        if (rb != null) rb.isKinematic = true;

        // 1. Calculate stable direction vectors based on markers
        Vector3 heading = endPoint.position - transform.position;
        Vector3 lineDir = heading.normalized;

        Vector3 flatHeading = new Vector3(heading.x, 0, heading.z).normalized;
        Vector3 rightOffsetDirection = new Vector3(flatHeading.z, 0, -flatHeading.x);
        Vector3 totalOffset = (rightOffsetDirection * sideOffset) - new Vector3(0, hangDistance, 0);

        Vector3 startPosWithOffset = transform.position + totalOffset;
        Vector3 targetPosWithOffset = endPoint.position + totalOffset;

        // 2. Project player onto the track to prevent teleporting
        Vector3 lhs = player.transform.position - startPosWithOffset;
        float dotP = Vector3.Dot(lhs, lineDir);
        dotP = Mathf.Clamp(dotP, 0f, heading.magnitude);
        Vector3 caughtPositionOnLine = startPosWithOffset + (lineDir * dotP);

        player.transform.position = caughtPositionOnLine;

        // 3. Set facing direction ONCE at the start of the ride instead of locking it every frame
        Quaternion fixedZiplineRotation = Quaternion.LookRotation(flatHeading, Vector3.up);
        player.transform.rotation = fixedZiplineRotation;

        // 4. Move smoothly down the rail while allowing camera rotation
        while (Vector3.Distance(player.transform.position, targetPosWithOffset) > 0.05f)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                targetPosWithOffset,
                speed * Time.deltaTime
            );

            // Removed the continuous rotation overwrite line!
            yield return null;
        }

        player.transform.position = targetPosWithOffset;

        if (controller != null) controller.enabled = true;
        if (rb != null) rb.isKinematic = false;

        yield return new WaitForSeconds(0.5f);
        isZipping = false;
    }
}


