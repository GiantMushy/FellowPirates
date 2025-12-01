using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Xml.Serialization;

public class ButtonSelectorController : MonoBehaviour
{
    public int buttonCount = 4;
    private int currentIndex = 0;
    float xStep = 250f;
    private RectTransform selectorRect;
    private Vector2 startPos;

    void Awake()
    {
        selectorRect = GetComponent<RectTransform>();
        startPos = selectorRect.anchoredPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(-1);
        }
    }
    void Move(int dir)
    {
        currentIndex = (currentIndex + dir + buttonCount) % buttonCount;
        selectorRect.anchoredPosition =
           startPos + new Vector2(currentIndex * xStep, 0);
    }

}
