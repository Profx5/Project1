using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpawningMenu : MonoBehaviour
{
    [SerializeField] SpawnMenuButton[] buttons;
    [SerializeField] SpawnMenuButton backButton;
    private string assetPath;
    private int buttonCount;
    [SerializeField] GameObject spawnLocation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        assetPath = Application.dataPath + "/Resources/SpawnableItems";
        buttonCount = buttons.Length;
        fillButtonsFromFile(assetPath);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void fillButtonsHelper(string[] buttonLabels, string[] filePaths)
    {
        if (buttonLabels.Length != filePaths.Length)
        {
            throw new System.Exception("The buttons do not all have file paths");
        }

        foreach (SpawnMenuButton button in buttons) {
            button.gameObject.SetActive(false);
        }

        for (int i = 0; i < buttonLabels.Length && i < buttonCount; i++) {
            SpawnMenuButton button = buttons[i];
            button.setButton(buttonLabels[i], filePaths[i]);
            button.gameObject.SetActive(true);
        }

    }

    public void fillButtonsFromFile(string path)
    {
        Debug.Log("Filling buttons from: " + path);
        string[] fileInfo1 = Directory.GetDirectories(path);
        string[] fileInfo2 = Directory.GetFiles(path, "*.fbx");
        string[] fileInfo = fileInfo1.Concat(fileInfo2).ToArray();
        string[] fileNames = new string[fileInfo.Length];

        for (int i = 0; i < fileInfo.Length; i++)
        {
            string file = fileInfo[i];
            Debug.Log("FileInfo: " + file);
            string fileName = file.Substring(path.Length + 1);
            Debug.Log("FileName: " + fileName);
            fileNames[i] = fileName;
        }

        if (path != assetPath)
        {
            backButton.setButton("Back", Directory.GetParent(path).FullName);
        }
        fillButtonsHelper(fileNames, fileInfo);
    }

    public void spawnItem(string path)
    {
        object thing = Resources.Load(path);
        Debug.Log(thing);
    }
}
