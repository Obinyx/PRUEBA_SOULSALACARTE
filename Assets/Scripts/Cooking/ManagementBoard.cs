using UnityEngine;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Cooking
{
    public class ManagementBoard : MonoBehaviour
    {
        public void OpenBoard()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.SetGameState(GameManager.GameState.ManagementBoard);
        }
    }
}
