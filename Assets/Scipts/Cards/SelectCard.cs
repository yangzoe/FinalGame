using UnityEngine;

public class SelecteCard : MonoBehaviour
{
    /*点击方法实现
    void Start()
    {
        // 获取 CardMove 组件
        CardMove cardMove = FindObjectOfType<CardMove>();

        // 订阅事件
        if (cardMove != null)
        {
            cardMove.OnCardSelected += HandleCardSelected;
        }
    }

    // 事件处理方法
    private void HandleCardSelected(int cardIndex)
    {
        Debug.Log($"选中的卡牌序号: {cardIndex}");
        // 这里可以添加你的自定义逻辑
    }

    void OnDestroy()
    {
        // 取消订阅（防止内存泄漏）
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
            cardMove.OnCardUnhovered += HandleUnhover; // 可选
        }
    }

    private void HandleHover(int cardIndex)
    {
        Debug.Log($"悬停卡牌: {cardIndex}");
        // 例如：显示卡牌详细信息
    }

    private void HandleUnhover(int cardIndex) // 可选
    {
        Debug.Log($"离开卡牌: {cardIndex}");
        // 例如：隐藏详细信息
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