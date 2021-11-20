using UnityEngine;

namespace Tetris
{
    public class TetrisPreview : MonoBehaviour
    {
        [SerializeField] private GameObject[] tetrisPreviews;
        [SerializeField] private Transform spawnTransform;

        private GameObject _currentTetrisPreview;

        public void ShowNextTetris(int tetrisIndex)
        {
            if(_currentTetrisPreview)
                Destroy(_currentTetrisPreview);

            _currentTetrisPreview = Instantiate(tetrisPreviews[tetrisIndex], spawnTransform);
        }
    }
}
