using UnityEngine;
using System.Collections;

public class Enemy_move_l : Enemy
{
    [Header("Ŀ������")]
    [SerializeField] private Transform player;

    [Header("�ƶ�����")]
    [Range(0.1f, 1.5f)] public float wanderStrength = 0.6f;
    [Range(0.1f, 2f)] public float patternChangeInterval = 0.8f;
    [Range(0.2f, 3f)] public float stopDistance = 1f;

    [Header("��ͣģʽ")]
    [Range(1f, 5f)] public float moveDuration = 2f;
    [Range(1f, 5f)] public float stopDuration = 1f;

    [Header("��������")]
    [Range(0.5f, 3f)] public float attackCooldown = 1.5f;
    [Range(1, 5)] public int attackDamage = 1;

    [Header("�߼�����")]
    [Range(0f, 1f)] public float speedVariation = 0.5f;
    [Range(0.1f, 3f)] public float noiseScale = 1.5f;
    [Range(1f, 5f)] public float movementMultiplier = 2f;

    public Animator animator;

    // �ƶ���ر���
    private float _noiseOffset;
    private float _patternTimer;
    private Vector2 _currentVelocity;
    private float _baseAngleScale = 0.4f;

    // ״̬���Ʊ���
    private bool _isMoving = true;
    private float _modeTimer;
    private bool _isAttacking;
    private float _lastAttackTime;
    private Coroutine _attackRoutine;
    private bool _shouldResumeMove;

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
            Debug.LogError("�Ҳ���tagΪPlayer�Ķ���", this);
            enabled = false;
        }
    }

    public override void OnSpawn()
    {
        hp = 5;
        damage = 1;
        speed = 3.0f;

        _noiseOffset = Random.Range(0f, 100f);
        _patternTimer = Random.Range(0f, patternChangeInterval);
        _currentVelocity = Vector2.zero;

        // ��ʼ��״̬
        _modeTimer = 0f;
        _isMoving = true;
        _isAttacking = false;
        _lastAttackTime = Time.time - attackCooldown;
        _shouldResumeMove = false;
    }

    void Update()
    {
        if (player == null) return;

        CheckHp();
        CheckAttackCondition();
        UpdateMovementMode();
        HandleMovement();
        RotateTowardPlayer();

        // ʵʱ���¶�������
        animator.SetBool("IsMoving", IsMoving);
    }

    #region �ƶ�����ϵͳ
    void UpdateMovementMode()
    {
        if (_isAttacking) return;

        _modeTimer += Time.deltaTime;
        float currentDuration = _isMoving ? moveDuration : stopDuration;

        if (_modeTimer >= currentDuration || _shouldResumeMove)
        {
            _isMoving = !_isMoving;
            _modeTimer = 0f;
            _shouldResumeMove = false;
            _currentVelocity = Vector2.zero;
        }
    }

    void HandleMovement()
    {
        if (!IsMoving)
        {
            _currentVelocity = Vector2.zero;
            return;
        }

        Vector2 movement = CalculateMonsterMovement(
            transform.position,
            player.position,
            Time.deltaTime
        );
        ApplyMovement(movement);
    }

    Vector2 CalculateMonsterMovement(Vector2 currentPosition, Vector2 playerPosition, float deltaTime)
    {
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
    }
    #endregion

    #region ����ϵͳ
    void CheckAttackCondition()
    {
        // ������ȴ������
        if (_isAttacking || Time.time < _lastAttackTime + attackCooldown * 0.5f) return;

        // ��̬��ⷶΧ������������С��
        float dynamicRange = Mathf.Lerp(stopDistance * 1.5f, stopDistance,
            (Time.time - _lastAttackTime) / attackCooldown);

        if (Vector2.Distance(transform.position, player.position) <= dynamicRange)
        {
            if (_attackRoutine != null) StopCoroutine(_attackRoutine);
            _attackRoutine = StartCoroutine(AttackProcess());
        }
    }

    // �޸ĺ�Ĺ���Э�̲���
    IEnumerator AttackProcess()
    {
        if (_isAttacking) yield break;
        _isAttacking = true;

        // ǿ�����ö���״̬��ȷ��ÿ�δ�ͷ����
        animator.Play("move_l_attack", 0, 0f);
        animator.SetTrigger("Attack");
        animator.SetBool("IsAttacking", true);

        // �����ƶ�ϵͳ
        bool wasMoving = _isMoving;
        _isMoving = false;
        _currentVelocity = Vector2.zero;

        // ���ɿ��Ķ����������
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("move_l_attack"));

        // ��ȡ������Ϣ
        AnimatorStateInfo attackState = animator.GetCurrentAnimatorStateInfo(0);
        float attackLength = attackState.length;

        // ʹ�þ�ȷʱ���⣨���ڶ���ʵ��ʱ����
        float timer = 0f;
        float damageTime = attackLength * 0.35f;
        bool damageApplied = false;

        while (timer < attackLength)
        {
            timer += Time.deltaTime;

            // ��ָ��ʱ���Ӧ���˺�
            if (!damageApplied && timer >= damageTime)
            {
                TryApplyDamage();
                damageApplied = true;
            }
            yield return null;
        }

        // ״̬�ָ�
        _isAttacking = false;
        _isMoving = wasMoving;
        _shouldResumeMove = true;
        _lastAttackTime = Time.time;

        animator.SetBool("IsAttacking", false);
        animator.ResetTrigger("Attack");
    }

    // �޸ĺ��TryApplyDamage����
    void TryApplyDamage()
    {
        // ���ݹ���ʱ�ĳ�ʼλ���жϣ���������ƶ���ʧЧ��
        float currentStopDistance = stopDistance * 1.2f;
        if (Vector2.Distance(transform.position, player.position) <= currentStopDistance)
        {
            // ʵ���˺��߼�
            Debug.Log("�ɹ�����˺�: " + attackDamage);
            // �������˺�Ӧ���߼����������ҵ����˷�����
        }
    }
    #endregion

    #region ��������
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

    void TakeDamage(int damage)
    {
        hp -= damage;
    }

    void OnMouseDown()
    {
        TakeDamage(5);
    }

    void LateUpdate()
    {
        // ǿ��ͬ������״̬
        animator.SetBool("IsAttacking", _isAttacking);

        // ��ֹ�������ڹ���״̬
        if (!_isAttacking && animator.GetCurrentAnimatorStateInfo(0).IsName("move_l_attack"))
        {
            animator.Play("move_l");
        }
    }

    public bool IsMoving
    {
        get => _isMoving &&
              !_isAttacking &&
              Vector2.Distance(transform.position, player.position) > stopDistance * 0.8f;
    }
    #endregion
}