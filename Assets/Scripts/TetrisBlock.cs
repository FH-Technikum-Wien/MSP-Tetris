using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    [SerializeField] private Transform[] blocks;
    [SerializeField] private Transform pivot;


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
        if (!CheckMovement(0, -1))
        {
            enabled = false;
        }

        Move(0, -1);
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
        transform.RotateAround(pivot.position, new Vector3(0,0,1), 90);
        ValidatePosition();
    }

    private bool CheckMovement(int xMovement, int yMovement)
    {
        foreach (Transform block in blocks)
        {
            Vector3 position = block.position;
            int gridPositionX = Mathf.RoundToInt(position.x);
            int gridPositionY = Mathf.RoundToInt(position.y);

            if (gridPositionX + xMovement < 0 || gridPositionX + xMovement > Constants.GRID_WIDTH ||
                gridPositionY + yMovement <= 0)
            {
                return false;
            }
        }

        return true;
    }

    private void ValidatePosition()
    {
        foreach (Transform block in blocks)
        {
            Vector3 position = block.position;
            int gridPositionX = Mathf.RoundToInt(position.x);
            int gridPositionY = Mathf.RoundToInt(position.y);

            if (gridPositionX < 0)
                transform.position += new Vector3(gridPositionX * -1, 0, 0);
            else if (gridPositionX >= Constants.GRID_WIDTH)
                transform.position -= new Vector3(gridPositionX - (Constants.GRID_WIDTH - 1), 0, 0);

            if (gridPositionY < 0)
                transform.position += new Vector3(0, gridPositionY * -1, 0);
        }
    }
}