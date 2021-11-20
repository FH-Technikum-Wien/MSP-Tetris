using Tetris;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TetrisManager tetrisManager;
    
    private void Awake()
    {
        tetrisManager.StartGame();
        tetrisManager.OnGameOver += OnGameOver;
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over!");
    }
}