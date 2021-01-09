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

        [SerializeField] Color _timeRunningOutColour;
        [SerializeField] FontStyle _timeRunningOutStyle;

        [SerializeField] float _shakeSpeed = 1.0f;
        [SerializeField] float _shakeIntensity = 0.15f;
        [SerializeField] float _shakeDuration = 0.8f;

        WaitForSeconds _reshuffleDelay;
        Coroutine _reshuffleRoutine;

        Color _defaultColour;
        FontStyle _defaultStyle;

        void Awake()
        {
            var gameplayEvents = GameEvents.Instance.Gameplay;
            gameplayEvents.GameStarted += OnGameStarted;
            gameplayEvents.GameFinished += OnGameFinished;
            gameplayEvents.ScoreChanged += OnScoreChanged;
            gameplayEvents.TimerChanged += OnTimerChanged;
            gameplayEvents.TimerRunningOut += OnRunningOut;
            var viewEvents = GameEvents.Instance.View;
            viewEvents.Reshuffling += OnReshuffling;

            _reshuffleDelay = new WaitForSeconds(_reshuffleVisibleDuration);
            _reshufflingFeedback.gameObject.SetActive(false);

            _defaultColour = _timeLeft.color;
            _defaultStyle = _timeLeft.fontStyle;
        }

        void OnDestroy()
        {
            var gameplayEvents = GameEvents.Instance.Gameplay;
            gameplayEvents.GameStarted -= OnGameStarted;
            gameplayEvents.GameFinished -= OnGameFinished;
            gameplayEvents.ScoreChanged -= OnScoreChanged;
            gameplayEvents.TimerChanged -= OnTimerChanged;
            gameplayEvents.TimerRunningOut -= OnRunningOut;
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

        void OnRunningOut()
        {
            SetTimeLabelFormat(runningOutTimer: true);
            StartCoroutine(Shake());
        }

        IEnumerator Shake()
        {
            float elapsed = 0.0f;
            var shakePosition = _timeLeft.transform.localPosition;
            while (elapsed <= _shakeDuration)
            {
                _timeLeft.transform.localPosition = shakePosition + new Vector3(Mathf.Sin(Time.time * _shakeSpeed) * _shakeIntensity, 0, 0);
                yield return null;
                elapsed += Time.deltaTime;
            }
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
            SetTimeLabelFormat(runningOutTimer:false);
            UpdateTime(totalTime - elapsed);
        }

        void SetTimeLabelFormat(bool runningOutTimer)
        {
            _timeLeft.color = runningOutTimer ? _timeRunningOutColour : _defaultColour;
            _timeLeft.fontStyle = runningOutTimer ? _timeRunningOutStyle : _defaultStyle;
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
