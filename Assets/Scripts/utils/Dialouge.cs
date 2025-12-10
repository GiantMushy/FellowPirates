// using UnityEngine;using TMPro;
// using System.Collections;


// public class Dialouge : MonoBehaviour
// {
//     public TextMeshProUGUI textComponent;
//     public string[] lines;
//     public float textSpeed;

//     private int index;

//     void Start()
//     {
//         textComponent.text = string.Empty;
//         StartDialouge();
//     }

//     void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             if (textComponent.text == lines[index])
//             {
//                 NextLine();
//             }
//             else
//             {
//                 StopAllCoroutines();
//                 textComponent.text = lines[index];
//             }
//         }
//     }

//     void StartDialouge()
//     {
//         index = 0;

//         StartCoroutine(TypeLine());

//     }

//     private IEnumerator TypeLine()
//     {
//         // type each character 1 by 1
//         foreach (char c in lines[index].ToCharArray())
//         {
//             textComponent.text += c;
//             yield return new WaitForSeconds(textSpeed);
//         }
//     }

//     private void NextLine()
//     {
//         if (index < lines.Length - 1)
//         {
//             index++;
//             textComponent.text = string.Empty;
//             StartCoroutine(TypeLine());
//         }
//         else
//         {
//             gameObject.SetActive(false);
//         }
//     }

// }
