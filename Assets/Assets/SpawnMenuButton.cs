using UnityEngine;

public class SpawnMenuButton : MonoBehaviour
{
    public enum SpawnType
    {
        Prefab1,
        Prefab2
    }

    public Spawner spawner;
    public SpawnType spawnPrefab;

    public void Activate()
    {
        switch (spawnPrefab)
        {
            case SpawnType.Prefab1:
                spawner.SpawnPrefab1();
                break;

            case SpawnType.Prefab2:
                spawner.SpawnPrefab2();
                break;
        }
    }
}