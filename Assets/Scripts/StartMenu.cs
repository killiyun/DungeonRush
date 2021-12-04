using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(2);
        DifficultyManager.instance.difficulty = 1;
    }

    public void SkipTutorial()
    {
        SceneManager.LoadScene(1);
    }
}
