using Unity.VisualScripting;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;

    [Header("General")]
    public float triggerPressThreshold = 0.75f;
    public float holdToReleaseTime = 1.0f;

    [Header("Move Mode")]
    public float moveDeadzone = 50f;
    public float maxMoveTilt = 50f;
    public float moveSpeed = 2.0f;
    public float verticalMoveSpeed = 2.0f;
    public float holdHeightOffset = -0.2f;
    public float initialHoldDistance = 2.5f;
    public float minX = -3.0f;
    public float maxX = 3.0f;
    public float minY = -1.0f;
    public float maxY = 2.0f;
    public float minZ = 0.75f;
    public float maxZ = 5.0f;

    [Header("Rotate Mode")]
    public float rotateDeadzone = 50f;
    public float maxRotateTilt = 55f;
    public float rotationSpeedX = 100f;
    public float rotationSpeedY = 120f;
    public float rotationSpeedZ = 120f;

    private GameObject heldObject;
    private SelectableObject currentSelectable;

    private bool wasTriggerPressed = false;
    private bool suppressNextTap = false;
    private float triggerHoldTime = 0f;
    private bool releasedByHoldThisPress = false;

    private GameObject currentRotateGizmo;
    private GameObject currentMoveGizmo;

    private Vector3 holdOffset;
    private float itemDistance;

    private enum ManipulationMode
    {
        Move,
        Rotate
    }

    private ManipulationMode currentMode = ManipulationMode.Move;

    void CycleMode()
    {
        switch (currentMode)
        {
            case ManipulationMode.Move:
                currentMode = ManipulationMode.Rotate;
                break;

            case ManipulationMode.Rotate:
                currentMode = ManipulationMode.Move;
                break;
        }

        Debug.Log("Mode changed to: " + currentMode);
    }

    void Update()
    {
        float triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        bool triggerPressed = triggerValue > triggerPressThreshold;
        bool triggerUp = !triggerPressed && wasTriggerPressed;

        bool gripHeld = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);

        if (heldObject != null)
        {
            if (triggerPressed)
            {
                triggerHoldTime += Time.deltaTime;

                if (currentSelectable != null)
                {
                    float progress = Mathf.Clamp01(triggerHoldTime / holdToReleaseTime);
                    currentSelectable.SetHighlightAmount(progress);
                }

                if (!releasedByHoldThisPress && triggerHoldTime >= holdToReleaseTime)
                {
                    ReleaseHeldObject();
                    releasedByHoldThisPress = true;
                    suppressNextTap = true;

                    wasTriggerPressed = triggerPressed;
                    return;
                }
            }

            if (triggerUp)
            {
                if (currentSelectable != null)
                {
                    currentSelectable.Unhighlight();
                }

                if (!suppressNextTap && !releasedByHoldThisPress && triggerHoldTime < holdToReleaseTime)
                {
                    CycleMode();
                }

                triggerHoldTime = 0f;
                suppressNextTap = false;
                releasedByHoldThisPress = false;
            }

            if (heldObject != null)
            {
                switch (currentMode)
                {
                    case ManipulationMode.Move:
                        //if (gripHeld)
                        //{
                        //    UpdateHeldObjectVerticalMovementByTilt();
                        //}
                        //else
                        //{
                        //    UpdateHeldObjectMovementByTilt2D();
                        //}

                        //UpdateHeldObjectPosition();
                        //


                        Ray ray = new Ray(rightHandAnchor.position, rightHandAnchor.forward);
                        heldObject.transform.position = ray.GetPoint(itemDistance);
                        SetGizmoVisibility(currentMoveGizmo, true);
                        SetGizmoVisibility(currentRotateGizmo, false);

                        break;

                    case ManipulationMode.Rotate:
                        //UpdateHeldObjectPosition();

                        if (gripHeld)
                        {
                            UpdateHeldObjectRotationZByTilt();
                        }
                        else
                        {
                            UpdateHeldObjectRotationXYByTilt();
                        }
                        SetGizmoVisibility(currentMoveGizmo, false);
                        SetGizmoVisibility(currentRotateGizmo, true);
                        break;
                }
            }
        }
        else
        {
            if (triggerUp)
            {
                triggerHoldTime = 0f;
                suppressNextTap = false;
                releasedByHoldThisPress = false;
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
        if (obj == null)
        {
            return;
        }
        if (heldObject != null)
        {
            return;
        }

        heldObject = obj;
        itemDistance = Vector3.Distance(rightHandAnchor.position, obj.transform.position);
        currentSelectable = heldObject.GetComponentInParent<SelectableObject>();

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        currentMode = ManipulationMode.Move;
        holdOffset = new Vector3(0f, holdHeightOffset, initialHoldDistance);

        Transform rotate = heldObject.transform.Find("RotateGizmo");
        currentRotateGizmo = rotate.gameObject;

        Transform move = heldObject.transform.Find("MoveGizmo");
        currentMoveGizmo = move.gameObject;

        SetGizmoVisibility(currentRotateGizmo, false);
        SetGizmoVisibility(currentMoveGizmo, true);

        suppressNextTap = true;
        triggerHoldTime = 0f;
        releasedByHoldThisPress = false;

        if (currentSelectable != null)
        {
            currentSelectable.Unhighlight();
        }

        UpdateHeldObjectPosition();

        Debug.Log("Holding object: " + obj.name + " | Mode: " + currentMode);
    }

    void ReleaseHeldObject()
    {
        if (heldObject == null)
        {
            return;
        }

        if (currentSelectable != null)
        {
            currentSelectable.Unhighlight();
        }

        SetGizmoVisibility(currentRotateGizmo, false);
        SetGizmoVisibility(currentMoveGizmo, false);

        currentRotateGizmo = null;
        currentMoveGizmo = null;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("Released object: " + heldObject.name);

        heldObject = null;
        currentSelectable = null;
    }

    void UpdateHeldObjectPosition()
    {
        if (heldObject == null || centerEyeAnchor == null)
        {
            return;
        }

        Vector3 forward = centerEyeAnchor.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = centerEyeAnchor.right;
        right.y = 0f;
        right.Normalize();

        Vector3 worldPos =
            centerEyeAnchor.position +
            right * holdOffset.x +
            Vector3.up * holdOffset.y +
            forward * holdOffset.z;

        heldObject.transform.position = worldPos;
    }

    void UpdateHeldObjectMovementByTilt2D()
    {
        if (rightHandAnchor == null)
        {
            return;
        }

        float pitch = NormalizeAngle(rightHandAnchor.localEulerAngles.x);
        float roll = NormalizeAngle(rightHandAnchor.localEulerAngles.z);

        float forwardInput = GetTiltInput(pitch, moveDeadzone, maxMoveTilt);
        float rightInput = GetTiltInput(roll, moveDeadzone, maxMoveTilt);

        holdOffset.z += forwardInput * moveSpeed * Time.deltaTime;
        holdOffset.x -= rightInput * moveSpeed * Time.deltaTime;

        holdOffset.x = Mathf.Clamp(holdOffset.x, minX, maxX);
        holdOffset.z = Mathf.Clamp(holdOffset.z, minZ, maxZ);
    }

    void UpdateHeldObjectVerticalMovementByTilt()
    {
        if (rightHandAnchor == null)
        {
            return;
        }

        float pitch = NormalizeAngle(rightHandAnchor.localEulerAngles.x);
        float verticalInput = GetTiltInput(pitch, moveDeadzone, maxMoveTilt);

        holdOffset.y -= verticalInput * verticalMoveSpeed * Time.deltaTime;
        holdOffset.y = Mathf.Clamp(holdOffset.y, minY, maxY);
    }

    void UpdateHeldObjectRotationXYByTilt()
    {
        if (heldObject == null || rightHandAnchor == null)
        {
            return;
        }

        float pitch = NormalizeAngle(rightHandAnchor.localEulerAngles.x);
        float roll = NormalizeAngle(rightHandAnchor.localEulerAngles.z);

        float rotateX = GetTiltInput(pitch, rotateDeadzone, maxRotateTilt);
        float rotateY = GetTiltInput(roll, rotateDeadzone, maxRotateTilt);

        heldObject.transform.Rotate(
            Vector3.right,
            rotateX * rotationSpeedX * Time.deltaTime,
            Space.World
        );

        heldObject.transform.Rotate(
            Vector3.up,
            rotateY * rotationSpeedY * Time.deltaTime,
            Space.World
        );
    }

    void UpdateHeldObjectRotationZByTilt()
    {
        if (heldObject == null || rightHandAnchor == null)
        {
            return;
        }

        float roll = NormalizeAngle(rightHandAnchor.localEulerAngles.z);
        float rotateZ = GetTiltInput(roll, rotateDeadzone, maxRotateTilt);

        heldObject.transform.Rotate(
            Vector3.forward,
            rotateZ * rotationSpeedZ * Time.deltaTime,
            Space.World
        );
    }

    void SetGizmoVisibility(GameObject gizmo, bool visible)
    {
        gizmo.SetActive(visible);
    }

    float GetTiltInput(float angle, float deadzone, float maxTilt)
    {
        float abs = Mathf.Abs(angle);

        if (abs <= deadzone)
            return 0f;

        float sign = Mathf.Sign(angle);
        float adjusted = (abs - deadzone) / (maxTilt - deadzone);

        return Mathf.Clamp(adjusted, 0f, 1f) * sign;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }
        return angle;
    }
}
