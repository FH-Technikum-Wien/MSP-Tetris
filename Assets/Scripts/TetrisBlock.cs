using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    [SerializeField] private Transform[] blocks;
    [SerializeField] private Transform pivot;


    public ref Transform[] GetBlocks() => ref blocks;
    public ref Transform GetPivot() => ref pivot;
}