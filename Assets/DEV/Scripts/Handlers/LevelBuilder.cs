using DEV.Scripts.Config;
using DEV.Scripts.Data;
using UnityEngine;

namespace DEV.Scripts.Handlers
{
    public class LevelBuilder
    {
        private LevelData _levelData;
        private GameConfig _gameConfig;
        
        public GameObject LevelParent;
        
        public void StartNewLevel(LevelData levelData, GameConfig gameConfig)
        {
            _levelData = levelData;
            _gameConfig = gameConfig;
        }
    }
}
