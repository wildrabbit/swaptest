using Game.Events;
using Game.View;
using UnityEngine;
using UInput = UnityEngine.Input;

namespace Game.Input
{
    /// <summary>
    /// This class tracks input events and communicates with the view in order to detect taps and swipes
    /// </summary>
    public class InputController : MonoBehaviour
    {
        const int kLeftMouseID = 0;
        const int kMouseTouchID = 10;

        [SerializeField] BoardView _boardView;
        [SerializeField] Camera _mainCamera;

        PieceView _selectedPiece;
        bool _enabled = true;

        void Awake()
        {
            UInput.simulateMouseWithTouches = true;
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        void Update()
        {
            if (!_enabled)
            {
                return;
            }

            if (UInput.touchCount == 0)
            {
                if (UInput.GetMouseButtonDown(kLeftMouseID))
                {
                    HandleTouch(kMouseTouchID, UInput.mousePosition, TouchPhase.Began);
                }
                if (UInput.GetMouseButton(kLeftMouseID))
                {
                    HandleTouch(kMouseTouchID, UInput.mousePosition, TouchPhase.Moved);
                }
                if (UInput.GetMouseButtonUp(kLeftMouseID))
                {
                    HandleTouch(kMouseTouchID, UInput.mousePosition, TouchPhase.Ended);
                }
            }
            else
            {
                var touch = UInput.GetTouch(0);
                HandleTouch(0, touch.position, touch.phase);
            }
        }

        void HandleTouch(int touchID, Vector2 screenPos, TouchPhase phase)
        {
            var worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            bool insideBounds = _boardView.PositionInsideBounds(worldPos);

            if (!insideBounds && _selectedPiece != null)
            {
                CancelSelection();
            }

            switch (phase)
            {
                case TouchPhase.Began:
                {
                    if (_boardView.TryGetPieceView(worldPos, out var piece))
                    {
                        if (_selectedPiece == null || !piece.IsAdjacentTo(_selectedPiece))
                        {
                            SelectPiece(piece);
                        }
                        else
                        {
                            _boardView.AttemptSwap(_selectedPiece, piece);
                        }
                    }
                    break;
                }
                case TouchPhase.Ended:
                case TouchPhase.Moved:
                {
                    if (_selectedPiece != null && _boardView.TryGetPieceView(worldPos, out var swapCandidatePiece) && swapCandidatePiece != _selectedPiece && swapCandidatePiece.IsAdjacentTo(_selectedPiece))
                    {
                        _boardView.AttemptSwap(_selectedPiece, swapCandidatePiece);
                    }
                    break;
                }
                case TouchPhase.Canceled:
                {
                    Debug.Log("Cancel!");

                    CancelSelection();
                    break;
                }
            }
        }

        void SubscribeToEvents()
        {
            var gameplayEvents = GameEvents.Instance.Gameplay;
            gameplayEvents.GameStarted += OnGameStarted;
            gameplayEvents.GameFinished += OnGameFinished;

            var viewEvents = GameEvents.Instance.View;
            viewEvents.BoardUpdateCompleted += OnBoardUpdateCompleted;
            viewEvents.BoardUpdateStarted += OnBoardUpdateStarted;
            viewEvents.SwapAttemptStarted += OnSwapAttemptStarted;
            viewEvents.FailedSwapAttempt += OnFailedSwapAttempt;
        }

        void UnsubscribeFromEvents()
        {
            var gameplayEvents = GameEvents.Instance.Gameplay;
            gameplayEvents.GameStarted -= OnGameStarted;
            gameplayEvents.GameFinished -= OnGameFinished;

            var viewEvents = GameEvents.Instance.View;
            viewEvents.BoardUpdateCompleted -= OnBoardUpdateCompleted;
            viewEvents.BoardUpdateStarted -= OnBoardUpdateStarted;
            viewEvents.SwapAttemptStarted -= OnSwapAttemptStarted;
            viewEvents.FailedSwapAttempt -= OnFailedSwapAttempt;
        }

        void OnGameStarted(int score, int highScore, float elapsedTime, float totalTime)
        {
            SetInputEnabled(true);
        }

        void OnGameFinished(int score, bool newHighScore, int highScore)
        {
            CancelSelection();
            SetInputEnabled(false);
        }

        void OnSwapAttemptStarted()
        {
            CancelSelection();
            SetInputEnabled(false);
        }

        void OnFailedSwapAttempt()
        {
            SetInputEnabled(true);
        }

        void OnBoardUpdateCompleted()
        {
            SetInputEnabled(true);
        }

        void OnBoardUpdateStarted()
        {
            SetInputEnabled(false);
        }

        void SelectPiece(PieceView piece)
        {
            if (_selectedPiece != piece)
            {
                CancelSelection();
            }
            _selectedPiece = piece;
            _selectedPiece.Select();
        }

        void CancelSelection()
        {
            if (_selectedPiece != null)
            {
                _selectedPiece.Deselect();
                _selectedPiece = null;
            }
        }

        void SetInputEnabled(bool enabled)
        {
            _enabled = enabled;
        }
    }
}