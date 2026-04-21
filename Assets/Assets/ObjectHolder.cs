using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;

    public float holdDistance = 2.5f;
    public float holdHeightOffset = -0.2f;
    public float triggerPressThreshold = 0.75f;

    private GameObject heldObject;
    private bool wasTriggerPressed = false;
    private bool waitingForTriggerRelease = false;

    void Update()
    {
        float triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        bool triggerPressed = triggerValue > triggerPressThreshold;
        bool triggerDown = triggerPressed && !wasTriggerPressed;

        if (heldObject != null)
        {
            heldObject.transform.position = GetHoldPosition();

            // Prevent same press from instantly releasing
            if (waitingForTriggerRelease)
            {
                if (!triggerPressed)
                {
                    waitingForTriggerRelease = false;
                }
            }
            else
            {
                // Press again to release
                if (triggerDown)
                {
                    ReleaseHeldObject();
                }
            }
        }

        wasTriggerPressed = triggerPressed;
    }

    public bool HasHeldObject()
    {
        return heldObject != null;
    }

    public void HoldObject(GameObject obj)
    {
        if (obj == null || heldObject != null)
        {
            return;
        }
     
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

        waitingForTriggerRelease = true;

        Debug.Log("Holding object: " + obj.name);
    }

    void ReleaseHeldObject()
    {
        if (heldObject == null)
        {
            return;
        }

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("Released object: " + heldObject.name);
        heldObject = null;
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
}