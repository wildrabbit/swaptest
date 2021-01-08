using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Game.Events;

namespace Game.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        const string kGameOverTextPattern = "You scored {0} points.";
        [SerializeField] Text _scoreMessage;
        [SerializeField] string _menuScene;

        void Awake()
        {
            var gameFlowEvents = GameEvents.Instance.Gameplay;
            gameFlowEvents.GameStarted += OnGameStarted;
            gameFlowEvents.GameFinished += OnGameFinished;
            Hide();
        }

        void OnDestroy()
        {
            var gameFlowEvents = GameEvents.Instance.Gameplay;
            gameFlowEvents.GameStarted -= OnGameStarted;
            gameFlowEvents.GameFinished -= OnGameFinished;
        }

        public void OnPlayNew()
        {
            GameEvents.Instance.UI.DispatchStartGameRequested(isRestart: false);
        }

        public void OnBackToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(_menuScene);
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
