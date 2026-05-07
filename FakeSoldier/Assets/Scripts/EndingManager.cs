using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI bodyText;
    [SerializeField] TextMeshProUGUI promptText;

    [Header("내용 (Inspector에서 씬별로 설정)")]
    [SerializeField] string titleContent;
    [SerializeField, TextArea(4, 15)] string bodyContent;

    [Header("설정")]
    [SerializeField] string nextScene = "Credit";
    [SerializeField] float charDelay = 0.03f;

    bool canProceed;

    void Start()
    {
        titleText.text = titleContent;
        promptText.gameObject.SetActive(false);
        StartCoroutine(ShowEnding());
    }

    IEnumerator ShowEnding()
    {
        bodyText.text = "";
        foreach (char c in bodyContent)
        {
            bodyText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        promptText.gameObject.SetActive(true);
        canProceed = true;
    }

    void Update()
    {
        if (!canProceed) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene(nextScene);
    }
}
