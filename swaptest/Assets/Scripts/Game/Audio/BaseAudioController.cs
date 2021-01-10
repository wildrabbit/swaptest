using Game.Events;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// This class handles sound and music playback, and tracks common events to trigger the 
    /// appropriate sfx
    /// </summary>
    public class BaseAudioController: MonoBehaviour
    {
        [Header("Shared properties")]
        [SerializeField] protected AudioClip _uiButtonTap;

        [SerializeField] protected AudioSource _audioSource;
        [SerializeField] protected AudioSource _musicSource;

        [SerializeField] protected bool _musicOn;
        [SerializeField] protected bool _sfxOn;

        public bool MusicOn => _musicOn;
        public bool SFXOn => _sfxOn;

        protected virtual void Awake()
        {
            GameEvents.Instance.UI.ButtonTapped += OnButtonTapped;
        }

        // Update is called once per frame
        protected virtual void OnDestroy()
        {
            GameEvents.Instance.UI.ButtonTapped -= OnButtonTapped;
        }

        protected void OnButtonTapped()
        {
            _audioSource.PlayOneShot(_uiButtonTap);
        }

        public bool ToggleMusic()
        {
            _musicOn = !_musicOn;
            if(_musicOn)
            {
                _musicSource.Play();
            }
            else
            {
                _musicSource.Stop();
            }
            return _musicOn;
        }

        public bool ToggleSfx()
        {
            _sfxOn = !_sfxOn;
            if (_sfxOn)
            {
                _audioSource.Play();
            }
            else if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            return _musicOn;
        }
    }
}
