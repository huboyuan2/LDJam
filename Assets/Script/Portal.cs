using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHint(true);
            //GameMgr.Instance.NextLevel();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") )
        {
            NextLevel();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHint(false);
            //GameMgr.Instance.NextLevel();
        }
    }
    void ShowHint(bool show)
    {
        Debug.Log("ShowHint: " + show);
        // Implement hint display logic here
    }
    void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Last level reached. No more levels to load.");
            GameLogic.Instance.Win();
            // Optionally, you can loop back to the first level or show a game completion screen
            // SceneManager.LoadScene(0); // Uncomment to loop back to the first level
        }
    }
}
