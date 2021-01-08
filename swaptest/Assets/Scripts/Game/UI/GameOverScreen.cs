using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

namespace Game.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        const string kGameOverTextPattern = "You scored {0} points.";
        [SerializeField] Text _scoreMessage;
        [SerializeField] UnityEngine.Object _menuScene;

        private void Awake()
        {
            var gameFlowEvents = GameController.GameEvents.Gameplay;
            gameFlowEvents.GameStarted += OnGameStarted;
            gameFlowEvents.GameFinished += OnGameFinished;
            Hide();
        }

        public void OnPlayNew()
        {
            GameController.GameEvents.UI.DispatchStartGameRequested(isRestart: false);
        }

        public void OnBackToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(_menuScene.name);
        }

        public void Show(int finalScore)
        {
            gameObject.SetActive(true);
            _scoreMessage.text = string.Format(kGameOverTextPattern, finalScore);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnGameFinished(int finalScore)
        {
            Show(finalScore);
        }

        private void OnGameStarted(int score, float elapsedTime, float remainingTime)
        {
            Hide();
        }
    }
}
