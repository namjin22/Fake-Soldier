using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;

    void Start()
    {
        startButton.onClick.AddListener(() => SceneManager.LoadScene("Stage_01"));
        quitButton.onClick.AddListener(() => Application.Quit());
    }
}
