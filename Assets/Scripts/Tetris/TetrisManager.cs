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
        [Header("Tetris")] [SerializeField] private int[] pointTable = {100, 300, 500, 800};
        [SerializeField] private float[] levelSpeedTable = {1.0f, 2.0f, 4.0f, 8.0f, 12.0f};
        [SerializeField] private int[] scoreLevelTable = {1000, 2000, 4000, 8000, 12000};
        [Header("Input")] [SerializeField] private float moveTouchMinDelta = 1.0f;
        [SerializeField] private float dropTouchMinDelta = -1.0f;
        [SerializeField] private float dropTouchMaxTime = 0.5f;
        [SerializeField] private float holdingMoveSpeed = 5.0f;

        public delegate void GameOverDelegate();

        public event GameOverDelegate OnGameOver;

        // Used for deletion
        private GameObject _tetrisBlockParent;

        private TetrisBlock _currentTetris = null;
        private int _nextTetrisIndex = 0;

        private float _fallingTime = 0.0f;
        private bool _isGameOver = true;

        private int _currentScore = 0;
        private int _currentLevel = 1;

        /// <summary>
        /// Two dimensional array for storing all blocks
        /// </summary>
        private Transform[,] _grid = new Transform[Constants.GRID_WIDTH, Constants.GRID_HEIGHT + 5];

        ///////////
        // INPUT //
        ///////////

        private PlayerControls _playerControls;

        private Vector2 _cameraSizeWorldUnits;
        private Vector2 _world2Pixel;

        private float _moveLeftRightAmount;
        private float _moveDownAmount;

        private Vector2 _touchStartPosition;
        private bool _isMovingLeftRight;
        private bool _isMovingDown;

        private bool _isHoldingMove;
        private float _holdingTime;

        private float _dropTouchStartPosition;
        private float _dropTouchEndPosition;
        private float _dropTouchStartTime;


        private void Awake()
        {
            _playerControls = new PlayerControls();
        }

        private void Start()
        {
            // Calculate camera size in world units
            _cameraSizeWorldUnits.y = Camera.main.orthographicSize * 2;
            _cameraSizeWorldUnits.x = _cameraSizeWorldUnits.y * Screen.width / Screen.height;
            _world2Pixel.x = Screen.width / _cameraSizeWorldUnits.x;
            _world2Pixel.y = Screen.height / _cameraSizeWorldUnits.y;


            _playerControls.Player.MoveTouch.started += OnMoveTouchStarted;
            _playerControls.Player.MoveTouch.performed += OnMoveTouchPerformed;
            _playerControls.Player.MoveTouch.canceled += OnMoveTouchCanceled;

            _playerControls.Player.MoveKeyboard.performed += OnMoveKeyboardPerformed;
            _playerControls.Player.MoveKeyboard.canceled += OnMoveKeyboardCanceled;
            _playerControls.Player.MoveKeyboardHold.performed += OnMoveKeyboardHoldPerformed;
            _playerControls.Player.MoveKeyboardHold.canceled += OnMoveKeyboardHoldCanceled;
            
            _playerControls.Player.Rotate.performed += OnRotatePerformed;
            
            _playerControls.Player.Drop.performed += OnDropPerformed;
            _playerControls.Player.DropTouch.started += OnDropTouchStarted;
            _playerControls.Player.DropTouch.performed += OnDropTouchPerformed;
            _playerControls.Player.DropTouch.canceled += OnDropTouchCanceled;
        }

        private void OnDisable()
        {
            _playerControls.Disable();
        }

        private void Update()
        {
            if (_isGameOver)
                return;

            if (!_isHoldingMove || _isHoldingMove && 1.0f / holdingMoveSpeed + _holdingTime <= Time.time)
            {
                _holdingTime = Time.time;
                
                // Only allow either left-right or down
                if (_isMovingLeftRight)
                {
                    float direction = Mathf.Sign(_moveLeftRightAmount);
                    int movements = (int) Mathf.Abs(_moveLeftRightAmount);
                    for (int i = 0; i < movements; i++)
                        MoveTetris((int) direction, 0);
                    if (!_isHoldingMove)
                        _moveLeftRightAmount -= movements * direction;
                }
                else if (_isMovingDown)
                {
                    int movements = (int) Mathf.Abs(_moveDownAmount);
                    for (int i = 0; i < movements; i++)
                        MoveTetris(0, -1);
                    if (!_isHoldingMove)
                        _moveDownAmount += movements;
                }
            }

            ApplyFalling(Time.deltaTime);
        }

        public int CurrentScore => _currentScore;

        public void StartGame()
        {
            ResetGame();
            EnableGameControls();

            int index = Random.Range(0, tetrisBlocks.Length);
            _currentTetris = Instantiate(tetrisBlocks[index], spawnPosition.position, Quaternion.identity,
                _tetrisBlockParent.transform);

            _nextTetrisIndex = Random.Range(0, tetrisBlocks.Length);
            tetrisPreview.ShowNextTetris(_nextTetrisIndex);
        }

        public void ResetGame()
        {
            DisableGameControls();
            _currentScore = 0;
            tetrisScore.UpdateScore(_currentScore);
            _currentLevel = 1;
            tetrisLevel.UpdateLevel(_currentLevel);
            _fallingTime = 0.0f;
            _isGameOver = false;

            if (_tetrisBlockParent)
                Destroy(_tetrisBlockParent);
            _tetrisBlockParent = new GameObject("TetrisBlocks");

            _grid = new Transform[Constants.GRID_WIDTH, Constants.GRID_HEIGHT + 5];
        }

        public void EnableGameControls()
        {
            _playerControls.Player.Enable();
            _playerControls.UI.Disable();
        }

        public void DisableGameControls()
        {
            _playerControls.Player.Disable();
            _playerControls.UI.Enable();
        }

        #region Tetris Logic

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

        private void ApplyFalling(float deltaTime)
        {
            _fallingTime += deltaTime;

            float fallingSpeed = 1 / levelSpeedTable[_currentLevel - 1];
            if (_fallingTime < fallingSpeed)
                return;

            _fallingTime -= fallingSpeed;

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

        private void DropTetris()
        {
            while (CheckTetrisMovement(0, -1))
            {
                MoveTetris(0, -1);
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
        
        #endregion

        #region Tetris Gameplay
        
        private void AddScore(int numberOfRowsCleared)
        {
            _currentScore += pointTable[numberOfRowsCleared - 1] * _currentLevel;
            tetrisScore.UpdateScore(_currentScore);

            if (_currentLevel <= scoreLevelTable.Length && _currentScore >= scoreLevelTable[_currentLevel - 1])
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
        
        #endregion

        #region Player Input

        #region Touch

        private void OnMoveTouchStarted(InputAction.CallbackContext obj)
        {
            _touchStartPosition = obj.ReadValue<Vector2>();
        }

        private void OnMoveTouchPerformed(InputAction.CallbackContext obj)
        {
            Vector2 position = obj.ReadValue<Vector2>();
            Vector2 delta = position - _touchStartPosition;
            // Wait until min delta is reached
            if (delta.magnitude < moveTouchMinDelta)
                return;

            // Prevent upwards delta
            delta.y = Mathf.Min(delta.y, 0.0f);

            // Determine if user wants to move left/right or down
            if (!_isMovingLeftRight && !_isMovingDown)
            {
                float angle = Vector2.Angle(Vector2.down, delta);

                if (angle >= 45)
                    _isMovingLeftRight = true;
                else
                    _isMovingDown = true;
            }

            if (_isMovingLeftRight)
            {
                _moveLeftRightAmount += delta.x / _world2Pixel.x;
            }
            else if (_isMovingDown)
            {
                _moveDownAmount += delta.y / _world2Pixel.x;
            }

            _touchStartPosition = position;
        }

        private void OnMoveTouchCanceled(InputAction.CallbackContext obj)
        {
            _isMovingDown = false;
            _isMovingLeftRight = false;
            _moveDownAmount = 0;
            _moveLeftRightAmount = 0;
        }

        private void OnDropTouchStarted(InputAction.CallbackContext obj)
        {
            if(_isMovingLeftRight)
                return;
            
            _dropTouchStartPosition = obj.ReadValue<float>();
            _dropTouchEndPosition = _dropTouchStartPosition;
            _dropTouchStartTime = Time.time;
        }
        
        private void OnDropTouchPerformed(InputAction.CallbackContext obj)
        {
            if(_isMovingLeftRight)
                return;
            _dropTouchEndPosition = obj.ReadValue<float>();
        }
        
        private void OnDropTouchCanceled(InputAction.CallbackContext obj)
        {
            if(_isMovingLeftRight)
                return;
            
            float delta = (_dropTouchEndPosition - _dropTouchStartPosition) / _world2Pixel.y;
            if (delta <= dropTouchMinDelta && _dropTouchStartTime + dropTouchMaxTime >= Time.time)
                DropTetris();
        }
        
        #endregion

        #region Keyboard

        private void OnMoveKeyboardPerformed(InputAction.CallbackContext obj)
        {
            Vector2 delta = obj.ReadValue<Vector2>();

            // Determine if user wants to move left/right or down
            if (!_isMovingLeftRight && !_isMovingDown)
            {
                if (Mathf.Abs(delta.x) > -delta.y)
                    _isMovingLeftRight = true;
                else
                    _isMovingDown = true;
            }

            if (_isMovingLeftRight)
                _moveLeftRightAmount += delta.x;
            else if (_isMovingDown)
                _moveDownAmount += delta.y;
        }
        
        private void OnMoveKeyboardCanceled(InputAction.CallbackContext obj)
        {
            _isMovingLeftRight = false;
            _isMovingDown = false;
            _moveLeftRightAmount = 0.0f;
            _moveDownAmount = 0.0f;
        }

        private void OnMoveKeyboardHoldPerformed(InputAction.CallbackContext obj)
        {
            _isHoldingMove = true;
            
            Vector2 delta = obj.ReadValue<Vector2>();
            if (_isMovingLeftRight)
                _moveLeftRightAmount += delta.x;
            else if (_isMovingDown)
                _moveDownAmount += delta.y;
        }

        private void OnMoveKeyboardHoldCanceled(InputAction.CallbackContext obj)
        {
            _isHoldingMove = false;
            _moveLeftRightAmount = 0.0f;
            _moveDownAmount = 0.0f;
        }
        
        private void OnDropPerformed(InputAction.CallbackContext obj)
        {
            DropTetris();
        }
        
        #endregion

        private void OnRotatePerformed(InputAction.CallbackContext obj)
        {
            RotateTetris();
        }

        #endregion
    }
}