using UnityEngine;
using Game.Events;
using Game.Board;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Game.View
{
    public class ScoreFeedbackController : MonoBehaviour
    {
        [SerializeField] ScoreFeedback _scoreTextPrefab;
        [SerializeField, FormerlySerializedAs("_view")] BoardView _boardView;

        // TODO: Pool instances
        List<ScoreFeedback> _currentInstances;

        void Awake()
        {
            GameEvents.Instance.Gameplay.MatchProcessed += OnMatchProcessed;
            GameEvents.Instance.Gameplay.GameFinished += OnGameFinished;
            _currentInstances = new List<ScoreFeedback>();
        }

        void OnDestroy()
        {
            GameEvents.Instance.Gameplay.MatchProcessed -= OnMatchProcessed;
            GameEvents.Instance.Gameplay.GameFinished -= OnGameFinished;
        }

        private void OnGameFinished(int obj)
        {
            foreach(var instance in _currentInstances)
            {
                instance.Kill();
                GameObject.Destroy(instance.gameObject);
            }
            _currentInstances.Clear();
        }

        void OnMatchProcessed(MatchInfo matchInfo, int score, int multiplier)
        {
            ScoreFeedback instance = Instantiate(_scoreTextPrefab);
            instance.transform.position = GetMatchPosition(matchInfo);
            instance.Init(score, multiplier, OnKilled);
            _currentInstances.Add(instance);
        }

        private Vector3 GetMatchPosition(MatchInfo matchInfo)
        {
            var sorted = new List<Vector2Int>(matchInfo.MatchCoords);
            sorted.Sort(CompareCoords);
            switch(matchInfo.MatchType)
            {
                case MatchType.Match3:
                case MatchType.Match5:
                {
                    if(_boardView.TryGetPieceView(sorted[sorted.Count / 2], out var piece))
                    {
                        return piece.transform.position;
                    }
                    break;
                }
                case MatchType.Match4:
                {
                    _boardView.TryGetPieceView(sorted[1], out var piece1);
                    _boardView.TryGetPieceView(sorted[2], out var piece2);
                    if(piece1 != null && piece2 != null)
                    {
                        return 0.5f * (piece1.transform.position + piece2.transform.position);
                    }
                    break;
                }
            }
            Debug.LogError("Error locating pieces in match");
            return Vector3.zero;
        }

        private int CompareCoords(Vector2Int c1, Vector2Int c2)
        {
            var rowCompare = c1.x.CompareTo(c2.x);
            if(rowCompare == 0)
            {
                return c1.y.CompareTo(c2.y);
            }
            return rowCompare;
        }

        void OnKilled(ScoreFeedback destroyedItem)
        {
            GameObject.Destroy(destroyedItem.gameObject);
            _currentInstances.Remove(destroyedItem);
        }
    }
}
