using UnityEngine;
using System.Collections;
using View;
using System;

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
        Input.simulateMouseWithTouches = true;
        _boardView.BoardUpdateCompleted += OnBoardUpdateCompleted;
        _boardView.BoardUpdateStarted += OnBoardUpdateStarted;
        _boardView.SwapAttemptStarted += OnSwapAttemptStarted;
        _boardView.FailedSwapAttempt += OnFailedSwapAttempt;
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
        if(!_enabled)
        {
            return;
        }

        if(Input.touchCount == 0)
        {
            if(Input.GetMouseButtonDown(kLeftMouseID))
            {
                HandleTouch(kMouseTouchID, Input.mousePosition, TouchPhase.Began);
            }
            if (Input.GetMouseButton(kLeftMouseID))
            {
                HandleTouch(kMouseTouchID, Input.mousePosition, TouchPhase.Moved);
            }
            if (Input.GetMouseButtonUp(kLeftMouseID))
            {
                HandleTouch(kMouseTouchID, Input.mousePosition, TouchPhase.Ended);
            }
        }
        else
        {
            var touch = Input.GetTouch(0);
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
        if(_selectedPiece != null && _selectedPiece != piece)
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
