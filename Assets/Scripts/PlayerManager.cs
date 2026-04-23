using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject handBeam;
    [SerializeField] GameObject eyeBeam;

    private GameObject currSelectionMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currSelectionMode = handBeam;
        handBeam.SetActive(true);
        eyeBeam.SetActive(false);
        switchSelectionMethod();
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            switchSelectionMethod();
        }


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
