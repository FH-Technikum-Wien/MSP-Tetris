using TMPro;
using UnityEngine;

namespace Tetris
{
    public class TetrisScore : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtScore;


        public void UpdateScore(int newScore)
        {
            txtScore.SetText("" + newScore);
        }
    }
}
