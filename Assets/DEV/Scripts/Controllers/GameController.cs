using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.GamePlay;
using DEV.Scripts.Handlers;
using DEV.Scripts.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using CannonColumn = DEV.Scripts.GamePlay.CannonColumn;

namespace DEV.Scripts.Controllers
{
    public class GameController : IController
    {
        private LevelBuilder _levelBuilder;
        private GameConfig _gameConfig;
        
        // Level referansları
        private ConveyorController _conveyorController;
        private List<CannonColumn> _cannonColumns = new List<CannonColumn>();

        public GameController(LevelBuilder levelBuilder)
        {
            _levelBuilder = levelBuilder;
        }
        
        public void SetLevelBuilder(LevelBuilder levelBuilder)
        {
            _levelBuilder = levelBuilder;
        }

        public void StartNewLevel(LevelData levelData, GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
            
            if (_levelBuilder == null)
            {
                Debug.LogError("GameController.StartNewLevel: _levelBuilder null!");
                return;
            }
            
            _levelBuilder.StartNewLevel(levelData, _gameConfig);
            
            // LevelBuilder'dan referansları al
            _conveyorController = _levelBuilder.GetConveyorController();
            _cannonColumns = _levelBuilder.GetCannonColumns();
        }
        
        public void MouseDown(GameObject clickedGameObject)
        {
            // Generic kullanım örneği:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // var box = clickedGameObject?.GetComponent<Box>();
            // if (cube != null) { /* Cube işlemleri */ }
            // if (box != null) { /* Box işlemleri */ }
        }
        
        public void MouseUp(GameObject clickedGameObject)
        {
            // Generic kullanım örneği:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // if (cube != null) { /* Cube işlemleri */ }
        }

        public void MouseDrag(GameObject clickedGameObject)
        {
            // Generic kullanım örneği:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // if (cube != null) { /* Cube işlemleri */ }
        }

        public void LevelDestroy()
        {
        }

        public void Dispose()
        {
            
        }
    }
}
