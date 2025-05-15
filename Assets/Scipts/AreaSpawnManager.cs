using System.Collections;
using UnityEngine;

public class AreaSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnConfig
    {
        public GameObject enemyPrefab;  // ����Ԥ����
        [Range(0, 1)] public float spawnWeight = 1f; // ����Ȩ��
    }

    [Header("��������")]
    [SerializeField] private SpawnConfig[] enemyTypes; // ������������
    [SerializeField] private float totalSpawnTime = 60f; // ������ʱ��
    [SerializeField] private float spawnInterval = 2f;   // ���ɼ��
    [SerializeField] private float startDelay = 1f;      // ��ʼ�ӳ�

    [Header("��������")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20, 15); // ��������ߴ�
    [SerializeField] private Vector2 spawnAreaCenterOffset = Vector2.zero; // ������������ƫ��
    [SerializeField] private Vector2 safeAreaSize = new Vector2(5, 3);    // ��ȫ���ߴ�
    [SerializeField] private Vector2 safeAreaCenterOffset = Vector2.zero; // ��ȫ������ƫ��

    private Transform player;
    private float totalWeight;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        CalculateWeights();
        StartCoroutine(SpawnRoutine());
    }

    private void CalculateWeights()
    {
        totalWeight = 0;
        foreach (var config in enemyTypes)
        {
            totalWeight += config.spawnWeight;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        float timer = 0;
        while (timer < totalSpawnTime)
        {
            TrySpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
            timer += spawnInterval;
        }
    }

    private void TrySpawnEnemy()
    {
        Vector2 spawnPos = GetValidSpawnPosition();
        GameObject selectedPrefab = GetRandomEnemyType();

        if (selectedPrefab != null)
        {
            GameObject enemy = MonsterPool.Instance.GetEnemy(selectedPrefab, spawnPos);
            if (enemy != null)
            {
                enemy.transform.position = spawnPos;
                enemy.SetActive(true);
            }
        }
    }

    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnCenter = (Vector2)transform.position + spawnAreaCenterOffset;
        Vector2 safeCenter = (Vector2)transform.position + safeAreaCenterOffset;

        Vector2 pos;
        int attempts = 0;
        bool inSafeArea;
        bool nearPlayer;

        do
        {
            // �������������������λ��
            pos = spawnCenter + new Vector2(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
            );

            // ��鰲ȫ��
            Vector2 safeDelta = pos - safeCenter;
            inSafeArea = Mathf.Abs(safeDelta.x) < safeAreaSize.x / 2 &&
                        Mathf.Abs(safeDelta.y) < safeAreaSize.y / 2;

            // ��������Χ
            nearPlayer = player != null &&
                       Vector2.Distance(pos, player.position) < 2f;

            attempts++;
        }
        while ((inSafeArea || nearPlayer) && attempts < 10);

        return pos;
    }

    private GameObject GetRandomEnemyType()
    {
        float randomPoint = Random.Range(0, totalWeight);

        foreach (var config in enemyTypes)
        {
            if (randomPoint < config.spawnWeight)
            {
                return config.enemyPrefab;
            }
            randomPoint -= config.spawnWeight;
        }

        return enemyTypes[0].enemyPrefab;
    }

    private void OnDrawGizmos()
    {
        // ������������
        Vector3 spawnCenter = transform.position + (Vector3)spawnAreaCenterOffset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnCenter, spawnAreaSize);

        // ���ư�ȫ��
        Vector3 safeCenter = transform.position + (Vector3)safeAreaCenterOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(safeCenter, safeAreaSize);
    }
}