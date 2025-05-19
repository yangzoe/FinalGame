using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("��������")]
    public int hp ;
    public int damage ;
    public float speed ;

    [Header("���ͱ�ʶ")]
    public GameObject prefabType; // ���������Ӧ��Ԥ����


    public virtual void OnSpawn()
    {
        // ��ʼ��λ�á�״̬��
    }
    public virtual void OnDespawn()
    {
        // ����λ�á�״̬��
        transform.position = Vector3.zero;
    }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
    }
}