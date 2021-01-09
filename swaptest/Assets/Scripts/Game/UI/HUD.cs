using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Game.Events;

namespace Game.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] Text _score;
        [SerializeField] Text _timeLeft;

        [SerializeField] Button _toggleSfx;
        [SerializeField] Button _toggleMusic;

        [SerializeField] RectTransform _reshufflingFeedback;
        [SerializeField] float _reshuffleVisibleDuration = 1.0f;

        [SerializeField] GameOverScreen _gameOverScreenPrefab;

        WaitForSeconds _reshuffleDelay;
        Coroutine _reshuffleRoutine;

        void Awake()
        {
            var gameplayEvents = GameEvents.Instance.Gameplay;
            gameplayEvents.GameStarted += OnGameStarted;
            gameplayEvents.GameFinished += OnGameFinished;
            gameplayEvents.ScoreChanged += OnScoreChanged;
            gameplayEvents.TimerChanged += OnTimerChanged;
            var viewEvents = GameEvents.Instance.View;
            viewEvents.Reshuffling += OnReshuffling;

            _reshuffleDelay = new WaitForSeconds(_reshuffleVisibleDuration);
            _reshufflingFeedback.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            var gameFlowEvents = GameEvents.Instance.Gameplay;
            gameFlowEvents.GameStarted -= OnGameStarted;
            gameFlowEvents.GameFinished -= OnGameFinished;
            gameFlowEvents.ScoreChanged -= OnScoreChanged;
            gameFlowEvents.TimerChanged -= OnTimerChanged;
            var viewEvents = GameEvents.Instance.View;
            viewEvents.Reshuffling -= OnReshuffling;
        }

        void OnReshuffling()
        {
            StopExistingReshuffleRoutine();
            _reshuffleRoutine = StartCoroutine(ReshuffleFeedback());
        }

        void StopExistingReshuffleRoutine()
        {
            if (_reshuffleRoutine != null)
            {
                StopCoroutine(_reshuffleRoutine);
                _reshuffleRoutine = null;
                _reshufflingFeedback.gameObject.SetActive(false);
            }
        }

        IEnumerator ReshuffleFeedback()
        {
            _reshufflingFeedback.gameObject.SetActive(true);
            // Polish: Add visual improvs (tween scale, alpha, etc)
            yield return _reshuffleDelay;
            _reshufflingFeedback.gameObject.SetActive(false);
            _reshuffleRoutine = null;
        }

        void OnTimerChanged(float elapsed, float totalTime)
        {
            UpdateTime(totalTime - elapsed);
        }

        void OnScoreChanged(int delta, int total)
        {
            UpdateScore(total);
        }

        void OnGameFinished(int finalScore)
        {
            StopExistingReshuffleRoutine();
            UpdateScore(finalScore);

            var gameOverPopup = Instantiate(_gameOverScreenPrefab);
            gameOverPopup.Show(finalScore);
        }

        void OnGameStarted(int score, float elapsed, float totalTime)
        {
            UpdateScore(score);
            UpdateTime(totalTime - elapsed);
        }

        void UpdateScore(int score)
        {
            _score.text = score.ToString("000000");
        }

        void UpdateTime(float secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            _timeLeft.text = t.ToString(@"mm\:ss");
        }

        
    }
}
