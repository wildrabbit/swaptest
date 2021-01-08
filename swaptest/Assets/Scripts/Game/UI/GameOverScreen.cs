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

        GameEvents _gameEvents;

        void Awake()
        {
            _gameEvents = GameEvents.Instance;
            var gameFlowEvents = _gameEvents.Gameplay;
            gameFlowEvents.GameStarted += OnGameStarted;
            gameFlowEvents.GameFinished += OnGameFinished;
            Hide();
        }

        void OnDestroy()
        {
            var gameFlowEvents = _gameEvents.Gameplay;
            gameFlowEvents.GameStarted -= OnGameStarted;
            gameFlowEvents.GameFinished -= OnGameFinished;
        }

        public void OnPlayNew()
        {
            _gameEvents.UI.DispatchButtonTapped();
            _gameEvents.UI.DispatchStartGameRequested(isRestart: false);
        }

        public void OnBackToMenu()
        {
            _gameEvents.UI.DispatchButtonTapped();
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
