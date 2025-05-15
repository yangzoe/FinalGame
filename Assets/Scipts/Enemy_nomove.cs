using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class Enemy_nomove : Enemy
{
    [Header("��������")]
    public GameObject projectilePrefab;
    private GameObject targetObject;
    private Transform target;
    [Tooltip("���������룩")]
    public float launchInterval = 2f;
    [Tooltip("Ԥ�Ƶ���Ŀ���ʱ�䣨�룩")]
    // �޸ĵ㣺���������ӳ�������
    
    [Header("��������")]
    public float hitDestroyDelay = 0.5f;
    public float flightTime = 1f;

    [Header("���������")]
    [SerializeField] private Transform launchPoint;
    [Tooltip("��������ϵ�µ�ƫ����")]
    public Vector2 spawnOffset = new Vector2(0.5f, 0);

    public override void OnSpawn()
    {
        if (launchPoint == null) launchPoint = transform;
        StartCoroutine(LaunchRoutine());
    }

    private void Start()
    {
        // �޸ĵ�1��ʹ�ø��ɿ��Ĳ��ҷ�ʽ
        targetObject = GameObject.FindWithTag("Player"); // Ҫ����Ҷ�����"Player"��ǩ
        if (targetObject == null)
        {
            Debug.LogError("δ�ҵ���Ҷ�����ȷ�������д��ڴ���Player��ǩ�Ķ���");
            return;
        }

        // �޸ĵ�2��ȷ��ִ��˳��
        if (launchPoint == null) launchPoint = transform;
        OnSpawn();
    }


    private IEnumerator LaunchRoutine()
    {
        // �޸ĵ�3����ӿն�����
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
                Debug.LogWarning("Ŀ���Ѷ�ʧ���������²���...");
                targetObject = GameObject.FindWithTag("Player");
            }
        }
        Debug.LogError("��Ҷ��������٣�ֹͣ����");
    }

    private void LaunchProjectile(Vector2 targetPosition)
    {
        Vector2 spawnPos = (Vector2)launchPoint.position + spawnOffset;

        // �޸ĵ�1��ʵ�����������������
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.SetActive(true); // �ȼ������

        // �����������
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>() ?? projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.velocity = CalculateLaunchVelocity(spawnPos, targetPosition);

        // �޸ĵ�2�������������������
        ProjectileLifecycle lifecycle = projectile.AddComponent<ProjectileLifecycle>();
        lifecycle.hitDestroyDelay = hitDestroyDelay; // �����ӳٲ���
        lifecycle.Initialize(flightTime); // �޸ĵ㣺ʹ��ԭʼ����ʱ��
    }

    private Vector2 CalculateLaunchVelocity(Vector2 startPos, Vector2 endPos)
    {
        Vector2 displacement = endPos - startPos;
        float horizontalVelocity = displacement.x / flightTime;
        float verticalVelocity = (displacement.y - 0.5f * Physics2D.gravity.y * flightTime * flightTime) / flightTime;
        return new Vector2(horizontalVelocity, verticalVelocity);
    }
}

// �޸ĵ㣺���������ӳٲ���
public class ProjectileLifecycle : MonoBehaviour
{
    private float destroyDelay;
    private bool isDestroying;
    private Coroutine destructionCoroutine;
    public float hitDestroyDelay = 0.2f; // �������к������ӳ�

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
            // �޸ĵ㣺ֹͣԭ��ʱ���Ϊ�ӳ�����
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