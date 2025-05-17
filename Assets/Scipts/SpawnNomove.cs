using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SpawnNomove : MonoBehaviour
{
    [Header("��������")]
    public GameObject spawnPrefab;       // ��Ҫ���ɵ�Ԥ����
    public Transform[] spawnPoints;     // ���ɵ����飨5����
    public Transform[] leftStartPoints;  // �����ʼ��
    public Transform[] rightStartPoints; // �Ҳ���ʼ��
    public float initialDelay = 3f;      // ��ʼ�ӳ�
    public int maxSpawnCount = 3;       // ���ͬʱ��������
    public float moveSpeed = 0.5f;        // �ƶ��ٶ�

    private HashSet<int> usedSpawnIndices = new HashSet<int>(); // �ѱ�ռ�õ����ɵ�����
    private Dictionary<GameObject, int> objToSpawnIndex = new Dictionary<GameObject, int>(); // �����Ӧ�����ɵ�����
    private bool isSpawning = true;

    private void Start()
    {
        ValidatePoints();
        StartCoroutine(SpawnRoutine());
    }

    private void ValidatePoints()
    {
        if (spawnPoints.Length != 5)
            Debug.LogWarning("��Ҫ5�����ɵ㣡��ǰ������" + spawnPoints.Length);

        if (leftStartPoints.Length == 0 || rightStartPoints.Length == 0)
            Debug.LogWarning("��Ҫ����һ�������Ҳ���ʼ�㣡");
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
        // ��ȡ�������ɵ�����
        var availableIndices = Enumerable.Range(0, spawnPoints.Length)
            .Where(i => !usedSpawnIndices.Contains(i)).ToList();

        if (availableIndices.Count == 0) return;

        int spawnIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        Transform spawnPoint = spawnPoints[spawnIndex];

        // ѡ����ʼ��
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
        
        // ��������ʱ����
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

    // �������ٵĶ���
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