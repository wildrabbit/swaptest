using Game.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    public class GameplayAudioController : BaseAudioController
    {
        [Header("Gameplay-specific settings")]
        [SerializeField] AudioClip _swapStart;
        [SerializeField] AudioClip _swapFailed;

        protected override void Awake()
        {
            base.Awake();
            var viewEvents = GameEvents.Instance.View;
            viewEvents.SwapAttemptStarted += OnSwapStarted;
            viewEvents.FailedSwapAttempt += OnSwapFailed;
        }

        protected void OnDestroy()
        {
            base.OnDestroy();
            var viewEvents = GameEvents.Instance.View;
            viewEvents.SwapAttemptStarted -= OnSwapStarted;
            viewEvents.FailedSwapAttempt -= OnSwapFailed;
        }

        private void OnSwapStarted()
        {
            _audioSource.PlayOneShot(_swapStart);
        }

        private void OnSwapFailed()
        {
            _audioSource.PlayOneShot(_swapFailed);
        }
    }
}
