using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnNomove : MonoBehaviour
{
    [Header("��������")]
    public GameObject spawnPrefab;       // ��Ҫ���ɵ�Ԥ����
    public Transform[] spawnPoints;     // 10�����ɵ㣨����Inspector�����ã�
    public float initialDelay = 3f;      // ��ʼ����ǰ�ĳ�ʼ�ӳ�
    public int maxSpawnCount = 5;       // ���ͬʱ��������

    private List<GameObject> activeObjects = new List<GameObject>(); // ��ǰ���ڵĶ���
    private bool isSpawning = true;     // ���ɿ��ƿ���

    private void Start()
    {
        // ��֤���ɵ�����
        if (spawnPoints.Length != 5)
        {
            Debug.LogWarning("��Ҫ����5�����ɵ㣡��ǰ������" + spawnPoints.Length);
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(initialDelay); // ��ʼ�ӳ�

        while (isSpawning)
        {
            if (activeObjects.Count < maxSpawnCount)
            {
                SpawnObject();
            }

            // ��������5-10�룩
            float waitTime = Random.Range(5f, 10f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnObject()
    {
        if (spawnPrefab == null || spawnPoints.Length == 0) return;

        // ���ѡ�����ɵ�
        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        // ʵ��������
        if (spawnPoint != null)
        {
            if(spawnPoint.transform.position.x < 0)
            {
                spawnPoint.transform.eulerAngles = new Vector3(spawnPoint.transform.eulerAngles.x,180,spawnPoint.transform.eulerAngles.z);
            }
        }
        GameObject newObj = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
        
        activeObjects.Add(newObj);

        // ������ټ���
        StartCoroutine(TrackObjectLifecycle(newObj));
    }

    private IEnumerator TrackObjectLifecycle(GameObject obj)
    {
        // �ȴ���������
        while (obj != null)
        {
            yield return null;
        }

        // ���б����Ƴ�
        activeObjects.Remove(obj);
    }

    // ֹͣ���ɣ���ѡ��
    public void StopSpawning()
    {
        isSpawning = false;
    }
}