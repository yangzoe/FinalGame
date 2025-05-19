using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    // 单例访问点
    public static MonsterPool Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;    // 敌人预制体
        public int preloadCount = 4; // 预加载数量
        public int maxCount = 8;     // 最大数量
    }

    [Header("敌人配置")]
    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();

    [Header("状态监控")]
    [Tooltip("当前场景中活跃的怪物数量")]
    public int activeMonstersCount; // 新增的公共变量


    // 对象池存储结构：预制体 -> 对象队列
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 步骤1：初始化所有对象池
    private void InitializePools()
    {
        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (PoolConfig config in poolConfigs)
        {
            // 为每个预制体创建独立队列
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            // 预生成对象
            for (int i = 0; i < config.preloadCount; i++)
            {
                GameObject obj = CreateNewObject(config.prefab);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(config.prefab, objectQueue);
        }
    }

    // 步骤2：创建新对象
    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 保持层级整洁
        return obj;
    }

    // 步骤3：从池中获取对象
    public GameObject GetEnemy(GameObject enemyType, Vector2 position)
    {
        // 检查类型是否存在
        if (!poolDictionary.ContainsKey(enemyType))
        {
            Debug.LogError($"未配置 {enemyType.name} 的对象池");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[enemyType];
        PoolConfig config = GetConfig(enemyType);

        // 情况1：池中有可用对象
        if (queue.Count > 0 && !queue.Peek().activeInHierarchy)
        {
            GameObject obj = queue.Dequeue();
            PrepareObject(obj, position);
            queue.Enqueue(obj);
            return obj;
        }

        // 情况2：允许创建新对象
        if (queue.Count < config.maxCount)
        {
            GameObject newObj = CreateNewObject(enemyType);
            PrepareObject(newObj, position);
            queue.Enqueue(newObj);
            return newObj;
        }

        Debug.LogWarning($"{enemyType.name} 已达最大数量限制");
        return null;
    }

    // 步骤4：准备对象
    private void PrepareObject(GameObject obj, Vector2 position)
    {
        obj.transform.position = position;
        obj.SetActive(true);
        activeMonstersCount++;

        // 调用敌人的初始化方法
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null) enemy.OnSpawn();
    }

    // 步骤5：回收对象
    public void ReturnEnemy(GameObject enemy)
    {
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent == null) return;

        GameObject prefabType = enemyComponent.prefabType;

        // 重置状态
        enemyComponent.OnDespawn();
        activeMonstersCount--;
        enemy.SetActive(false);

        // 重新入队
        if (poolDictionary.ContainsKey(prefabType))
        {
            poolDictionary[prefabType].Enqueue(enemy);
        }
    }

    // 辅助方法：获取配置
    private PoolConfig GetConfig(GameObject enemyType)
    {
        foreach (PoolConfig config in poolConfigs)
        {
            if (config.prefab == enemyType) return config;
        }
        return null;
    }
}