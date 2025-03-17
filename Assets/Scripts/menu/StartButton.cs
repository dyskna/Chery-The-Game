using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StartButton : MonoBehaviour
{
    // Start is called before the first frame update
    public int gameStartScene;

    // Update is called once per frame
    public void StartGame()
    {
        SceneManager.LoadScene(gameStartScene);
    }
}
