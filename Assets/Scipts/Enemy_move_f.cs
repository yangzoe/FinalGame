using UnityEngine;

public class Enemy_move_f : Enemy
{
    [Header("目标设置")]
    [SerializeField] private Transform player;

    [Header("移动参数")]
    [Range(0.2f, 3f)] public float stopDistance = 0.3f;  // 停止距离
    [Range(1f, 10f)] public float movementMultiplier = 5f; // 移动速度倍率

    // 状态变量
    private Vector2 _currentVelocity;

    private void Awake()
    {
        InitializePlayer();
        OnSpawn();
    }

    void InitializePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("找不到tag为Player的对象！", this);
            enabled = false;
        }
    }

    public override void OnSpawn()
    {
        hp = 5;
        damage = 1;
        speed = 10.0f;  // 基础速度值
        _currentVelocity = Vector2.zero;
    }

    void Update()
    {
        if (player == null) return;
        CheckHp();
        Vector2 movement = CalculateMonsterMovement(
            transform.position,
            player.position,
            Time.deltaTime
        );
        RotateTowardPlayer();
        ApplyMovement(movement);
    }

    Vector2 CalculateMonsterMovement(Vector2 currentPosition, Vector2 playerPosition, float deltaTime)
    {
        // 距离判断
        float distance = Vector2.Distance(currentPosition, playerPosition);
        if (distance <= stopDistance) return Vector2.zero;

        // 直接朝向玩家移动
        Vector2 toPlayer = (playerPosition - currentPosition).normalized;
        return toPlayer * speed * deltaTime * movementMultiplier;
    }

    void ApplyMovement(Vector2 movement)
    {
        Vector2 targetPos = (Vector2)transform.position + movement;
        transform.position = Vector2.SmoothDamp(
            transform.position,
            targetPos,
            ref _currentVelocity,
            0.05f
        );

        // ====== 怪物旋转代码 ======
        /*
         if (movement.magnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }
        */
    }

    void RotateTowardPlayer()
    {
        if (player == null) return;
        if (transform.position.x < player.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void CheckHp()
    {
        if(hp <= 0)
        {
            MonsterPool.Instance.ReturnEnemy(gameObject);
        }
    }

    void TakeDamage(int damage)
    {
        hp -= damage;
    }

    void OnMouseDown()
    {
        TakeDamage(5);
    }
}