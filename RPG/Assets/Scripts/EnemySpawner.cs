using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviourPun
{
    public string enemyMeleePrefabPath;
    public string enemyRangePrefabPath;
    public float maxMeleeEnemies;
    public float maxRangeEnemies;
    public float spawnRadius;
    public float spawnCheckTime;
    private float lastSpawnCheckTime;
    private List<GameObject> curMeleeEnemies = new List<GameObject>();
    private List<GameObject> curRangeEnemies = new List<GameObject>();

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (Time.time - lastSpawnCheckTime > spawnCheckTime)
        {
            lastSpawnCheckTime = Time.time;
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        // remove any dead enemies from the curEnemies list
        for (int x = 0; x < curMeleeEnemies.Count; ++x)
        {
            if (!curMeleeEnemies[x])
                curMeleeEnemies.RemoveAt(x);
        }
        for (int x = 0; x < curRangeEnemies.Count; ++x)
        {
            if (!curRangeEnemies[x])
                curRangeEnemies.RemoveAt(x);
        }
        // if we have maxed out our enemies, return
        if ((curRangeEnemies.Count + curMeleeEnemies.Count) >= (maxMeleeEnemies + maxRangeEnemies))
            return;
        // otherwise, spawn an enemy
        Vector3 randomInCircle = Random.insideUnitCircle * spawnRadius;
        bool spawnMelee = curMeleeEnemies.Count <= curRangeEnemies.Count;
        if (spawnMelee && (curMeleeEnemies.Count < maxMeleeEnemies))
        {
            GameObject enemy = PhotonNetwork.Instantiate(enemyMeleePrefabPath, transform.position + randomInCircle, Quaternion.identity);
            curMeleeEnemies.Add(enemy);
        }
        else if (curRangeEnemies.Count < maxRangeEnemies)
        {
            GameObject enemy = PhotonNetwork.Instantiate(enemyRangePrefabPath, transform.position + randomInCircle, Quaternion.identity);
            curRangeEnemies.Add(enemy);
        }
    }
}
