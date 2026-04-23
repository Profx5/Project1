using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject handBeam;
    [SerializeField] GameObject eyeBeam;

    public float triggerPressThreshold = 0.75f;
    private bool wasRightGripPressed = false;

    private GameObject currSelectionMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currSelectionMode = handBeam;
        handBeam.SetActive(true);
        eyeBeam.SetActive(false);
        //switchSelectionMethod();
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        float triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        bool rightGripPressed = triggerValue > triggerPressThreshold;
        bool rightGripDown = rightGripPressed && !wasRightGripPressed;


        if (rightGripDown)
        {
            switchSelectionMethod();
        }

        wasRightGripPressed = rightGripPressed;
    }

    public void switchSelectionMethod()
    {
        currSelectionMode.SetActive(false);
        if (currSelectionMode == handBeam)
        {
            currSelectionMode = eyeBeam;
            Debug.Log("EyeBeam activated");
        } else
        {
            currSelectionMode = handBeam;
            Debug.Log("HandBeam activated");
        }
        currSelectionMode.SetActive(true);
    }
}
