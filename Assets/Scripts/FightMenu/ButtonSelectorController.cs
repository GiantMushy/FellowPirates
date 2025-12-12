using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelectorController : MonoBehaviour
{
    private RectTransform selectorRect;
    private GameObject lastSelected;

    public AudioClip moveSound;

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

        if (selected != lastSelected)
        {
            if (moveSound != null && SoundEffectManager.instance != null)
                SoundEffectManager.instance.PlaySoundClip(moveSound, transform, 0.7f);

            lastSelected = selected;
        }

        RectTransform btnRect = selected.GetComponent<RectTransform>();

        if (btnRect != null)
        {
            selectorRect.anchoredPosition = btnRect.anchoredPosition + new Vector2(0f, -120f);
        }
    }
}
