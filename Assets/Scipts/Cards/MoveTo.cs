using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class MoveTo : MonoBehaviour
{
    [Header("Layout Range (0~1比例)")]
    [Tooltip("水平起始百分比 (0=左边,1=右边)")]
    [Range(0f, 1f)] public float leftRatio = 0f;
    [Tooltip("水平结束百分比 (0=左边,1=右边)")]
    [Range(0f, 1f)] public float rightRatio = 1f;

    [Header("垂直位置参考点")]
    public RectTransform startPoint;
    public RectTransform spawnPoint;

    [Header("Card Properties")]
    public float cardWidth = 100f;

    [Header("Hover Effects")]
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1f);
    public Vector2 hoverOffset = new Vector2(0, 20);
    public float hoverDuration = 0.3f;
    public Ease hoverEase = Ease.OutBack;

    [Header("Entrance Animation")]
    public float moveDuration = 0.5f;
    public Ease moveEase = Ease.OutQuad;

    private Canvas canvas;
    private RectTransform canvasRect;
    private float canvasHeight;
    private float cardHalfWidth;

    private class Card
    {
        public RectTransform rect;
        public Vector2 originalPos;
        public Vector3 originalScale = Vector3.one;
        public int index;
    }
    private Card[] cards;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 预先计算半宽，用于边缘贴合
        cardHalfWidth = cardWidth * 0.5f;
    }

    void Start()
    {
        InitializeCards();
        StartLayoutAnimation();
    }

    void InitializeCards()
    {
        string[] names = { "Card_1", "Card_2", "Card_3", "Card_2", "Card_3", "Card_2", "Card_3", "Card_2" };
        cards = new Card[names.Length];
        bool isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;

        for (int i = 0; i < names.Length; i++)
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/Cards/{names[i]}");
            if (prefab == null) continue;
            var obj = Instantiate(prefab, canvas.transform);
            var rt = obj.GetComponent<RectTransform>();
            AddUIComponents(obj, i);

            Vector2 initPos = isOverlay ? spawnPoint.anchoredPosition : GetLocalPosition(spawnPoint);
            cards[i] = new Card { rect = rt, originalPos = initPos, index = i };
            rt.anchoredPosition = initPos;
            rt.localScale = Vector3.one;
        }
    }

    void AddUIComponents(GameObject go, int idx)
    {
        if (!go.TryGetComponent<Canvas>(out var c)) c = go.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = cards.Length - idx;
        if (!go.TryGetComponent<GraphicRaycaster>(out _)) go.AddComponent<GraphicRaycaster>();
        var trig = go.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => OnHover(idx, true));
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => OnHover(idx, false));
        trig.triggers.Add(enter);
        trig.triggers.Add(exit);
    }

    void OnHover(int i, bool over)
    {
        var c = cards[i];
        c.rect.DOKill();
        Vector2 target = over ? c.originalPos + hoverOffset : c.originalPos;
        if (over)
        {
            c.rect.SetAsLastSibling();
            c.rect.GetComponent<Canvas>().sortingOrder = 999;
        }
        else
        {
            c.rect.SetSiblingIndex(i);
            c.rect.GetComponent<Canvas>().sortingOrder = cards.Length - i;
        }
        DOTween.To(() => c.rect.anchoredPosition, x => c.rect.anchoredPosition = x, target, hoverDuration).SetEase(hoverEase);
        c.rect.DOScale(over ? hoverScale : c.originalScale, hoverDuration).SetEase(hoverEase);
    }

    void StartLayoutAnimation()
    {
        int n = cards.Length;
        float range = Mathf.Clamp01(rightRatio - leftRatio);

        // 取Canvas当前Rect
        Rect rect = canvasRect.rect;
        float xMin = rect.xMin;
        float totalWidth = rect.width;

        // 垂直基准Y
        float baseY = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? startPoint.anchoredPosition.y
            : GetLocalPosition(startPoint).y;

        // 可用水平空间 = 总宽度 - 卡牌宽度
        float usableWidth = totalWidth - cardWidth;

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < n; i++)
        {
            int idx = i;
            var c = cards[idx];
            float t = (n == 1) ? 0.5f : idx / (float)(n - 1);
            float xNorm = Mathf.Lerp(leftRatio, rightRatio, t);
            // x = xMin + halfCardW + xNorm * usableWidth
            float x = xMin + cardHalfWidth + xNorm * usableWidth;
            Vector2 target = new Vector2(x, baseY);

            seq.AppendCallback(() =>
            {
                c.originalPos = target;
                c.rect.SetSiblingIndex(idx);
                DOTween.To(() => c.rect.anchoredPosition, v => c.rect.anchoredPosition = v, target, moveDuration)
                       .SetEase(moveEase);
            });
            seq.AppendInterval(moveDuration * 0.5f);
        }
    }

    Vector2 GetLocalPosition(RectTransform rt)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, canvas.worldCamera, out Vector2 local);
        return local;
    }

    void OnValidate()
    {
        if (Application.isPlaying && cards != null) StartLayoutAnimation();
    }
}
