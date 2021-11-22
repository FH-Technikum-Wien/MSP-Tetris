using Tetris;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TetrisManager tetrisManager;
    [SerializeField] private Canvas startScreen;
    [SerializeField] private Button btnPlay;
    
    [SerializeField] private Canvas gameOverScreen;
    [SerializeField] private Button btnPlayAgain;
    [SerializeField] private TextMeshProUGUI txtGameOverScreen;
    
    private void Awake()
    {
        tetrisManager.OnGameOver += OnGameOver;
        btnPlay.onClick.AddListener(OnPlayPressed);
        btnPlayAgain.onClick.AddListener(OnPlayAgainPressed);
    }

    private void OnPlayPressed()
    {
        startScreen.enabled = false;
        btnPlay.enabled = false;
        tetrisManager.ResetGame();
        tetrisManager.StartGame();
    }
    
    private void OnPlayAgainPressed()
    {
        gameOverScreen.enabled = false;
        btnPlayAgain.enabled = false;
        tetrisManager.ResetGame();
        tetrisManager.StartGame();
    }

    private void OnGameOver()
    {
        gameOverScreen.enabled = true;
        btnPlayAgain.enabled = true;
        txtGameOverScreen.SetText("SCORE: " + tetrisManager.CurrentScore);
    }
}