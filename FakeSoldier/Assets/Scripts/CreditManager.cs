using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditManager : MonoBehaviour
{
    [SerializeField] RectTransform creditContainer;
    [SerializeField] float scrollSpeed = 80f;
    [SerializeField] float endHoldTime = 4f;

    void Start()
    {
        StartCoroutine(RollCredits());
    }

    IEnumerator RollCredits()
    {
        // 화면 아래에서 시작해 위로 스크롤
        float startY = -Screen.height * 0.5f - creditContainer.rect.height * 0.5f;
        float endY = Screen.height * 0.5f + creditContainer.rect.height * 0.5f;
        creditContainer.anchoredPosition = new Vector2(0, startY);

        while (creditContainer.anchoredPosition.y < endY)
        {
            creditContainer.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(endHoldTime);
        SceneManager.LoadScene("MainMenu");
    }
}
