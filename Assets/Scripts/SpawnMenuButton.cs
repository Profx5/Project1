using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using TMPro;
using UnityEngine;
using Unity.UI;

public class SpawnMenuButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buttonLabel;
    private string buttonPath;
    private SpawningMenu spawnMenu;

    private void Start()
    {
        spawnMenu = gameObject.GetComponentInParent<SpawningMenu>();
    }

    public void setButton(string label, string path)
    {
        buttonLabel.text = label;
        buttonPath = path;
    }

    public void Activate()
    {
        if (buttonPath == null)
        {
            return;
        }
        Debug.Log("Button Path: " + buttonPath);
        string extension = buttonPath.Substring(buttonPath.Length - 4);
        Debug.Log("Extension: " + extension);
        if (extension != ".fbx")
        {
            spawnMenu.fillButtonsFromFile(buttonPath);
        } else
        {
            spawnMenu.spawnItem(buttonPath);
        }
    }

}