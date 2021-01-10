using Game.Events;
using System.Collections;
using System;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Main in-game UI class. It will handle score and timer updates,
    /// and also show additional widgets on demand, such as the game over popup
    /// or the reshuffling widget.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        const string kScoreFormatString = "000000";
        const string kTimerFormatString = @"mm\:ss";

        [SerializeField] TMP_Text _score;
        [SerializeField] TMP_Text _highScore;
        [SerializeField] TMP_Text _timeLeft;

        [SerializeField] RectTransform _reshufflingFeedback;
        [SerializeField] float _reshuffleVisibleDuration = 1.0f;

        [SerializeField] GameOverScreen _gameOverScreenPrefab;

        [SerializeField] Color _timeRunningOutColour;
        [SerializeField] FontStyles _timeRunningOutStyle;

        [SerializeField] float _shakeSpeed = 1.0f;
        [SerializeField] float _shakeIntensity = 0.15f;
        [SerializeField] float _shakeDuration = 0.8f;

        WaitForSeconds _reshuffleDelay;
        Coroutine _reshuffleRoutine;

        Color _defaultColour;
        FontStyles _defaultStyle;

        UIEvents _uiEvents;

        bool _firstHighScoreInPlaythrough = true;

        void Awake()
        {
            var gameEvents = GameEvents.Instance;
            var gameplayEvents = gameEvents.Gameplay;
            gameplayEvents.GameStarted += OnGameStarted;
            gameplayEvents.GameFinished += OnGameFinished;
            gameplayEvents.ScoreChanged += OnScoreChanged;
            gameplayEvents.HighScoreChanged += OnHighScoreChanged;
            gameplayEvents.TimerChanged += OnTimerChanged;
            gameplayEvents.TimerRunningOut += OnRunningOut;
            var viewEvents = gameEvents.View;
            viewEvents.Reshuffling += OnReshuffling;

            _reshuffleDelay = new WaitForSeconds(_reshuffleVisibleDuration);
            _reshufflingFeedback.gameObject.SetActive(false);

            _uiEvents = gameEvents.UI;

            _defaultColour = _timeLeft.color;
            _defaultStyle = _timeLeft.fontStyle;
        }

        void OnDestroy()
        {
            var gameplayEvents = GameEvents.Instance.Gameplay;
            gameplayEvents.GameStarted -= OnGameStarted;
            gameplayEvents.GameFinished -= OnGameFinished;
            gameplayEvents.ScoreChanged -= OnScoreChanged;
            gameplayEvents.HighScoreChanged -= OnHighScoreChanged;
            gameplayEvents.TimerChanged -= OnTimerChanged;
            gameplayEvents.TimerRunningOut -= OnRunningOut;
            var viewEvents = GameEvents.Instance.View;
            viewEvents.Reshuffling -= OnReshuffling;
        }


        public void OnResetSave()
        {
            _uiEvents.DispatchResetSaveRequest();
        }

        public void OnSFXToggle()
        {
            _uiEvents.DispatchSFXToggle();
        }

        public void OnMusicToggle()
        {
            _uiEvents.DispatchMusicToggle();
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
            // Polish: Add visual improvements (tween scale, alpha, etc)
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

        void OnHighScoreChanged(int highScore)
        {
            if (_firstHighScoreInPlaythrough)
            {
                StartCoroutine(Shake(_highScore.rectTransform));
                _firstHighScoreInPlaythrough = false;
            }
            UpdateHighScore(highScore);
        }

        void OnRunningOut()
        {
            SetTimeLabelFormat(runningOutTimer: true);
            StartCoroutine(Shake(_timeLeft.rectTransform));
        }

        IEnumerator Shake(RectTransform shakingTransform)
        {
            float elapsed = 0.0f;
            var shakePosition = shakingTransform.localPosition;
            while (elapsed <= _shakeDuration)
            {
                shakingTransform.localPosition = shakePosition + new Vector3(Mathf.Sin(Time.time * _shakeSpeed) * _shakeIntensity, 0, 0);
                yield return null;
                elapsed += Time.deltaTime;
            }
            shakingTransform.localPosition = shakePosition;
        }

        void OnGameFinished(int finalScore, bool isNewHighScore, int highScore)
        {
            StopExistingReshuffleRoutine();
            UpdateScore(finalScore);

            var gameOverPopup = Instantiate(_gameOverScreenPrefab);
            gameOverPopup.Show(finalScore, isNewHighScore);
        }

        void OnGameStarted(int score, int highScore, float elapsed, float totalTime)
        {
            UpdateScore(score);
            UpdateHighScore(highScore);
            _firstHighScoreInPlaythrough = true;
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
            _score.text = score.ToString(kScoreFormatString);
        }

        void UpdateHighScore(int highScore)
        {
            _highScore.text = highScore.ToString(kScoreFormatString);
        }

        void UpdateTime(float secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            _timeLeft.text = t.ToString(kTimerFormatString);
        }
    }
}
