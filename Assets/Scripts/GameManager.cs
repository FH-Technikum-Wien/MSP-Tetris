using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TetrisManager tetrisManager;
    
    private void Awake()
    {
        tetrisManager.SpawnTetris();
    }
}