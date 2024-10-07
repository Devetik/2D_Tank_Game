using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoint = new List<SpawnPoint>();

    private void OnEnable()
    {
        spawnPoint.Add(this);
    }

    private void OnDisable()
    {
        spawnPoint.Remove(this);
    }

    public static Vector3 GetRandomSpawnPoint()
    {
        if(spawnPoint.Count == 0)
        {
            return Vector3.zero;
        }

        return spawnPoint[Random.Range(0, spawnPoint.Count)].transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
