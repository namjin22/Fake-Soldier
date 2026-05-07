using System.Collections;
using UnityEngine;

// 각 스테이지 이벤트 포인트에 부착. Trigger Collider2D 필요.
// Inspector에서 대화·선택지·선택 타입을 설정.
public class StageDirector : MonoBehaviour
{
    [Header("스테이지 번호 (1~5)")]
    [SerializeField] int stageNumber = 1;

    [Header("선택 전 대화")]
    [SerializeField] DialogueLine[] preDialogue;

    [Header("선택지 (최대 3개)")]
    [SerializeField] string[] choiceTexts;
    [SerializeField] GameManager.ChoiceType[] choiceTypes;
    [SerializeField] bool useTimer = false; // Stage 4만 true

    [Header("선택 후 공통 대화 (없으면 비워둬도 됨)")]
    [SerializeField] DialogueLine[] postDialogue;

    [Header("완료 후 다음 스테이지로 자동 이동")]
    [SerializeField] bool autoLoadNext = true;

    bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        GameManager.Instance?.SetStage(stageNumber);
        StartCoroutine(RunStage());
    }

    IEnumerator RunStage()
    {
        // 1. 선택 전 대화
        if (preDialogue.Length > 0)
        {
            bool done = false;
            DialogueManager.Instance.StartDialogue(preDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        // 2. 선택지 표시
        bool choiceDone = false;
        int chosenIndex = 0;
        ChoiceSystem.Instance.ShowChoices(choiceTexts, index =>
        {
            chosenIndex = index;
            choiceDone = true;
        }, useTimer);
        yield return new WaitUntil(() => choiceDone);

        // 3. 양심 점수 반영
        if (chosenIndex < choiceTypes.Length)
            GameManager.Instance?.ApplyChoice(choiceTypes[chosenIndex]);

        // 4. 선택 후 대화
        if (postDialogue.Length > 0)
        {
            bool done = false;
            DialogueManager.Instance.StartDialogue(postDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        // 5. 다음 스테이지 이동
        if (autoLoadNext)
            GameManager.Instance?.LoadNextStage();
    }
}
