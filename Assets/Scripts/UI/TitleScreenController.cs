using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown)
        {
            // Transition to Menu or directly to first level if desired
            SceneManager.LoadScene("Level_1");
        }
    }
}
