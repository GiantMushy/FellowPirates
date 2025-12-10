using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyDialougeController : MonoBehaviour
{
    public Canvas EnemyDialougeCanvas;
    public Image DialougeBox;
    public Image NameUnderlayImage;
    public TextMeshProUGUI EnemyNameText;
    public TextMeshProUGUI EnemySpeakingText;

    // Enemy sprites
    public RectTransform EnemyOneSprite;
    public RectTransform EnemyTwoSprite;
    public RectTransform EnemyThreeSprite;
    public RectTransform EnemyFourSprite;

    // Dialouge
    public string[] lines;
    public float textSpeed = 0.03f;
    private int index = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private TextMeshProUGUI textComponent;

    void Start()
    {
        textComponent = EnemySpeakingText;
        StartFirstEnemyDialouge();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (lines == null || lines.Length == 0) return;

            if (isTyping)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                textComponent.text = lines[index];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    void StartFirstEnemyDialouge()
    {
        ShowOnlySprite(EnemyOneSprite);

        EnemyNameText.text = "FLying Dutchman";
        NameUnderlayImage.color = Hex("#FF9B04");
        DialougeBox.color = Hex("#F1E0C4");

        lines = new string[]
            {
            "Example text for the Flying Dutchman - line 1.",
            "Second line of dialogue for the Flying Dutchman.",
            "Final line for the Flying Dutchman."
            };

        StartDialouge();
    }

    void StartSecondEnemyDialouge()
    {
        ShowOnlySprite(EnemyTwoSprite);

        EnemyNameText.text = "Comrade Murr";
        NameUnderlayImage.color = Hex("#0033CC");
        DialougeBox.color = Hex("#66B2FF");
        lines = new string[]
         {
        "Example text for the Comrade Murr - line 1.",
        "Another Comrade Murr line."
         };
        StartDialouge();
    }

    void StartThirdEnemyDialouge()
    {
        ShowOnlySprite(EnemyThreeSprite);

        EnemyNameText.text = "Pegleg Pete";
        NameUnderlayImage.color = Hex("#6A00CC");
        DialougeBox.color = Hex("#D6A3FF");
        lines = new string[]
               {
            "Example text for the Pegleg Pete - line 1.",
            "Another Pegleg Pete line."
        };
        StartDialouge();
    }

    void StartBlackbeardEnemyDialouge()
    {
        ShowOnlySprite(EnemyFourSprite);

        EnemyNameText.text = "Blackbeard";
        NameUnderlayImage.color = Hex("#B80000");
        DialougeBox.color = Hex("#FF7A7A");

        lines = new string[]
              {
            "Example text for the Blackbeard - line 1.",
            "Another Blackbeard line."
       };

        StartDialouge();
    }

    void StartDialouge()
    {
        EnemyDialougeCanvas.gameObject.SetActive(true);

        if (lines == null || lines.Length == 0) return;

        index = 0;
        textComponent.text = string.Empty;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine());

    }

    private IEnumerator TypeLine()
    {
        // type each character 1 by 1
        isTyping = true;
        textComponent.text = string.Empty;

        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }
    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            DisableAllDialogue();
        }
    }

    void ShowOnlySprite(RectTransform activeSprite)
    {
        EnemyOneSprite.gameObject.SetActive(false);
        EnemyTwoSprite.gameObject.SetActive(false);
        EnemyThreeSprite.gameObject.SetActive(false);
        EnemyFourSprite.gameObject.SetActive(false);

        activeSprite.gameObject.SetActive(true);
    }

    public void DisableAllDialogue()
    {
        EnemyDialougeCanvas.gameObject.SetActive(false);

        EnemyOneSprite.gameObject.SetActive(false);
        EnemyTwoSprite.gameObject.SetActive(false);
        EnemyThreeSprite.gameObject.SetActive(false);
        EnemyFourSprite.gameObject.SetActive(false);
    }

    Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }


}
