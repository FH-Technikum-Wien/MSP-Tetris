using TMPro;
using UnityEngine;

namespace Tetris
{
    public class TetrisLevel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtLevel;

        public void UpdateLevel(int newLevel)
        {
            txtLevel.SetText("LEVEL " + newLevel);
        }
    }
}
