using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    public Text scoreText = null;
    public Text killText = null;

    private void Awake()
    {
        scoreText.text = "Score: " + GameController.instance.playerScore.ToString();
        killText.text = "Demons Killed: " + GameController.instance.playerKills.ToString();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
