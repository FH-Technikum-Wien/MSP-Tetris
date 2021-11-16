using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TetrisManager : MonoBehaviour
{
    [SerializeField] private TetrisBlock[] tetrisBlocks;
    [SerializeField] private Transform spawnPosition;

    private TetrisBlock _currentTetris = null;

    private float _fallingSpeed = 1.0f;
    private float _time = 0.0f;

    /// <summary>
    /// Two dimensional array for storing all blocks
    /// </summary>
    private readonly Transform[,] _grid = new Transform[Constants.GRID_WIDTH, Constants.GRID_HEIGHT + 5];

    private void Update()
    {
        _time += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.A))
            MoveTetris(-1, 0);

        if (Input.GetKeyDown(KeyCode.D))
            MoveTetris(1, 0);

        if (Input.GetKeyDown(KeyCode.S))
            MoveTetris(0, -1);

        if (Input.GetKeyDown(KeyCode.W))
            MoveTetris(0, 1);

        if (Input.GetKeyDown(KeyCode.Space))
            RotateTetris();

        if (_time < _fallingSpeed)
            return;

        _time -= _fallingSpeed;
        ApplyFalling();
    }


    public void SpawnTetris()
    {
        int index = Random.Range(0, tetrisBlocks.Length);
        _currentTetris = Instantiate(tetrisBlocks[index], spawnPosition.position, Quaternion.identity);
    }

    private void MoveTetris(int xMovement, int yMovement)
    {
        if (CheckTetrisMovement(xMovement, yMovement))
        {
            _currentTetris.transform.position += new Vector3(xMovement, yMovement, 0);
        }
    }

    private void ApplyFalling()
    {
        // Bottom-reached check
        if (!CheckTetrisMovement(0, -1))
        {
            _currentTetris.enabled = false;
            AddTetrisToGrid();
            CheckTetrisRows();
            SpawnTetris();
            return;
        }

        MoveTetris(0, -1);
    }

    private void RotateTetris()
    {
        Transform pivot = _currentTetris.GetPivot();
        _currentTetris.transform.RotateAround(pivot.position, new Vector3(0, 0, 1), 90);
        // If rotation would be illegal, revert it
        if (!CheckTetrisMovement(0, 0))
            _currentTetris.transform.RotateAround(pivot.position, new Vector3(0, 0, 1), -90);
    }

    private bool CheckTetrisMovement(int xMovement, int yMovement)
    {
        foreach (Transform block in _currentTetris.GetBlocks())
        {
            Vector3 position = block.position;
            int gridPositionX = Mathf.RoundToInt(position.x + xMovement);
            int gridPositionY = Mathf.RoundToInt(position.y + yMovement);

            if (gridPositionX is < 0 or >= Constants.GRID_WIDTH)
                return false;

            if (gridPositionY < 0)
                return false;

            if (_grid[gridPositionX, gridPositionY] != null)
                return false;
        }

        return true;
    }

    private void AddTetrisToGrid()
    {
        foreach (Transform block in _currentTetris.GetBlocks())
        {
            Vector3 position = block.position;
            int gridPositionX = Mathf.RoundToInt(position.x);
            int gridPositionY = Mathf.RoundToInt(position.y);

            _grid[gridPositionX, gridPositionY] = block;
        }
    }

    private void CheckTetrisRows()
    {
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            int count = 0;
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                if (_grid[x, y] == null)
                    break;

                ++count;
            }

            if (count != _grid.GetLength(0)) 
                continue;
            
            DeleteTetrisRow(y);
            MoveTetrisRowsDown(y + 1);
            --y;
        }
    }

    private void DeleteTetrisRow(int row)
    {
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            Destroy(_grid[x, row].gameObject);
            _grid[x, row] = null;
        }
    }

    private void MoveTetrisRowsDown(int startingRow)
    {
        for (int y = startingRow; y < _grid.GetLength(1); y++)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                Transform block = _grid[x, y];

                if (block == null)
                    continue;

                block.transform.position += new Vector3(0, -1, 0);
                _grid[x, y - 1] = block;
                _grid[x, y] = null;
            }
        }
    }
}