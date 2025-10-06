using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class WinScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("Score").GetComponent<TextMeshProUGUI>())
        {
            GameObject.Find("Score").GetComponent<TextMeshProUGUI>().text = GameObject.Find("Score").GetComponent<TextMeshProUGUI>().text + NextSceneData.Score.ToString("F2");
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(1);
        }
    }
}
