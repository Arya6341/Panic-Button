
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("L1");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Instructions()
    {
        SceneManager.LoadScene("Instruction");
    }

    public void Restart()
    {
        string level = PlayerPrefs.GetString("Level", "L1");
        SceneManager.LoadScene(level);
    }

    public void Next()
    {
        string level = PlayerPrefs.GetString("Level", "L1");
        int number = int.Parse(level.Substring(1));

        if (number < 4)
            SceneManager.LoadScene("L" + (number + 1));
        else
            SceneManager.LoadScene("MainMenu");
    }
}
