using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI instance;
    public GameObject panel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(Vector3 worldPosition)
    {
        if (panel == null)
        {
            Debug.LogError("GameOverUI: panel is null.");
            return;
        }

        panel.SetActive(true);

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        if (canvas != null && panelRect != null)
        {
            Vector2 localPoint;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out localPoint
            );

            panelRect.anchoredPosition = localPoint;
        }
    }
}