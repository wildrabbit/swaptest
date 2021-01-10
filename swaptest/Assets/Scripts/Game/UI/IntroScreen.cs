using Game.Events;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Intro screen control class
    /// </summary>
    public class IntroScreen : MonoBehaviour
    {
        [SerializeField] string _gameScene;
        [SerializeField] GameObject _exitButton;

        void Start()
        {
#if UNITY_STANDALONE
            _exitButton.SetActive(true);
#endif
        }

        public void OnPlay()
        {

            GameEvents.Instance.UI.DispatchButtonTapped();
            UnityEngine.SceneManagement.SceneManager.LoadScene(_gameScene);
        }

        public void OnExit()
        {
            GameEvents.Instance.UI.DispatchButtonTapped();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
