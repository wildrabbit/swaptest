using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Board;
using Game.View;

namespace Game.Events
{
    public class GameplayEvents
    {
        public event Action<int, float, float> GameStarted;
        public event Action<int> GameFinished;
        public event Action<int, int> ScoreChanged;
        public event Action<float, float> TimerChanged;
        public event Action TimerExpired;
        public event Action TimerRunningOut;
        public event Action<MatchInfo, int, int> MatchProcessed;

        public void DispatchGameStarted(int score, float elapsed, float totalTime)
        {
            GameStarted?.Invoke(score, elapsed, totalTime);
        }
        public void DispatchGameFinished(int finalScore)
        {
            GameFinished?.Invoke(finalScore);
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
    }

    public class UIEvents
    {
        public event Action<bool> StartGameRequested;
        public event Action ButtonTapped;

        public void DispatchStartGameRequested(bool isRestart)
        {
            StartGameRequested?.Invoke(isRestart);
        }
        
        public void DispatchButtonTapped()
        {
            ButtonTapped?.Invoke();
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
        public event Action<List<PieceView>> PiecesExploded;
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

        public void DispatchPiecesExploded(List<PieceView> pieces)
        {
            PiecesExploded?.Invoke(pieces);
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
