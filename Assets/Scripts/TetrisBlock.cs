using System;
using UnityEngine;
using UnityEngine.Events;

public class TetrisBlock : MonoBehaviour
{
    [SerializeField] private Transform[] blocks;
    [SerializeField] private Transform pivot;

    
    public delegate void DisabledDelegate(TetrisBlock sender);
    public event DisabledDelegate OnDisable;
    
    
    /// <summary>
    /// Two dimensional array for storing all blocks
    /// </summary>
    private static readonly Transform[,] Grid = new Transform[Constants.GRID_WIDTH, Constants.GRID_HEIGHT + 5];


    private float _fallingSpeed = 1.0f;
    private float _time = 0.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            Move(-1, 0);

        if (Input.GetKeyDown(KeyCode.D))
            Move(1, 0);

        if (Input.GetKeyDown(KeyCode.S))
            Move(0, -1);

        if (Input.GetKeyDown(KeyCode.W))
            Move(0, 1);

        if (Input.GetKeyDown(KeyCode.Space))
            Rotate();

        if (_time + _fallingSpeed > Time.time)
            return;

        _time = Time.time;
        ApplyFalling();
    }

    private void ApplyFalling()
    {
        // Bottom-reached check
        if (CheckMovement(0, -1))
        {
            Move(0, -1);
        }
        else
        {
            AddToGrid();
            OnDisable?.Invoke(this);
            enabled = false;
        }

    }

    private void Move(int xMovement, int yMovement)
    {
        if (CheckMovement(xMovement, yMovement))
        {
            transform.position += new Vector3(xMovement, yMovement, 0);
        }
    }

    private void Rotate()
    {
        transform.RotateAround(pivot.position, new Vector3(0, 0, 1), 90);
        if(!CheckMovement(0,0))
            transform.RotateAround(pivot.position, new Vector3(0, 0, 1), -90);
    }

    private bool CheckMovement(int xMovement, int yMovement)
    {
        foreach (Transform block in blocks)
        {
            Vector3 position = block.position;
            int gridPositionX = Mathf.RoundToInt(position.x + xMovement);
            int gridPositionY = Mathf.RoundToInt(position.y + yMovement);

            if (gridPositionX is < 0 or >= Constants.GRID_WIDTH)
                return false;

            if (gridPositionY < 0)
                return false;

            if (Grid[gridPositionX, gridPositionY] != null)
                return false;
        }

        return true;
    }

    private void AddToGrid()
    {
        foreach (Transform block in blocks)
        {
            Vector3 position = block.position;
            int gridPositionX = Mathf.RoundToInt(position.x);
            int gridPositionY = Mathf.RoundToInt(position.y);

            Grid[gridPositionX, gridPositionY] = block;
        }
    }
}