using UnityEngine;
using System.Collections;

public class Enemy_move_f : Enemy
{
    [Header("目标设置")]
    [SerializeField] private Transform player;

    [Header("移动参数")]
    [Range(0.2f, 3f)] public float stopDistance = 0.3f;
    [Range(1f, 10f)] public float movementMultiplier = 5f;

    [Header("攻击设置")]
    [Range(0.5f, 3f)] public float attackCooldown = 1.5f;
    [Range(1, 5)] public int attackDamage = 1;
    public Animator animator;

    // 状态变量
    private Vector2 _currentVelocity;
    private bool _isAttacking;
    private float _lastAttackTime;
    private Coroutine _attackRoutine;

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
        speed = 5.0f;
        _currentVelocity = Vector2.zero;
        _isAttacking = false;
        _lastAttackTime = Time.time - attackCooldown;
    }

    void Update()
    {
        if (player == null) return;

        CheckHp();
        CheckAttackCondition();

        if (!_isAttacking)
        {
            Vector2 movement = CalculateMonsterMovement(
                transform.position,
                player.position,
                Time.deltaTime
            );
            ApplyMovement(movement);
        }

        RotateTowardPlayer();
        animator.SetBool("IsMoving", ShouldMove());
    }

    bool ShouldMove()
    {
        return !_isAttacking &&
               Vector2.Distance(transform.position, player.position) > stopDistance;
    }

    Vector2 CalculateMonsterMovement(Vector2 currentPosition, Vector2 playerPosition, float deltaTime)
    {
        float distance = Vector2.Distance(currentPosition, playerPosition);
        if (distance <= stopDistance) return Vector2.zero;

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
    }

    #region 攻击系统
    void CheckAttackCondition()
    {
        if (_isAttacking || Time.time < _lastAttackTime + attackCooldown) return;

        if (Vector2.Distance(transform.position, player.position) <= stopDistance * 1.2f)
        {
            if (_attackRoutine != null) StopCoroutine(_attackRoutine);
            _attackRoutine = StartCoroutine(AttackProcess());
        }
    }

    IEnumerator AttackProcess()
    {
        _isAttacking = true;
        animator.SetTrigger("Attack");  // 仅使用Trigger控制动画

        // 强制播放攻击动画并等待进入状态
        animator.Play("move_f_attack", 0, 0f);
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("move_f_attack"));

        // 获取动画信息
        AnimatorStateInfo attackState = animator.GetCurrentAnimatorStateInfo(0);
        float attackLength = attackState.length;

        // 精确时间检测伤害
        float timer = 0f;
        float damageTime = attackLength * 0.3f;
        bool damageApplied = false;

        while (timer < attackLength)
        {
            timer += Time.deltaTime;
            if (!damageApplied && timer >= damageTime)
            {
                TryApplyDamage();
                damageApplied = true;
            }
            yield return null;
        }

        // 状态恢复
        _isAttacking = false;
        _lastAttackTime = Time.time;
    }

    void TryApplyDamage()
    {
        if (Vector2.Distance(transform.position, player.position) <= stopDistance * 1.5f)
        {
            Debug.Log($"造成伤害: {attackDamage}");
            // 实际伤害逻辑示例：
            // player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }
    }
    #endregion

    #region 其他方法
    void RotateTowardPlayer()
    {
        if (player == null) return;
        float rotationY = transform.position.x < player.position.x ? 180f : 0f;
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    void CheckHp()
    {
        if (hp <= 0)
        {
            MonsterPool.Instance.ReturnEnemy(gameObject);
        }
    }

    void OnMouseDown()
    {
        TakeDamage(5);
    }

    void LateUpdate()
    {
        // 自动状态恢复保障
        if (!_isAttacking && animator.GetCurrentAnimatorStateInfo(0).IsName("move_f_attack"))
        {
            animator.Play("move_f");
        }
    }
    #endregion
}