using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelectorController : MonoBehaviour
{
    private RectTransform selectorRect;

    void Awake()
    {
        selectorRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            return;
        }

        RectTransform btnRect = selected.GetComponent<RectTransform>();

        if (btnRect != null)
        {
            selectorRect.anchoredPosition = btnRect.anchoredPosition + new Vector2(0f, -120f);
        }
    }
}
