using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroScreen : MonoBehaviour
{
    [SerializeField] Object _gameScene;
    [SerializeField] GameObject _exitButton;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_STANDALONE
        _exitButton.SetActive(true);
#endif
    }

    public void OnPlay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_gameScene.name);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
