using UnityEngine;
using UnityEngine.UI;
using SimpleMatch3.Gameplay;

namespace SimpleMatch3.UI
{

    public class UIGameplay : MonoBehaviour
    {
        public GameManager gameManager;
        public Text turnsText;
        public Text pointsText;
        public GameObject gameOverPanel;

        // Start is called before the first frame update
        void Start()
        {
            GameManager.OnPlayerMatch += UpdateTurnsText;
            GameManager.OnPlayerScore += UpdatePointsText;
            GameManager.OnGameFinished += ShowGameOverPanel;
            GameManager.OnGameStart += HideGameOverPanel;
            gameOverPanel.SetActive(false);
        }

        private void UpdatePointsText(int points)
        {
            pointsText.text = "Points: " + '\n' + points;
        }

        private void UpdateTurnsText(int turns)
        {
            turnsText.text = "Turns Left: " + '\n' + turns;
        }

        private void ShowGameOverPanel(int amount)
        {
            gameOverPanel.SetActive(true);
        }

        private void HideGameOverPanel(int amount)
        {
            gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            GameManager.OnPlayerMatch -= UpdateTurnsText;
            GameManager.OnPlayerScore -= UpdatePointsText;
            GameManager.OnGameFinished -= ShowGameOverPanel;
            GameManager.OnGameStart -= HideGameOverPanel;
        }
    }
}

    
