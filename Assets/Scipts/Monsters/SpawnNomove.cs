using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SpawnNomove : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject spawnPrefab;       // 需要生成的预制体
    public Transform[] spawnPoints;     // 生成点数组（5个）
    public Transform[] leftStartPoints;  // 左侧起始点
    public Transform[] rightStartPoints; // 右侧起始点
    public float initialDelay = 3f;      // 初始延迟
    public int maxSpawnCount = 3;       // 最大同时存在数量
    public float moveSpeed = 0.5f;        // 移动速度

    private HashSet<int> usedSpawnIndices = new HashSet<int>(); // 已被占用的生成点索引
    private Dictionary<GameObject, int> objToSpawnIndex = new Dictionary<GameObject, int>(); // 对象对应的生成点索引
    private bool isSpawning = true;

    private void Start()
    {
        ValidatePoints();
        StartCoroutine(SpawnRoutine());
    }

    private void ValidatePoints()
    {
        if (spawnPoints.Length != 5)
            Debug.LogWarning("需要5个生成点！当前数量：" + spawnPoints.Length);

        if (leftStartPoints.Length == 0 || rightStartPoints.Length == 0)
            Debug.LogWarning("需要至少一个左侧和右侧起始点！");
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (isSpawning)
        {
            if (objToSpawnIndex.Count < maxSpawnCount)
            {
                TrySpawnObject();
            }
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    private void TrySpawnObject()
    {
        // 获取可用生成点索引
        var availableIndices = Enumerable.Range(0, spawnPoints.Length)
            .Where(i => !usedSpawnIndices.Contains(i)).ToList();

        if (availableIndices.Count == 0) return;

        int spawnIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        Transform spawnPoint = spawnPoints[spawnIndex];

        // 选择起始点
        Transform[] startPoints = spawnPoint.position.x < 0 ? leftStartPoints : rightStartPoints;
        Transform startPoint = startPoints[Random.Range(0, startPoints.Length)];

        
        GameObject obj = Instantiate(spawnPrefab, startPoint.position, 
            startPoint.position.x < 0?  Quaternion.Euler(startPoint.rotation.eulerAngles + new Vector3(0, 180f, 0))
        : startPoint.rotation);
        StartCoroutine(MoveToTarget(obj, spawnPoint.position, spawnIndex));

        usedSpawnIndices.Add(spawnIndex);
        objToSpawnIndex.Add(obj, spawnIndex);
    }

    private IEnumerator MoveToTarget(GameObject obj, Vector3 target, int spawnIndex)
    {
        while (obj != null && Vector3.Distance(obj.transform.position, target) > 0.1f)
        {
            obj.transform.position = Vector3.MoveTowards(
                obj.transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        obj.GetComponent<Enemy_nomove>().animator.SetBool("Is_des", true);
        
        // 对象销毁时清理
        if (obj == null)
        {
            usedSpawnIndices.Remove(spawnIndex);
            objToSpawnIndex.Remove(obj);
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    // 清理销毁的对象
    private void Update()
    {
        var nullEntries = objToSpawnIndex.Where(pair => pair.Key == null).ToList();
        foreach (var entry in nullEntries)
        {
            usedSpawnIndices.Remove(entry.Value);
            objToSpawnIndex.Remove(entry.Key);
        }
    }
}