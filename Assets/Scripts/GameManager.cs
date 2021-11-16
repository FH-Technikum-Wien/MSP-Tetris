using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TetrisBlock[] tetrisBlocks;
    [SerializeField] private Transform spawnPosition;

    private void Awake()
    {
        TetrisBlock tetrisBlock = Instantiate(tetrisBlocks[Random.Range(0, tetrisBlocks.Length)],
            spawnPosition.position, Quaternion.identity);
        tetrisBlock.OnDisable += OnTetrisBlockDisabled;
    }

    private void OnTetrisBlockDisabled(TetrisBlock sender)
    {
        sender.OnDisable -= OnTetrisBlockDisabled;
        TetrisBlock tetrisBlock = Instantiate(tetrisBlocks[Random.Range(0, tetrisBlocks.Length)],
            spawnPosition.position, Quaternion.identity);
        tetrisBlock.OnDisable += OnTetrisBlockDisabled;
    }
}