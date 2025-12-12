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

    public bool IsDialogueFinished { get; private set; }

    void Start()
    {
        DisableAllDialogue();
        textComponent = EnemySpeakingText;
        // StartFirstEnemyDialouge();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
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

    public void StartFirstEnemyDialouge()
    {
        ShowOnlySprite(EnemyOneSprite);

        EnemyNameText.text = "Flying Dutchman";
        NameUnderlayImage.color = Hex("#FF9B04");
        DialougeBox.color = Hex("#F1E0C4");

        lines = new string[]
        {
            "Hello fellow pirate! I am the Flying Dutchman, and today you will lose a ship!",
            "Please stand still while I sink you.",
            "This process will only take a moment."
        };

        EnemyNameText.color = Color.black;
        StartDialouge();
    }

    public void StartSecondEnemyDialouge()
    {
        ShowOnlySprite(EnemyTwoSprite);

        EnemyNameText.text = "Comrade Murr";
        NameUnderlayImage.color = Hex("#0033CC");
        DialougeBox.color = Hex("#66B2FF");
        lines = new string[]
         {
            "Crewmates, today we claim victory for the tides and for me.",
            "Some of you will be… unexpectedly removed from duty.",
            "Your bravery will be briefly remembered."
         };

        EnemyNameText.color = Color.white;
        StartDialouge();
    }

    public void StartThirdEnemyDialouge()
    {
        ShowOnlySprite(EnemyThreeSprite);

        EnemyNameText.text = "Pegleg Pete";
        NameUnderlayImage.color = Hex("#6A00CC");
        DialougeBox.color = Hex("#D6A3FF");
        lines = new string[]
        {
            "Well hello there, handsome fellow pirate. I didn’t expect today to get this interesting.",
            "So tell me,",
            "are you here to flirt or are you here to fight?",
            "Either way, I like my odds."
        };
        EnemyNameText.color = Color.white;
        StartDialouge();
    }

    public void StartBlackbeardEnemyDialouge()
    {
        ShowOnlySprite(EnemyFourSprite);

        EnemyNameText.text = "Blackbeard";
        NameUnderlayImage.color = Hex("#B80000");
        DialougeBox.color = Hex("#FF7A7A");

        lines = new string[]
              {
            "Ye dare distrub me on my rum break!",
            "Your gold is mine landlubber!"
       };
        EnemyNameText.color = Color.white;
        StartDialouge();
    }

    public void StartDialouge()
    {
        EnemyDialougeCanvas.gameObject.SetActive(true);

        if (lines == null || lines.Length == 0) return;

        index = 0;
        textComponent.text = string.Empty;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        IsDialogueFinished = false;

        typingCoroutine = StartCoroutine(TypeLine());

    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        textComponent.text = string.Empty;

        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
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

        IsDialogueFinished = true;
    }

    Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }


}
