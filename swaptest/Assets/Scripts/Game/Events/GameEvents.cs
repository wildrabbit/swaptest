using Game.Board;
using Game.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Events
{
    public class GameplayEvents
    {
        public event Action<int, int, float, float> GameStarted;
        public event Action<int, bool, int> GameFinished;
        public event Action<int, int> ScoreChanged;
        public event Action<int> HighScoreChanged;
        public event Action<float, float> TimerChanged;
        public event Action TimerExpired;
        public event Action TimerRunningOut;
        public event Action<MatchInfo, int, int> MatchProcessed;
        public event Action SavedGame;
        public event Action LoadGame;
        public event Action ResetGameSave;

        public void DispatchGameStarted(int score, int highScore, float elapsed, float totalTime)
        {
            GameStarted?.Invoke(score, highScore, elapsed, totalTime);
        }
        public void DispatchGameFinished(int finalScore, bool isNewHighScore, int highScore)
        {
            GameFinished?.Invoke(finalScore, isNewHighScore, highScore);
        }
        public void DispatchScoreChanged(int scoreDelta, int totalScore)
        {
            ScoreChanged?.Invoke(scoreDelta, totalScore);
        }
        public void DispatchTimerChanged(float elapsed, float totalTime)
        {
            TimerChanged?.Invoke(elapsed, totalTime);
        }
        public void DispatchTimerExpired()
        {
            TimerExpired?.Invoke();
        }
        public void DispatchTimerRunningOut()
        {
            TimerRunningOut?.Invoke();
        }

        public void DispatchMatchProcessed(MatchInfo match, int score, int chainStep)
        {
            MatchProcessed?.Invoke(match, score, chainStep);
        }

        public void DispatchHighScoreChanged(int newHighScore)
        {
            HighScoreChanged?.Invoke(newHighScore);
        }

        public void DispatchSaved()
        {
            SavedGame?.Invoke();
        }

        public void DispatchLoad()
        {
            LoadGame?.Invoke();
        }

        public void DispatchResetGameSave()
        {
            ResetGameSave?.Invoke();
        }
    }

    public class UIEvents
    {
        public event Action<bool> StartGameRequested;
        public event Action ResetSaveRequested;
        public event Action ButtonTapped;
        public event Action SFXToggle;
        public event Action MusicToggle;

        public void DispatchStartGameRequested(bool isRestart)
        {
            StartGameRequested?.Invoke(isRestart);
        }

        public void DispatchResetSaveRequest()
        {
            ResetSaveRequested?.Invoke();
        }
        
        public void DispatchButtonTapped()
        {
            ButtonTapped?.Invoke();
        }

        public void DispatchSFXToggle()
        {
            SFXToggle?.Invoke();
        }

        public void DispatchMusicToggle()
        {
            MusicToggle?.Invoke();
        }
    }

    public class BoardEvents
    {
        public event Action<List<MatchInfo>, int> MatchesFound;
        public event Action DropStepCompleted;

        public void DispatchMatchesFound(List<MatchInfo> matches, int chainStep)
        {
            MatchesFound?.Invoke(matches, chainStep);
        }
        public void DispatchDropStepCompleted()
        {
            DropStepCompleted?.Invoke();
        }
    }

    public class ViewEvents
    {
        public event Action BoardUpdateCompleted;
        public event Action BoardUpdateStarted;
        public event Action SwapAttemptStarted;
        public event Action FailedSwapAttempt;
        public event Action<Vector2Int, Vector2Int> SwapAnimationCompleted;
        public event Action<List<PieceView>, int> PiecesExploded;
        public event Action DropCompleted;
        public event Action Reshuffling;

        public void DispatchBoardUpdateCompleted()
        {
            BoardUpdateCompleted?.Invoke();
        }

        public void DispatchBoardUpdateStarted()
        {
            BoardUpdateStarted?.Invoke();
        }

        public void DispatchSwapAttemptStarted()
        {
            SwapAttemptStarted?.Invoke();
        }

        public void DispatchFailedSwapAttempt()
        {
            FailedSwapAttempt?.Invoke();
        }

        public void DispatchSwapAnimationCompleted(Vector2Int sourceCoords, Vector2Int targetCoords)
        {
            SwapAnimationCompleted?.Invoke(sourceCoords, targetCoords);
        }

        public void DispatchPiecesExploded(List<PieceView> pieces, int chainStep)
        {
            PiecesExploded?.Invoke(pieces, chainStep);
        }

        public void DispatchDropCompleted()
        {
            DropCompleted?.Invoke();
        }

        public void DispatchReshuffling()
        {
            Reshuffling?.Invoke();
        }
    }

    public class GameEvents
    {
        public GameplayEvents Gameplay = new GameplayEvents();
        public UIEvents UI = new UIEvents();
        public BoardEvents Board = new BoardEvents();
        public ViewEvents View = new ViewEvents();

        static GameEvents _instance;
        public static GameEvents Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameEvents();
                }
                return _instance;
            }
        }
    }
}
