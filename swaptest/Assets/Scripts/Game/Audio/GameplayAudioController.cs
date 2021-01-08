﻿using Game.Events;
using Game.View;
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
        [SerializeField] AudioClip _matchExplosion;
        [SerializeField] AudioClip _dropCompleted;
        [SerializeField] AudioClip _reshuffling;
        [SerializeField] AudioClip _timerRunning;
        [SerializeField] AudioClip _timerExpired;
        [SerializeField] AudioClip _gameFinished;
        [SerializeField] AudioClip _gameStarted;

        protected override void Awake()
        {
            base.Awake();

            var gameEvents = GameEvents.Instance;
            var viewEvents = gameEvents.View;
            viewEvents.SwapAttemptStarted += OnSwapStarted;
            viewEvents.FailedSwapAttempt += OnSwapFailed;
            viewEvents.PiecesExploded += OnPiecesExploded;
            viewEvents.Reshuffling += OnReshuffling;
            var boardEvents = gameEvents.Board;
            boardEvents.DropStepCompleted += OnDropCompleted;
            var gameplayEvents = gameEvents.Gameplay;
            gameplayEvents.GameStarted += OnGameStarted;
            gameplayEvents.GameFinished += OnGameFinished;
            gameplayEvents.TimerRunningOut += OnTimerRunningOut;
            gameplayEvents.TimerExpired += OnTimerExpired;

        }

        void OnTimerExpired()
        {
            _audioSource.PlayOneShot(_timerExpired);
        }

        void OnTimerRunningOut()
        {
            _audioSource.PlayOneShot(_timerRunning);
        }

        void OnGameFinished(int obj)
        {
            _audioSource.PlayOneShot(_gameFinished);
        }

        void OnGameStarted(int arg1, float arg2, float arg3)
        {
            _audioSource.PlayOneShot(_gameStarted);
        }

        protected void OnDestroy()
        {
            base.OnDestroy();
            var gameEvents = GameEvents.Instance;
            var viewEvents = gameEvents.View;
            viewEvents.SwapAttemptStarted -= OnSwapStarted;
            viewEvents.FailedSwapAttempt -= OnSwapFailed;
            viewEvents.PiecesExploded -= OnPiecesExploded;
            viewEvents.DropCompleted -= OnDropCompleted;
            viewEvents.Reshuffling -= OnReshuffling;

            var boardEvents = gameEvents.Board;
            boardEvents.DropStepCompleted -= OnDropCompleted;

            var gameplayEvents = gameEvents.Gameplay;
            gameplayEvents.GameStarted -= OnGameStarted;
            gameplayEvents.GameFinished -= OnGameFinished;
            gameplayEvents.TimerRunningOut -= OnTimerRunningOut;
            gameplayEvents.TimerExpired -= OnTimerExpired;
        }

        void OnSwapStarted()
        {
            _audioSource.PlayOneShot(_swapStart);
        }

        void OnSwapFailed()
        {
            _audioSource.PlayOneShot(_swapFailed);
        }

        void OnPiecesExploded(List<PieceView> pieces)
        {
            _audioSource.PlayOneShot(_matchExplosion);
        }

        void OnDropCompleted()
        {
            _audioSource.PlayOneShot(_dropCompleted);
        }

        void OnReshuffling()
        {
            _audioSource.PlayOneShot(_reshuffling);
        }
    }
}
