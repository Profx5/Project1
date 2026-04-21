using UnityEngine;

public class VRSpawner : MonoBehaviour
{
    [Header("Spawnable Prefabs")]
    public GameObject prefab1;
    public GameObject prefab2;

    [Header("References")]
    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;

    [Header("Hold Position")]
    public float holdDistance = 2.5f;
    public float holdHeightOffset = -0.2f;

    [Header("Scale Settings")]
    public float scaleSpeed = 2.0f;
    public float minScale = 0.2f;
    public float maxScale = 3.0f;
    public float triggerPressThreshold = 0.75f;

    private GameObject heldObject;
    private Vector3 previousHandPosition;
    private bool wasRightTriggerPressed = false;

    // Prevent the same trigger press used for pickup from also releasing
    private bool waitingForTriggerRelease = false;

    void Update()
    {
        OVRInput.Update();

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        bool triggerPressed = triggerValue > triggerPressThreshold;
        bool triggerDown = triggerPressed && !wasRightTriggerPressed;
        bool gripHeld = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);

        if (heldObject != null)
        {
            if (triggerPressed && gripHeld)
            {
                UpdateHeldObjectScale();
            }
            else
            {
                UpdateHeldObjectPosition();
                UpdateHeldObjectRotation();
            }

            // Ignore trigger presses until user lets go once after pickup/spawn
            if (waitingForTriggerRelease)
            {
                if (!triggerPressed)
                {
                    waitingForTriggerRelease = false;
                }
            }
            else
            {
                // Press T again to place/release
                if (triggerDown && !gripHeld)
                {
                    ReleaseHeldObject();
                }
            }

            previousHandPosition = rightHandAnchor.position;
        }

        wasRightTriggerPressed = triggerPressed;
    }

    public void SpawnPrefab1()
    {
        if (heldObject != null) return;
        Spawn(prefab1);
    }

    public void SpawnPrefab2()
    {
        if (heldObject != null) return;
        Spawn(prefab2);
    }

    public void SelectExistingObject(GameObject obj)
    {
        if (obj == null) return;
        if (heldObject != null) return;

        heldObject = obj;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        heldObject.transform.position = GetHoldPosition();
        heldObject.transform.rotation = rightHandAnchor.rotation;

        previousHandPosition = rightHandAnchor.position;
        waitingForTriggerRelease = true;

        Debug.Log("Selected existing object: " + obj.name);
    }

    public bool HasHeldObject()
    {
        return heldObject != null;
    }

    void Spawn(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPos = GetHoldPosition();
        Quaternion spawnRot = rightHandAnchor.rotation;

        heldObject = Instantiate(prefab, spawnPos, spawnRot);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        previousHandPosition = rightHandAnchor.position;
        waitingForTriggerRelease = true;

        Debug.Log("Spawned prefab: " + prefab.name);
    }

    Vector3 GetHoldPosition()
    {
        Vector3 forward = centerEyeAnchor.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 holdPos = centerEyeAnchor.position + forward * holdDistance;
        holdPos.y = rightHandAnchor.position.y + holdHeightOffset;

        return holdPos;
    }

    void UpdateHeldObjectPosition()
    {
        if (heldObject == null) return;
        heldObject.transform.position = GetHoldPosition();
    }

    void UpdateHeldObjectRotation()
    {
        if (heldObject == null) return;
        heldObject.transform.rotation = rightHandAnchor.rotation;
    }

    void UpdateHeldObjectScale()
    {
        if (heldObject == null) return;

        Vector3 handDelta = rightHandAnchor.position - previousHandPosition;
        float scaleChange = Vector3.Dot(handDelta, centerEyeAnchor.forward) * scaleSpeed;

        Vector3 newScale = heldObject.transform.localScale + Vector3.one * scaleChange;

        float x = Mathf.Clamp(newScale.x, minScale, maxScale);
        float y = Mathf.Clamp(newScale.y, minScale, maxScale);
        float z = Mathf.Clamp(newScale.z, minScale, maxScale);

        heldObject.transform.localScale = new Vector3(x, y, z);
    }

    void ReleaseHeldObject()
    {
        if (heldObject == null) return;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("Released object: " + heldObject.name);
        heldObject = null;
    }
}