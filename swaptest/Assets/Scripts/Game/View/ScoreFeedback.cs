using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.View
{
    /// <summary>
    /// Score visual feedback spawned in the board when matches are done.
    /// </summary>
    public class ScoreFeedback : MonoBehaviour
    {

        [SerializeField] float _showDuration;
        [SerializeField] float _totalTimeToLive;
        [SerializeField] float _yOffset;
        [SerializeField] AnimationCurve _fadeCurve;
        [SerializeField] TMP_Text _text;

        WaitForSeconds _holdDelay;

        Action<ScoreFeedback> _onFinished;

        Coroutine _routine;

        void Awake()
        {
            _holdDelay = new WaitForSeconds(_totalTimeToLive - _showDuration);
        }

        public void Init(int score, int multiplier, Action<ScoreFeedback> onFinished)
        {
            string stringValue = score.ToString("####");
            if (multiplier > 1)
            {
                stringValue += $"(x{multiplier})";
            }
            _text.text = stringValue;
            _onFinished = onFinished;
            _routine = StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            Color startColor = _text.color;
            Color endColor = startColor;
            startColor.a = 0.0f;
            endColor.a = 1.0f;
            
            float t = 0.0f;
            float elapsed = 0.0f;
            Vector3 startPos = transform.localPosition;
            Vector3 endPos = startPos;
            endPos.y += _yOffset;

            while (elapsed < _showDuration)
            {
                t = _fadeCurve.Evaluate(elapsed / _showDuration);
                _text.color = Color.Lerp(startColor, endColor, t);
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
                elapsed += Time.deltaTime;
            }
            yield return _holdDelay;

            _routine = null;
            gameObject.SetActive(false);
            _onFinished?.Invoke(this);
        }

        public void Kill()
        {
            if(_routine != null)
            {
                StopCoroutine(_routine);
                gameObject.SetActive(false);
                _routine = null;
            }
        }
    }
}