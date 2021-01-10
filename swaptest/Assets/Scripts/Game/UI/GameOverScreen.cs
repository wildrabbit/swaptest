using Game.Events;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        const string kGameOverTextPattern = "You scored {0} points. \nPlay again to beat the high score!";
        const string kGameOverHighScorePattern = "You beat the high score with {0} points! :D";
        [SerializeField] TMP_Text _scoreMessage;
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

        public void Show(int finalScore, bool isHighScore)
        {
            gameObject.SetActive(true);
            _scoreMessage.text = string.Format(isHighScore ? kGameOverHighScorePattern : kGameOverTextPattern, finalScore);
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
