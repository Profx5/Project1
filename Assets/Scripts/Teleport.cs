using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform rayOrigin;
    public Transform rigRoot;
    public Transform centerEyeAnchor;
    public Transform teleportMarker;

    public float maxDistance = 20f;
    public LayerMask teleportLayer;

    public float snapAngle = 45f;
    public float turnCooldown = 0.3f;

    private float lastTurnTime = 0f;

    private bool triggerWasPressed = false;

    void Update()
    {
        OVRInput.Update();

        HandleSnapTurn();

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, teleportLayer))
        {
            if (teleportMarker != null)
            {
                teleportMarker.gameObject.SetActive(true);
                teleportMarker.transform.position = hit.point + hit.normal * 0.01f;
            }

            float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            bool triggerPressed = trigger > 0.75f;

            if (triggerPressed && !triggerWasPressed)
            {
                TeleportTo(hit.point);
            }

            triggerWasPressed = triggerPressed;
        }
        else
        {
            if (teleportMarker != null)
            {
                teleportMarker.gameObject.SetActive(false);
            }
        }
    }

    void TeleportTo(Vector3 targetPoint)
    {
        Vector3 headOffset = centerEyeAnchor.position - rigRoot.position;
        headOffset.y = 0f;

        Vector3 newRigPosition = targetPoint - headOffset;
        newRigPosition.y = rigRoot.position.y;

        rigRoot.position = newRigPosition;
        Debug.Log("Teleported to: " + targetPoint);
    }

    void HandleSnapTurn()
    {
        float input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;

        if (Time.time - lastTurnTime > turnCooldown)
        {
            if (input > 0.75f)
            {
                Vector3 pivot = centerEyeAnchor.position;
                rigRoot.RotateAround(pivot, Vector3.up, snapAngle);
                lastTurnTime = Time.time;
            }
            else if (input < -0.75f)
            {
                Vector3 pivot = centerEyeAnchor.position;
                rigRoot.RotateAround(pivot, Vector3.up, -snapAngle);
                lastTurnTime = Time.time;
            }
        }
    }
}
