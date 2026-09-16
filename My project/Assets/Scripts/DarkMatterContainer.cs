using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DarkMatterContainer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Данные")]
    public string contentName;
    public bool hasMass;
    public bool emitsLight;
    public bool isDarkMatter;

    [Header("Визуал")]
    public Image containerImage;
    public GameObject lightGlow;
    public RectTransform scaleDropZone;

    private RectTransform rt;
    private RectTransform canvasRect;
    private Camera canvasCamera;
    private Vector2 originalAnchoredPos;
    private Vector2 dragOffset;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
        }
    }

    void Start()
    {
        originalAnchoredPos = rt.anchoredPosition;
        if (lightGlow != null) lightGlow.SetActive(emitsLight);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (DarkMatterPuzzleManager.Instance == null) return;


        Vector2 mouseLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, canvasCamera, out mouseLocal);


        dragOffset = rt.anchoredPosition - mouseLocal;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DarkMatterPuzzleManager.Instance == null) return;

        Vector2 mouseLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, canvasCamera, out mouseLocal);

        rt.anchoredPosition = mouseLocal + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DarkMatterPuzzleManager.Instance == null || !DarkMatterPuzzleManager.Instance.IsPuzzleActive) return;

        bool droppedOnScale = RectTransformUtility.RectangleContainsScreenPoint(scaleDropZone, eventData.position);

        if (droppedOnScale)
        {
            DarkMatterPuzzleManager.Instance.OnContainerMeasured(hasMass);
        }

        StartCoroutine(SnapBack());
    }

    public void OnLoadClicked()
    {
        if (DarkMatterPuzzleManager.Instance == null || !DarkMatterPuzzleManager.Instance.IsPuzzleActive) return;
        DarkMatterPuzzleManager.Instance.TryLoadContainer(this);
    }

    System.Collections.IEnumerator SnapBack()
    {
        float t = 0f;
        Vector2 startPos = rt.anchoredPosition;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 8f; 

            rt.anchoredPosition = Vector2.Lerp(startPos, originalAnchoredPos, t);
            yield return null;
        }
        rt.anchoredPosition = originalAnchoredPos; 
    }
}