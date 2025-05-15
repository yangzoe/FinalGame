using UnityEngine;

public class Enemy_move_l : Enemy
{
    [Header("目标设置")]
    [SerializeField] private Transform player;

    [Header("移动参数")]
    [Range(0.1f, 1.5f)] public float wanderStrength = 0.6f;
    [Range(0.1f, 2f)] public float patternChangeInterval = 0.8f;
    [Range(0.2f, 3f)] public float stopDistance = 1f;  // 停止距离

    [Header("高级设置")]
    [Range(0f, 1f)] public float speedVariation = 0.5f;
    [Range(0.1f, 3f)] public float noiseScale = 1.5f;
    [Range(1f, 5f)] public float movementMultiplier = 2f;

    // 状态变量
    private float _noiseOffset;
    private float _patternTimer;
    private Vector2 _currentVelocity;
    private float _baseAngleScale = 0.4f;

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
        speed = 10.0f;

        _noiseOffset = Random.Range(0f, 100f);
        _patternTimer = Random.Range(0f, patternChangeInterval);
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

        _patternTimer += deltaTime;

        if (_patternTimer >= patternChangeInterval)
        {
            _noiseOffset += Random.Range(-Mathf.PI * 0.3f, Mathf.PI * 0.3f);
            _patternTimer = 0f;
        }

        Vector2 toPlayer = (playerPosition - currentPosition).normalized;

        float timeNoise = Time.time * noiseScale;
        float perlinNoise = Mathf.PerlinNoise(timeNoise, _noiseOffset) * 2f - 1f;
        float sinWave = Mathf.Sin(timeNoise * 1.5f + _noiseOffset);
        float combinedNoise = Mathf.Lerp(perlinNoise, sinWave, 0.3f);

        Vector2 disturbance = new Vector2(
            Mathf.Cos(combinedNoise * Mathf.PI * _baseAngleScale),
            Mathf.Sin(combinedNoise * Mathf.PI * _baseAngleScale)
        ) * wanderStrength;

        Vector2 targetDir = (toPlayer + disturbance * 0.7f).normalized;
        float currentSpeed = speed * (1 + Mathf.Abs(sinWave) * speedVariation);

        return targetDir * currentSpeed * deltaTime * movementMultiplier;
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

        // ====== 怪物旋转代码开始 ======
        /*
        if (movement.magnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }
        */
        // ====== 怪物旋转代码结束 ======
    }

    void RotateTowardPlayer()
    {
        if (player == null) return;
        if( transform.position.x < player.position.x)
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
        if (hp <= 0)
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