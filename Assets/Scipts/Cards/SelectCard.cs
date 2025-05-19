using UnityEngine;

public class SelecteCard : MonoBehaviour
{
    /*�������ʵ��
    void Start()
    {
        // ��ȡ CardMove ���
        CardMove cardMove = FindObjectOfType<CardMove>();

        // �����¼�
        if (cardMove != null)
        {
            cardMove.OnCardSelected += HandleCardSelected;
        }
    }

    // �¼�������
    private void HandleCardSelected(int cardIndex)
    {
        Debug.Log($"ѡ�еĿ������: {cardIndex}");
        // ��������������Զ����߼�
    }

    void OnDestroy()
    {
        // ȡ�����ģ���ֹ�ڴ�й©��
        CardMove cardMove = FindObjectOfType<CardMove>();
        if (cardMove != null)
        {
            cardMove.OnCardSelected -= HandleCardSelected;
        }
    }
    */
    void Start()
    {
        CardMove cardMove = FindObjectOfType<CardMove>();
        if (cardMove != null)
        {
            cardMove.OnCardHovered += HandleHover;
            cardMove.OnCardUnhovered += HandleUnhover; // ��ѡ
        }
    }

    private void HandleHover(int cardIndex)
    {
        Debug.Log($"��ͣ����: {cardIndex}");
        // ���磺��ʾ������ϸ��Ϣ
    }

    private void HandleUnhover(int cardIndex) // ��ѡ
    {
        Debug.Log($"�뿪����: {cardIndex}");
        // ���磺������ϸ��Ϣ
    }

    void OnDestroy()
    {
        CardMove cardMove = FindObjectOfType<CardMove>();
        if (cardMove != null)
        {
            cardMove.OnCardHovered -= HandleHover;
            cardMove.OnCardUnhovered -= HandleUnhover;
        }
    }
}