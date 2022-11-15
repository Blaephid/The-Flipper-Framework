using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HintBox : MonoBehaviour
{
    public static HintBox instance;
    public TextMeshProUGUI hintText;
    public float TextDelay;
    public int FadeSpread;
    public Animator BoxAnimator;
    public bool IsShowing { get; protected set; }
    int page = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Color initialColor = hintText.color;
        initialColor.a = 0;
        hintText.color = initialColor;
        gameObject.SetActive(false);
    }
    public void ShowHint (string[] hint, float[] duration)
    {
        page = 0;
        gameObject.SetActive(true);
        if (hintText == null)
        {
            Debug.LogError("No text asset was assigned to the hint box.");
            return;
        }
        StartCoroutine(DisplayText(hint[0], duration, hint));
    }

    IEnumerator DisplayText (string text, float[] duration, string[] fullText)
    {
        bool turnPage = false;
        //Debug.Log(text);

        IsShowing = true;
        BoxAnimator.SetBool("Active", true);
        hintText.text = text;

        hintText.ForceMeshUpdate();
        TMP_TextInfo textInfo = hintText.textInfo;
        Color32[] newVertexColors;

        int index = 0;
        int startChar = index;
        bool OutOfRange = false;
        while (!OutOfRange)
        {
            int Length = textInfo.characterCount;

            byte fadeSteps = (byte)Mathf.Max(1, 255 / FadeSpread);
            for (int i = startChar; i < index + 1; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                byte alpha = (byte)Mathf.Clamp(newVertexColors[vertexIndex].a + fadeSteps, 0, 255);
                newVertexColors[vertexIndex].a = alpha;
                newVertexColors[vertexIndex + 1].a = alpha;
                newVertexColors[vertexIndex + 2].a = alpha;
                newVertexColors[vertexIndex + 3].a = alpha;

                if (alpha == 255)
                {
                    startChar += 1;
                    if (startChar == Length)
                    {
                        hintText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                        yield return new WaitForSeconds(duration[page]);
                        page += 1;

                        if(page < fullText.Length)
                        {
                            turnPage = true;
                            
                        }

                        hintText.ForceMeshUpdate();
                        OutOfRange = true;

                    }
                }
            }

            hintText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            if (index + 1 < Length) index += 1;
            yield return new WaitForSeconds(TextDelay);
        }
        BoxAnimator.SetBool("Active", false);
        gameObject.SetActive(false);
        IsShowing = false;

        if(turnPage)
        {
            gameObject.SetActive(true);
            StartCoroutine(DisplayText(fullText[page], duration, fullText));
        }
    }
}
