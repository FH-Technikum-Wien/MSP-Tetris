using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Tetris
{
    public class TetrisManager : MonoBehaviour
    {
        [SerializeField] private TetrisBlock[] tetrisBlocks;
        [SerializeField] private Transform spawnPosition;
        [SerializeField] private TetrisPreview tetrisPreview;
        [SerializeField] private TetrisScore tetrisScore;
        [SerializeField] private TetrisLevel tetrisLevel;

        public delegate void GameOverDelegate();

        public event GameOverDelegate OnGameOver;

        // Used for deletion
        private GameObject _tetrisBlockParent;

        private TetrisBlock _currentTetris = null;
        private int _nextTetrisIndex = 0;

        private float _time = 0.0f;
        private bool _isGameOver = true;

        private readonly int[] _pointTable = {100, 300, 500, 800};
        private int _currentScore = 0;
        private readonly float[] _levelSpeedTable = {1.0f, 2.0f, 4.0f, 8.0f};
        private int _currentLevel = 1;

        private readonly int[] _scoreLevelTable = {2000, 4000, 10000};

        /// <summary>
        /// Two dimensional array for storing all blocks
        /// </summary>
        private Transform[,] _grid = new Transform[Constants.GRID_WIDTH, Constants.GRID_HEIGHT + 5];

        ///////////
        // INPUT //
        ///////////
        private float _pressToHoldDelay = 0.25f;
        private float _timeSincePress = 0.0f;
        private bool _isHolding = false;

        private float _inputTime = 0.0f;
        private float _inputSpeed = 0.2f;


        private void Update()
        {
            if (_isGameOver)
                return;

            _time += Time.deltaTime;

            Touch[] touches = Input.touches;

            if (touches.Length > 0)
            {
                Touch touch = touches[0];
                Debug.Log(touch);
            }

            HandleInput(Time.deltaTime);

            float fallingSpeed = 1 / _levelSpeedTable[_currentLevel - 1];
            if (_time < fallingSpeed)
                return;

            _time -= fallingSpeed;
            ApplyFalling();
        }

        public void StartGame()
        {
            ResetGame();
            int index = Random.Range(0, tetrisBlocks.Length);
            _currentTetris = Instantiate(tetrisBlocks[index], spawnPosition.position, Quaternion.identity,
                _tetrisBlockParent.transform);

            _nextTetrisIndex = Random.Range(0, tetrisBlocks.Length);
            tetrisPreview.ShowNextTetris(_nextTetrisIndex);
        }

        public void ResetGame()
        {
            _currentScore = 0;
            tetrisScore.UpdateScore(_currentScore);
            _currentLevel = 1;
            tetrisLevel.UpdateLevel(_currentLevel);
            _time = 0.0f;
            _isGameOver = false;

            if (_tetrisBlockParent)
                Destroy(_tetrisBlockParent);
            _tetrisBlockParent = new GameObject("TetrisBlocks");

            _grid = new Transform[Constants.GRID_WIDTH, Constants.GRID_HEIGHT + 5];
        }

        private void HandleInput(float deltaTime)
        {
            if (Input.GetKey(KeyCode.A))
            {
                _inputTime += deltaTime;
                
                if (!_isHolding)
                {
                    MoveTetris(-1, 0);
                    _isHolding = true;
                    _timeSincePress = Time.time;
                }
                else if(_timeSincePress + _pressToHoldDelay < Time.time)
                {
                    if (_inputTime >= _inputSpeed)
                    {
                        _inputTime -= _inputSpeed;
                        MoveTetris(-1, 0);
                    }
                }
            }
            else
            {
                _isHolding = false;
                _inputTime = 0.0f;
            }
            
            
            /*
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A))
                MoveTetris(-1, 0);

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D))
                MoveTetris(1, 0);

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S))
                MoveTetris(0, -1);

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W))
                MoveTetris(0, 1);

            if (Input.GetKeyDown(KeyCode.Space))
                RotateTetris();
                */
        }


        private void SpawnTetris()
        {
            _currentTetris = Instantiate(tetrisBlocks[_nextTetrisIndex], spawnPosition.position, Quaternion.identity,
                _tetrisBlockParent.transform);

            if (!CheckTetrisMovement(0, 0))
                GameOver();

            _nextTetrisIndex = Random.Range(0, tetrisBlocks.Length);
            tetrisPreview.ShowNextTetris(_nextTetrisIndex);
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
                AddTetrisToGrid();
                if (_isGameOver)
                    return;
                CheckTetrisRows();
                SpawnTetris();
                return;
            }

            MoveTetris(0, -1);
        }

        private void RotateTetris()
        {
            Transform pivot = _currentTetris.GetPivot();

            if (_currentTetris.HasOnlyTwoPosition)
            {
                // Ping pong between two rotation states
                float angle = _currentTetris.transform.rotation.eulerAngles.z <= 45 ? 90 : -90;
                _currentTetris.transform.RotateAround(pivot.position, new Vector3(0, 0, 1), -angle);
                // If rotation would be illegal, revert it
                if (!CheckTetrisMovement(0, 0))
                    _currentTetris.transform.RotateAround(pivot.position, new Vector3(0, 0, 1), angle);
            }
            else
            {
                _currentTetris.transform.RotateAround(pivot.position, new Vector3(0, 0, 1), -90);
                // If rotation would be illegal, revert it
                if (!CheckTetrisMovement(0, 0))
                    _currentTetris.transform.RotateAround(pivot.position, new Vector3(0, 0, 1), 90);
            }
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

                if (gridPositionY >= Constants.GRID_HEIGHT)
                {
                    GameOver();
                    return;
                }

                _grid[gridPositionX, gridPositionY] = block;

                block.parent = _tetrisBlockParent.transform;

                // Delete not needed parent and pivot
                Destroy(_currentTetris.gameObject);
            }
        }

        private void CheckTetrisRows()
        {
            int rowsDeleted = 0;
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
                ++rowsDeleted;
            }

            if (rowsDeleted > 0)
                AddScore(rowsDeleted);
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


        private void AddScore(int numberOfRowsCleared)
        {
            _currentScore = _pointTable[numberOfRowsCleared - 1] * _currentLevel;
            tetrisScore.UpdateScore(_currentScore);

            if (_currentLevel <= _scoreLevelTable.Length - 1 && _currentScore >= _scoreLevelTable[_currentLevel])
                IncreaseLevel();
        }

        private void IncreaseLevel()
        {
            ++_currentLevel;
            tetrisLevel.UpdateLevel(_currentLevel);
        }

        private void GameOver()
        {
            _isGameOver = true;
            OnGameOver?.Invoke();
        }
    }
}