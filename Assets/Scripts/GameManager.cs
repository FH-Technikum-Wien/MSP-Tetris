using Tetris;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TetrisManager tetrisManager;
    [SerializeField] private Button btnPlay;
    [SerializeField] private Canvas startScreen;
    
    private void Awake()
    {
        tetrisManager.OnGameOver += OnGameOver;
        btnPlay.onClick.AddListener(OnPlayPressed);
    }

    private void OnPlayPressed()
    {
        btnPlay.enabled = false;
        startScreen.enabled = false;
        tetrisManager.ResetGame();
        tetrisManager.StartGame();
    }

    private void OnGameOver()
    {
        btnPlay.enabled = true;
        startScreen.enabled = true;
    }
}