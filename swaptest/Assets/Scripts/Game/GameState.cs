using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class GameState
    {
        [System.Serializable]
        public class HighScoreEntry
        {
            public string LevelDataName;
            public int HighScore;
        }

        [SerializeField] List<HighScoreEntry> _highScoreTable = new List<HighScoreEntry>();
        string _savePath;

        public GameState(string savePath)
        {
            _savePath = savePath;
        }

        public int GetHighScoreForLevel(string name)
        {
            var existingEntry = _highScoreTable.Find(entry => entry.LevelDataName.Equals(name));
            return existingEntry == null ? -1 : existingEntry.HighScore;
        }

        public void AddHighScoreEntry(string name, int value)
        {
            var existingEntry = _highScoreTable.Find(entry => entry.LevelDataName.Equals(name));
            if (existingEntry == null)
            {
                _highScoreTable.Add(new HighScoreEntry
                {
                    LevelDataName = name,
                    HighScore = value
                });
            }
            else
            {
                existingEntry.HighScore = value;
            }
        }

        public void Load()
        {
            if (!string.IsNullOrEmpty(_savePath) && File.Exists(_savePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(_savePath, FileMode.Open);
                CopyFrom((GameState)bf.Deserialize(file));
                file.Close();
            }
            else
            {
                _highScoreTable.Clear();
            }
        }

        private void CopyFrom(GameState gameState)
        {
            _highScoreTable.Clear();
            _highScoreTable.AddRange(gameState._highScoreTable);
        }

        public void Save()
        {
            if(string.IsNullOrEmpty(_savePath))
            {
                Debug.LogError("Tried to save game state @ empty path");
                return;
            }
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(_savePath);
            bf.Serialize(file, this);
            file.Close();
        }

        public void Delete()
        {
            if (!string.IsNullOrEmpty(_savePath) && File.Exists(_savePath))
            {
                File.Delete(_savePath);
            }
            _highScoreTable.Clear();
        }
    }
}
