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

        UIEvents _uiEvents;

        private void Awake()
        {
            _uiEvents = GameEvents.Instance.UI;
        }

        public void OnPlayNew()
        {
            _uiEvents.DispatchButtonTapped();
            _uiEvents.DispatchStartGameRequested(isRestart: false);
            Close();
        }

        public void OnBackToMenu()
        {
            _uiEvents.DispatchButtonTapped();
            UnityEngine.SceneManagement.SceneManager.LoadScene(_menuScene);
            Close();
        }

        public void Show(int finalScore)
        {
            gameObject.SetActive(true);
            _scoreMessage.text = string.Format(kGameOverTextPattern, finalScore);
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
