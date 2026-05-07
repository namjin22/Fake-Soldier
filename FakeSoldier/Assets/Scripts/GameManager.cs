using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 양심 점수
    public int ConscienceScore { get; private set; } = 0;

    // Stage 4 핵심 발포 거부 여부 (True End 조건)
    public bool KeyRefusalDone { get; private set; } = false;

    // 현재 스테이지 번호
    public int CurrentStage { get; private set; } = 0;

    // 엔딩 분기 점수 기준 (수정 가능)
    [Header("엔딩 분기 기준 점수")]
    public int badEndMaxScore = 0;      // 이하 → Bad End
    public int normalEndMaxScore = 4;   // 이하 → Normal End
                                        // 초과 + KeyRefusal → True End

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 선택 타입
    public enum ChoiceType
    {
        FullObey,       // 명령 완전 이행: -2
        Passive,        // 수동적 방관: 0
        MinorRefusal,   // 소극적 거부: +1
        ActiveRefusal,  // 적극적 거부: +2
        KeyRefusal,     // 핵심 발포 거부 (Stage 4): +3
    }

    // 선택 처리 - StageDirector 등에서 호출
    public void ApplyChoice(ChoiceType choice)
    {
        switch (choice)
        {
            case ChoiceType.FullObey:      ConscienceScore -= 2; break;
            case ChoiceType.Passive:       ConscienceScore += 0; break;
            case ChoiceType.MinorRefusal:  ConscienceScore += 1; break;
            case ChoiceType.ActiveRefusal: ConscienceScore += 2; break;
            case ChoiceType.KeyRefusal:
                ConscienceScore += 3;
                KeyRefusalDone = true;
                break;
        }

        Debug.Log($"[GameManager] 선택: {choice} / 양심 점수: {ConscienceScore}");
    }

    public void SetStage(int stage)
    {
        CurrentStage = stage;
    }

    // 엔딩 씬으로 이동
    public void GoToEnding()
    {
        string endingScene = DetermineEnding();
        Debug.Log($"[GameManager] 최종 점수: {ConscienceScore} / 엔딩: {endingScene}");
        SceneManager.LoadScene(endingScene);
    }

    string DetermineEnding()
    {
        if (ConscienceScore <= badEndMaxScore)
            return "Ending_Bad";

        if (ConscienceScore <= normalEndMaxScore)
            return "Ending_Normal";

        // True End는 점수 높음 + 핵심 거부 필요
        if (KeyRefusalDone)
            return "Ending_True";

        // 핵심 거부 없이 점수만 높으면 Normal
        return "Ending_Normal";
    }

    // 씬 이동 헬퍼
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextStage()
    {
        CurrentStage++;
        if (CurrentStage > 5)
        {
            GoToEnding();
            return;
        }
        SceneManager.LoadScene($"Stage_0{CurrentStage}");
    }

    // 게임 초기화 (메인메뉴 돌아갈 때)
    public void ResetGame()
    {
        ConscienceScore = 0;
        KeyRefusalDone = false;
        CurrentStage = 0;
        SceneManager.LoadScene("MainMenu");
    }
}
