using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Spawner : MonoBehaviour
{
    public GameObject prefab1;
    public GameObject prefab2;

    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;
    public ObjectHolder holder;

    public float spawnDistance = 2.5f;
    public float spawnHeight = -0.2f;

    private GameObject obj;

    public void SpawnPrefab1()
    {
        Spawn(prefab1);
    }

    public void SpawnPrefab2()
    {
        Spawn(prefab2);
    }
    void Spawn(GameObject prefab)
    {
        if(prefab == null || holder.HasHeldObject())
        {
            return;
        }
        Vector3 forward = centerEyeAnchor.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 spawnPos = centerEyeAnchor.position + forward * spawnDistance;
        Quaternion spawnRot = Quaternion.identity;

        obj = Instantiate(prefab, spawnPos, spawnRot);
       
        holder.HoldObject(obj);
    }
}
