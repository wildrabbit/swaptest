using UnityEngine;
using Game.View;
using UInput = UnityEngine.Input;
using Game.Events;
using System;

namespace Game.Input
{
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
            _boardView.BoardUpdateCompleted += OnBoardUpdateCompleted;
            _boardView.BoardUpdateStarted += OnBoardUpdateStarted;
            _boardView.SwapAttemptStarted += OnSwapAttemptStarted;
            _boardView.FailedSwapAttempt += OnFailedSwapAttempt;
        }

        public void Init()
        {
            GameController.GameEvents.GameFlow.GameStarted -= OnGameStarted;
            GameController.GameEvents.GameFlow.GameStarted += OnGameStarted;
            GameController.GameEvents.GameFlow.GameFinished -= OnGameFinished;
            GameController.GameEvents.GameFlow.GameFinished += OnGameFinished;
        }

        void OnGameStarted()
        {
            SetInputEnabled(true);
        }

        void OnGameFinished(int obj)
        {
            CancelSelection();
            SetInputEnabled(false);
        }

        private void OnSwapAttemptStarted()
        {
            CancelSelection();
            SetInputEnabled(false);
        }

        private void OnFailedSwapAttempt()
        {
            SetInputEnabled(true);
        }

        private void OnBoardUpdateCompleted()
        {
            SetInputEnabled(true);
        }

        private void OnBoardUpdateStarted()
        {
            SetInputEnabled(false);
        }

        void OnDestroy()
        {
            _boardView.BoardUpdateCompleted -= OnBoardUpdateCompleted;
            _boardView.BoardUpdateStarted -= OnBoardUpdateStarted;
            _boardView.SwapAttemptStarted -= OnSwapAttemptStarted;
            _boardView.FailedSwapAttempt -= OnFailedSwapAttempt;
        }

        // Update is called once per frame
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

        void SelectPiece(PieceView piece)
        {
            Debug.Log($"Selected piece @ {piece.Coords}");
            if (_selectedPiece != null && _selectedPiece != piece)
            {
                //_selectedPiece.Deselect();
                _selectedPiece = null;
            }
            _selectedPiece = piece;
            // _selectedPiece.Select();
        }

        void CancelSelection()
        {
            if (_selectedPiece != null)
            {
                _selectedPiece = null;
                //_selectedPiece.Deselect();
            }
        }


        public void SetInputEnabled(bool enabled)
        {
            _enabled = enabled;
        }
    }
}