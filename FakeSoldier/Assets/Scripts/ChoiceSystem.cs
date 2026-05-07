using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class ChoiceSystem : MonoBehaviour
{
    public static ChoiceSystem Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] GameObject choicePanel;
    [SerializeField] TextMeshProUGUI[] choiceLabels; // Inspector에서 3개 연결
    [SerializeField] TextMeshProUGUI timerText;       // Stage 4용, 없으면 null

    [Header("하이라이트 색상")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color selectedColor = Color.yellow;

    [Header("Stage 4 제한 시간")]
    [SerializeField] float timeout = 10f;

    string[] currentChoices;
    int choiceCount;
    int selectedIndex;
    bool isActive;
    Coroutine timerCoroutine;
    Action<int> onChoiceMade;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        choicePanel.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        // 숫자키 1~3 직접 선택
        for (int i = 0; i < choiceCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Confirm(i);
                return;
            }
        }

        // 방향키로 커서 이동
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + choiceCount) % choiceCount;
            UpdateHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % choiceCount;
            UpdateHighlight();
        }

        // Enter로 확정
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Confirm(selectedIndex);
    }

    // choices: 선택지 텍스트 배열 (최대 3개)
    // callback: 확정 시 0-based 인덱스 전달
    // useTimer: Stage 4처럼 제한 시간 적용 여부
    public void ShowChoices(string[] choices, Action<int> callback, bool useTimer = false)
    {
        currentChoices = choices;
        choiceCount = Mathf.Clamp(choices.Length, 1, 3);
        onChoiceMade = callback;
        selectedIndex = 0;
        isActive = true;

        for (int i = 0; i < choiceLabels.Length; i++)
            choiceLabels[i].gameObject.SetActive(i < choiceCount);

        UpdateHighlight();

        if (useTimer)
        {
            timerText?.gameObject.SetActive(true);
            timerCoroutine = StartCoroutine(TimerRoutine());
        }
        else
        {
            timerText?.gameObject.SetActive(false);
        }

        choicePanel.SetActive(true);
        FindFirstObjectOfType<PlayerController>()?.LockMovement();
    }

    void Confirm(int index)
    {
        if (!isActive) return;
        isActive = false;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        choicePanel.SetActive(false);
        timerText?.gameObject.SetActive(false);
        FindFirstObjectOfType<PlayerController>()?.UnlockMovement();
        onChoiceMade?.Invoke(index);
    }

    void UpdateHighlight()
    {
        for (int i = 0; i < choiceCount; i++)
        {
            bool selected = (i == selectedIndex);
            choiceLabels[i].color = selected ? selectedColor : normalColor;
            choiceLabels[i].text = (selected ? "▶ " : "  ") + currentChoices[i];
        }
    }

    IEnumerator TimerRoutine()
    {
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            if (timerText != null)
                timerText.text = $"{Mathf.CeilToInt(timeout - elapsed)}";
            yield return null;
        }
        // 시간 초과 → index 0 (첫 번째 선택 = 명령 이행) 자동 확정
        Confirm(0);
    }
}
