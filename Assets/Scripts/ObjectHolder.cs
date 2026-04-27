using Unity.VisualScripting;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    public static ObjectHolder Instance { get; private set; }

    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;
    public Transform leftHandAnchor;
    public Transform movementAnchor;

    [Header("General")]
    public float triggerPressThreshold = 0.75f;
    public float holdToReleaseTime = 1.0f;

    [Header("Rotate Mode")]
    public float rotateDeadzone = 50f;
    public float maxRotateTilt = 55f;
    public float rotationSpeedX = 100f;
    public float rotationSpeedY = 120f;
    public float rotationSpeedZ = 120f;

    [Header("Scale Mode")]
    public float maxScale = 3.0f;
    public float minScale = 0.5f;
    public float expandDistanceThreshold = 5.0f;
    public float shrinkDistanceThreshold = 0.5f;

    public GameObject selectionManager;


    private GameObject heldObject;
    private SelectableObject currentSelectable;

    private bool wasTriggerPressed = false;
    private bool suppressNextTap = false;
    private float triggerHoldTime = 0f;
    private bool releasedByHoldThisPress = false;

    private GameObject currentRotateGizmo;
    private GameObject currentMoveGizmo;
    private GameObject currentScaleGizmo;

    private float itemDistance;

    private enum ManipulationMode
    {
        Move,
        Rotate,
        Scale
    }

    private ManipulationMode currentMode = ManipulationMode.Move;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        } else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void CycleMode()
    {
        switch (currentMode)
        {
            case ManipulationMode.Move:
                currentMode = ManipulationMode.Rotate;
                break;

            case ManipulationMode.Rotate:
                currentMode = ManipulationMode.Scale;
                break;

            case ManipulationMode.Scale:
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
                        selectionManager.GetComponent<PlayerManager>().enabled = false;
                        Ray ray = new Ray(movementAnchor.position, movementAnchor.forward);
                        heldObject.transform.position = ray.GetPoint(itemDistance);
                        SetGizmoVisibility(currentMoveGizmo, true);
                        SetGizmoVisibility(currentRotateGizmo, false);
                        SetGizmoVisibility(currentScaleGizmo, false);

                        break;

                    case ManipulationMode.Rotate:
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
                        SetGizmoVisibility(currentScaleGizmo, false);
                        break;

                    case ManipulationMode.Scale:
                        if(gripHeld)
                        {
                            UpdateHeldObjectScale();
                        }
                     
                        SetGizmoVisibility(currentMoveGizmo, false);
                        SetGizmoVisibility(currentRotateGizmo, false);
                        SetGizmoVisibility(currentScaleGizmo, true);
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

        Transform rotate = heldObject.transform.Find("RotateGizmo");
        currentRotateGizmo = rotate.gameObject;

        Transform move = heldObject.transform.Find("MoveGizmo");
        currentMoveGizmo = move.gameObject;

        Transform scale = heldObject.transform.Find("ScaleGizmo");
        currentScaleGizmo = scale.gameObject;

        SetGizmoVisibility(currentMoveGizmo, true);
        SetGizmoVisibility(currentRotateGizmo, false);
        SetGizmoVisibility(currentScaleGizmo, false);

        suppressNextTap = true;
        triggerHoldTime = 0f;
        releasedByHoldThisPress = false;

        if (currentSelectable != null)
        {
            currentSelectable.Unhighlight();
        }

        Debug.Log("Holding object: " + obj.name + " | Mode: " + currentMode);
    }

    void ReleaseHeldObject()
    {
        if (heldObject == null)
        {
            return;
        }
        selectionManager.GetComponent<PlayerManager>().enabled = true;
        if (currentSelectable != null)
        {
            currentSelectable.Unhighlight();
        }

        SetGizmoVisibility(currentRotateGizmo, false);
        SetGizmoVisibility(currentMoveGizmo, false);
        SetGizmoVisibility(currentScaleGizmo, false);   

        currentRotateGizmo = null;
        currentMoveGizmo = null;
        currentScaleGizmo = null;

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

    void UpdateHeldObjectScale()
    {
        if(heldObject == null || rightHandAnchor == null)
        {
            return;
        }

        Vector3 rightHandPos = rightHandAnchor.transform.position;
        Vector3 leftHandPos = leftHandAnchor.transform.position;

        Debug.Log(rightHandPos);
        Debug.Log(leftHandPos);

        float distance = Mathf.Abs(rightHandPos.x - leftHandPos.x);
        if(distance >= expandDistanceThreshold)
        {
            Debug.Log("Expanding");
            if(heldObject.transform.localScale.x <= maxScale)
            {
                heldObject.transform.localScale += new Vector3(1, 1, 1) * Time.deltaTime;
            }
        }
        else if(distance <= shrinkDistanceThreshold)
        {
            Debug.Log("Shrinking");
            if(heldObject.transform.localScale.x >= minScale)
            {
                heldObject.transform.localScale -= new Vector3(1, 1, 1) * Time.deltaTime;
            }
        }
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
