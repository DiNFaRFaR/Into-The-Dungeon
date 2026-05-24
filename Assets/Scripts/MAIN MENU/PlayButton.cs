using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Map 1");
    }
}