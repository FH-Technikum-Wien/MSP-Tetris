using UnityEngine;

namespace Tetris
{
    public class TetrisBlock : MonoBehaviour
    {
        [SerializeField] private Transform[] blocks;
        [SerializeField] private Transform pivot;
        [SerializeField] private bool hasOnlyTwoPosition = false;


        public ref Transform[] GetBlocks() => ref blocks;
        public ref Transform GetPivot() => ref pivot;
        public bool HasOnlyTwoPosition => hasOnlyTwoPosition;
    }
}