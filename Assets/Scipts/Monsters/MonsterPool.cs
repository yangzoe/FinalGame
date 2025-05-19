using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    // �������ʵ�
    public static MonsterPool Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;    // ����Ԥ����
        public int preloadCount = 4; // Ԥ��������
        public int maxCount = 8;     // �������
    }

    [Header("��������")]
    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();

    [Header("״̬���")]
    [Tooltip("��ǰ�����л�Ծ�Ĺ�������")]
    public int activeMonstersCount; // �����Ĺ�������


    // ����ش洢�ṹ��Ԥ���� -> �������
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // ������ʼ��
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

    // ����1����ʼ�����ж����
    private void InitializePools()
    {
        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (PoolConfig config in poolConfigs)
        {
            // Ϊÿ��Ԥ���崴����������
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            // Ԥ���ɶ���
            for (int i = 0; i < config.preloadCount; i++)
            {
                GameObject obj = CreateNewObject(config.prefab);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(config.prefab, objectQueue);
        }
    }

    // ����2�������¶���
    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        obj.transform.SetParent(transform); // ���ֲ㼶����
        return obj;
    }

    // ����3���ӳ��л�ȡ����
    public GameObject GetEnemy(GameObject enemyType, Vector2 position)
    {
        // ��������Ƿ����
        if (!poolDictionary.ContainsKey(enemyType))
        {
            Debug.LogError($"δ���� {enemyType.name} �Ķ����");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[enemyType];
        PoolConfig config = GetConfig(enemyType);

        // ���1�������п��ö���
        if (queue.Count > 0 && !queue.Peek().activeInHierarchy)
        {
            GameObject obj = queue.Dequeue();
            PrepareObject(obj, position);
            queue.Enqueue(obj);
            return obj;
        }

        // ���2���������¶���
        if (queue.Count < config.maxCount)
        {
            GameObject newObj = CreateNewObject(enemyType);
            PrepareObject(newObj, position);
            queue.Enqueue(newObj);
            return newObj;
        }

        Debug.LogWarning($"{enemyType.name} �Ѵ������������");
        return null;
    }

    // ����4��׼������
    private void PrepareObject(GameObject obj, Vector2 position)
    {
        obj.transform.position = position;
        obj.SetActive(true);
        activeMonstersCount++;

        // ���õ��˵ĳ�ʼ������
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null) enemy.OnSpawn();
    }

    // ����5�����ն���
    public void ReturnEnemy(GameObject enemy)
    {
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent == null) return;

        GameObject prefabType = enemyComponent.prefabType;

        // ����״̬
        enemyComponent.OnDespawn();
        activeMonstersCount--;
        enemy.SetActive(false);

        // �������
        if (poolDictionary.ContainsKey(prefabType))
        {
            poolDictionary[prefabType].Enqueue(enemy);
        }
    }

    // ������������ȡ����
    private PoolConfig GetConfig(GameObject enemyType)
    {
        foreach (PoolConfig config in poolConfigs)
        {
            if (config.prefab == enemyType) return config;
        }
        return null;
    }
}