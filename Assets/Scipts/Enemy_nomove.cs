using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class Enemy_nomove : Enemy
{
    [Header("发射设置")]
    public GameObject projectilePrefab;
    private GameObject targetObject;
    private Transform target;
    [Tooltip("发射间隔（秒）")]
    public float launchInterval = 2f;
    [Tooltip("预计到达目标的时间（秒）")]
    // 修改点：新增命中延迟配置项
    
    [Header("命中设置")]
    public float hitDestroyDelay = 0.5f;
    public float flightTime = 1f;

    [Header("发射点配置")]
    [SerializeField] private Transform launchPoint;
    [Tooltip("世界坐标系下的偏移量")]
    public Vector2 spawnOffset = new Vector2(0.5f, 0);

    public override void OnSpawn()
    {
        if (launchPoint == null) launchPoint = transform;
        StartCoroutine(LaunchRoutine());
    }

    private void Start()
    {
        // 修改点1：使用更可靠的查找方式
        targetObject = GameObject.FindWithTag("Player"); // 要求玩家对象有"Player"标签
        if (targetObject == null)
        {
            Debug.LogError("未找到玩家对象！请确保场景中存在带有Player标签的对象");
            return;
        }

        // 修改点2：确保执行顺序
        if (launchPoint == null) launchPoint = transform;
        OnSpawn();
    }


    private IEnumerator LaunchRoutine()
    {
        // 修改点3：添加空对象检查
        while (targetObject != null)
        {
            target = targetObject.transform;
            yield return new WaitForSeconds(launchInterval);

            if (target != null)
            {
                LaunchProjectile(target.position);
            }
            else
            {
                Debug.LogWarning("目标已丢失，尝试重新查找...");
                targetObject = GameObject.FindWithTag("Player");
            }
        }
        Debug.LogError("玩家对象已销毁，停止发射");
    }

    private void LaunchProjectile(Vector2 targetPosition)
    {
        Vector2 spawnPos = (Vector2)launchPoint.position + spawnOffset;

        // 修改点1：实例化后立即激活对象
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.SetActive(true); // 先激活对象

        // 配置物理组件
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>() ?? projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.velocity = CalculateLaunchVelocity(spawnPos, targetPosition);

        // 修改点2：最后添加生命周期组件
        ProjectileLifecycle lifecycle = projectile.AddComponent<ProjectileLifecycle>();
        lifecycle.hitDestroyDelay = hitDestroyDelay; // 传递延迟参数
        lifecycle.Initialize(flightTime); // 修改点：使用原始飞行时间
    }

    private Vector2 CalculateLaunchVelocity(Vector2 startPos, Vector2 endPos)
    {
        Vector2 displacement = endPos - startPos;
        float horizontalVelocity = displacement.x / flightTime;
        float verticalVelocity = (displacement.y - 0.5f * Physics2D.gravity.y * flightTime * flightTime) / flightTime;
        return new Vector2(horizontalVelocity, verticalVelocity);
    }
}

// 修改点：新增命中延迟参数
public class ProjectileLifecycle : MonoBehaviour
{
    private float destroyDelay;
    private bool isDestroying;
    private Coroutine destructionCoroutine;
    public float hitDestroyDelay = 0.2f; // 新增命中后销毁延迟

    public void Initialize(float lifetime)
    {
        destroyDelay = lifetime;
        destructionCoroutine = StartCoroutine(DestructionCountdown(destroyDelay));
    }

    private IEnumerator DestructionCountdown(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isDestroying) DestroyProjectile();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collide!");

        if (collision.gameObject.CompareTag("Player") && !isDestroying)
        {
            // 修改点：停止原有时序改为延迟销毁
            if (destructionCoroutine != null)
            {
                StopCoroutine(destructionCoroutine);
            }
            destructionCoroutine = StartCoroutine(DestructionCountdown(hitDestroyDelay));
        }
    }

    private void DestroyProjectile()
    {
        if (isDestroying) return;
        isDestroying = true;
        Destroy(gameObject);
    }
}