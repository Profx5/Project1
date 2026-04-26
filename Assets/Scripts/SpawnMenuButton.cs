using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using TMPro;
using UnityEngine;
using Unity.UI;

public class SpawnMenuButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buttonLabel;
    private string buttonPath;
    private SpawningMenu spawnMenu;
    private bool hasItem = false; // false if the button has a folder path, true if the button has an item path
    private string fileExtension = ".prefab";


    private void Start()
    {
        spawnMenu = gameObject.GetComponentInParent<SpawningMenu>();
    }

    public void setButton(string label, string path)
    {
        string temp = label;
        string temp2 = path;
        //Debug.Log("Button path: " + path);
        if (getExtension(path) == fileExtension)
        {
            temp = "Spawn " + label.Substring(0, label.Length - fileExtension.Length);
            temp2 = path.Substring(0, path.Length - fileExtension.Length);
            hasItem = true;
        }
        buttonLabel.text = temp;
        buttonPath = temp2;
    }

    public void Activate()
    {
        if (buttonPath == null)
        {
            return;
        }
        if (!hasItem)
        {
            spawnMenu.fillButtonsFromFile(buttonPath);
        } else
        {
            spawnMenu.spawnItem(buttonPath);
        }
    }

    public string getExtension(string path)
    {
        string toReturn = path.Substring(path.Length - fileExtension.Length);
        return toReturn;
    }

}