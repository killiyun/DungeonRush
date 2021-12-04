using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyMenu : MonoBehaviour
{
    public void Easy()
    {
        SceneManager.LoadScene(1);
        DifficultyManager.instance.difficulty = 1;
    }

    public void Medium()
    {
        SceneManager.LoadScene(1);
        DifficultyManager.instance.difficulty = 12;
    }

    public void Hard()
    {
        SceneManager.LoadScene(1);
        DifficultyManager.instance.difficulty = 20;
    }
}