using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnNomove : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject spawnPrefab;       // 需要生成的预制体
    public Transform[] spawnPoints;     // 10个生成点（需在Inspector中设置）
    public float initialDelay = 3f;      // 开始生成前的初始延迟
    public int maxSpawnCount = 5;       // 最大同时存在数量

    private List<GameObject> activeObjects = new List<GameObject>(); // 当前存在的对象
    private bool isSpawning = true;     // 生成控制开关

    private void Start()
    {
        // 验证生成点数量
        if (spawnPoints.Length != 5)
        {
            Debug.LogWarning("需要设置5个生成点！当前数量：" + spawnPoints.Length);
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(initialDelay); // 初始延迟

        while (isSpawning)
        {
            if (activeObjects.Count < maxSpawnCount)
            {
                SpawnObject();
            }

            // 随机间隔（5-10秒）
            float waitTime = Random.Range(5f, 10f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnObject()
    {
        if (spawnPrefab == null || spawnPoints.Length == 0) return;

        // 随机选择生成点
        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        // 实例化对象
        if (spawnPoint != null)
        {
            if(spawnPoint.transform.position.x < 0)
            {
                spawnPoint.transform.eulerAngles = new Vector3(spawnPoint.transform.eulerAngles.x,180,spawnPoint.transform.eulerAngles.z);
            }
        }
        GameObject newObj = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
        
        activeObjects.Add(newObj);

        // 添加销毁监听
        StartCoroutine(TrackObjectLifecycle(newObj));
    }

    private IEnumerator TrackObjectLifecycle(GameObject obj)
    {
        // 等待对象被销毁
        while (obj != null)
        {
            yield return null;
        }

        // 从列表中移除
        activeObjects.Remove(obj);
    }

    // 停止生成（可选）
    public void StopSpawning()
    {
        isSpawning = false;
    }
}