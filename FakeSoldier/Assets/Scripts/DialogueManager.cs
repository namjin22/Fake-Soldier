using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TextMeshProUGUI speakerText;
    [SerializeField] TextMeshProUGUI bodyText;

    [Header("타이핑 속도 (초/글자)")]
    [SerializeField] float charDelay = 0.04f;

    bool isTyping;
    bool skipRequested;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;
        // 타이핑 중 스킵 입력 감지
        if (isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z)))
            skipRequested = true;
    }

    // lines: 표시할 대화 라인 배열
    // onComplete: 대화 전체 종료 후 콜백
    public void StartDialogue(DialogueLine[] lines, Action onComplete)
    {
        StartCoroutine(PlayDialogue(lines, onComplete));
    }

    IEnumerator PlayDialogue(DialogueLine[] lines, Action onComplete)
    {
        FindFirstObjectOfType<PlayerController>()?.LockMovement();
        dialoguePanel.SetActive(true);

        foreach (var line in lines)
        {
            speakerText.text = line.speaker;
            yield return StartCoroutine(TypeText(line.body));

            yield return null; // 스킵키가 같은 프레임에 진행까지 처리되지 않도록 플러시
            yield return new WaitUntil(() =>
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Z));
            yield return null; // 진행키 플러시
        }

        dialoguePanel.SetActive(false);
        FindFirstObjectOfType<PlayerController>()?.UnlockMovement();
        onComplete?.Invoke();
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        skipRequested = false;
        bodyText.text = "";

        foreach (char c in text)
        {
            if (skipRequested)
            {
                bodyText.text = text;
                break;
            }
            bodyText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }
}

[Serializable]
public struct DialogueLine
{
    public string speaker;
    [TextArea(2, 5)]
    public string body;
}
