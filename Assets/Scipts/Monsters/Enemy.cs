using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("属性配置")]
    public int hp ;
    public int damage ;
    public float speed ;

    [Header("类型标识")]
    public GameObject prefabType; // 必须拖入对应的预制体


    public virtual void OnSpawn()
    {
        // 初始化位置、状态等
    }
    public virtual void OnDespawn()
    {
        // 重置位置、状态等
        transform.position = Vector3.zero;
    }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
    }
}