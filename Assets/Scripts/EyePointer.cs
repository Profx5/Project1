using UnityEngine;

public class EyePointer : MonoBehaviour
{
    public Transform beam;
    public Transform pointerDot;

    public float maxDistance = 10f;
    public Vector3 rayOffset = new Vector3(0.05f, -0.02f, 0f);
    public float triggerPressThreshold = 0.75f;
    [SerializeField] float hoverTime = 1f;
    private float currHoverTime;

    public ObjectHolder holder;
    public bool selectable = false;

    private SelectableObject currentHover;
    private bool wasRightTriggerPressed = false;

    void Update()
    {
        OVRInput.Update();
        float triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        bool rightTriggerPressed = triggerValue > triggerPressThreshold;
        bool rightTriggerDown = rightTriggerPressed && !wasRightTriggerPressed;

        if (holder.HasHeldObject() && selectable)
        {
            ClearHover();

            pointerDot.gameObject.SetActive(false);
            beam.gameObject.SetActive(false);

            wasRightTriggerPressed = rightTriggerPressed;
            return;
        }

        beam.gameObject.SetActive(true);

        Vector3 origin = transform.position + transform.right * rayOffset.x + transform.up * rayOffset.y + transform.forward * rayOffset.z;

        Ray ray = new Ray(origin, transform.forward);
        RaycastHit hit;

        float distance = maxDistance;
        SelectableObject newHover = null;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            distance = hit.distance;

            if (pointerDot != null)
            {
                pointerDot.position = hit.point + hit.normal * 0.01f;
                pointerDot.gameObject.SetActive(true);
            }

            SpawnMenuButton button = hit.collider.GetComponentInParent<SpawnMenuButton>();
            newHover = hit.collider.GetComponentInParent<SelectableObject>();

            if (currentHover == newHover)
            {
                currHoverTime += Time.deltaTime;
            } else
            {
                currHoverTime = 0;
            }

            if (currHoverTime >= hoverTime && selectable)
            {
                currHoverTime = 0;
                if (button != null)
                {
                    button.Activate();
                }
                else if (newHover != null && holder != null)
                {
                    holder.HoldObject(newHover.gameObject);
                }
            }
        }
        else
        {
            if (pointerDot != null)
            {
                pointerDot.gameObject.SetActive(false);
            }
        }

        if(selectable)
        {
            UpdateHighlight(newHover);
        }
        UpdateBeam(origin, distance);

        wasRightTriggerPressed = rightTriggerPressed;
    }

    void UpdateHighlight(SelectableObject newHover)
    {
        if (newHover != currentHover)
        {
            if (currentHover != null)
            {
                currentHover.Unhighlight();
            }

            currentHover = newHover;

            if (currentHover != null)
            {
                currentHover.Highlight();
            }
        }
    }

    void ClearHover()
    {
        if (currentHover != null)
        {
            currentHover.Unhighlight();
            currentHover = null;
        }
    }

    void UpdateBeam(Vector3 origin, float distance)
    {
        if (beam == null)
        {
            return;
        }

        beam.position = origin + transform.forward * (distance / 2f);
        beam.rotation = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(90f, 0f, 0f);
        beam.localScale = new Vector3(0.02f, distance / 2f, 0.02f);
    }
}