using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    [SerializeField] private Transform[] blocks;

    private void Awake()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ApplyMovement(-1, 0);

        if (Input.GetKeyDown(KeyCode.D))
            ApplyMovement(1, 0);

        if (Input.GetKeyDown(KeyCode.S))
            ApplyMovement(0, -1);

        if (Input.GetKeyDown(KeyCode.W))
            ApplyMovement(0, 1);
    }

    private void ApplyFalling()
    {

    }

    private void ApplyMovement(int xMovement, int yMovement)
    {
        if (ValidateMovement(xMovement, yMovement))
        {
            foreach (Transform block in blocks)
            {
                block.position += new Vector3(xMovement, yMovement, 0);
            }
        }
    }

    private bool ValidateMovement(int xMovement, int yMovement)
    {
        foreach (Transform block in blocks)
        {
            int gridPositionX = Mathf.RoundToInt(block.position.x);
            int gridPositionY = Mathf.RoundToInt(block.position.y);

            if (gridPositionX + xMovement < 0 || gridPositionX + xMovement > Constants.GRID_WIDTH || gridPositionY + yMovement <= 0)
            {
                return false;
            }
        }

        return true;
    }
}
