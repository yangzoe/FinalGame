using System.Collections;
using UnityEngine;

public class AreaSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnConfig
    {
        public GameObject enemyPrefab;  // 敌人预制体
        [Range(0, 1)] public float spawnWeight = 1f; // 生成权重
    }

    [Header("生成设置")]
    [SerializeField] private SpawnConfig[] enemyTypes; // 敌人类型配置
    [SerializeField] private float totalSpawnTime = 60f; // 总生成时间
    [SerializeField] private float spawnInterval = 2f;   // 生成间隔
    [SerializeField] private float startDelay = 1f;      // 开始延迟

    [Header("生成区域")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20, 15); // 生成区域尺寸
    [SerializeField] private Vector2 spawnAreaCenterOffset = Vector2.zero; // 生成区域中心偏移
    [SerializeField] private Vector2 safeAreaSize = new Vector2(5, 3);    // 安全区尺寸
    [SerializeField] private Vector2 safeAreaCenterOffset = Vector2.zero; // 安全区中心偏移

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
            // 在生成区域内随机生成位置
            pos = spawnCenter + new Vector2(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
            );

            // 检查安全区
            Vector2 safeDelta = pos - safeCenter;
            inSafeArea = Mathf.Abs(safeDelta.x) < safeAreaSize.x / 2 &&
                        Mathf.Abs(safeDelta.y) < safeAreaSize.y / 2;

            // 检查玩家周围
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
        // 绘制生成区域
        Vector3 spawnCenter = transform.position + (Vector3)spawnAreaCenterOffset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnCenter, spawnAreaSize);

        // 绘制安全区
        Vector3 safeCenter = transform.position + (Vector3)safeAreaCenterOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(safeCenter, safeAreaSize);
    }
}