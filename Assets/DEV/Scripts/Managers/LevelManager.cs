using System;
using System.Collections.Generic;
using DEV.Scripts.Config;
using DEV.Scripts.Controllers;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using DEV.Scripts.Handlers;
using DEV.Scripts.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    public class LevelManager
    {
        private GameConfig _gameConfig;
        private InputHandler _inputHandler;
        private GameController _gameController;
        private Dictionary<Type, IController> _controllers;
        private bool _isInitialized;
        private LevelBuilder _levelBuilder;
        private GameObject _levelParent;

        public void Initialize(GameConfig gameConfig, InputHandler inputHandler)
        {
            if (gameConfig == null)
            {
                Debug.LogError("LevelManager.Initialize: gameConfig null!");
                return;
            }
            
            if (inputHandler == null)
            {
                Debug.LogError("LevelManager.Initialize: inputHandler null!");
                return;
            }
            
            _gameConfig = gameConfig;
            _inputHandler = inputHandler;
            _controllers = new Dictionary<Type, IController>();
            CreateControllers();
            AddListeners(_inputHandler);

            // Event subscription'ı en sona al (tüm hazırlıklar tamamlandıktan sonra)
            StateManager.OnGameStateChanged += OnGameStateChange;
            
            // State değişikliklerini en sona al
            StateManager.SetGameState(GameState.Pause);
            StateManager.SetGameState(GameState.Loading);
            StateManager.SetPopupState(PopupState.None);
        }

        private void StartNewLevel()
        {
            if (_gameConfig == null)
            {
                Debug.LogError("LevelManager.StartNewLevel: _gameConfig null!");
                return;
            }
            
            if (_gameConfig.GamePlayConfig == null)
            {
                Debug.LogError("LevelManager.StartNewLevel: GamePlayConfig null!");
                return;
            }
            
            if (_gameConfig.GamePlayConfig.LevelConfig == null)
            {
                Debug.LogError("LevelManager.StartNewLevel: LevelConfig null!");
                return;
            }
            
            var levelData = _gameConfig.GamePlayConfig.LevelConfig.GetLevel();
            
            if (levelData == null)
            {
                Debug.LogError("LevelManager: LevelData null! TestLevel veya Levels listesi kontrol edilmeli.");
                return;
            }
            
            _isInitialized = true;

            if (levelData.LevelDifficultyType == LevelDifficultyType.Hard)
            {
                StateManager.SetPopupState(PopupState.HardLevelPopUp);
            }

            var levelID = DataSaver.GetLevelId() + 1;
            
            _levelParent = new GameObject($"Level_{levelID}");
            
            _levelBuilder = new LevelBuilder
            {
                LevelParent = _levelParent
            };
            
            // GameController'a levelBuilder'ı set et
            if (_gameController != null)
            {
                _gameController.SetLevelBuilder(_levelBuilder);
            }
            
            StartControllers(levelData);
        }

        private void DestroyLevel()
        {
            _isInitialized = false;
            Factory.DestroyAll();
            DestroyControllers();
            
            if (_levelParent != null)
            {
                UnityEngine.Object.Destroy(_levelParent);
                _levelParent = null;
            }
        }

        private void OnGameStateChange(GameState state)
        {
            // Null kontrolleri
            if (_gameConfig == null || _gameConfig.GamePlayConfig == null || _gameConfig.GamePlayConfig.LevelConfig == null)
            {
                Debug.LogError("LevelManager.OnGameStateChange: GameConfig veya LevelConfig null!");
                return;
            }
            
            if (state == GameState.Start)
            {
                if (_isInitialized)
                {
                    DestroyLevel();
                }

                StartNewLevel();
            }
            else if (state == GameState.Restart && _isInitialized)
            {
                DestroyLevel();
                _isInitialized = false;
                StartLevel();
            }
            else
            {
                _isInitialized = false;
            }
        }

        #region LevelEvents

        private void StartLevel()
        {
            StateManager.SetGameState(GameState.Start);
        }

        public void LevelComplete()
        {
            StateManager.SetGameState(GameState.End);
            DataSaver.IncreaseLevelId();
            DOVirtual.DelayedCall(_gameConfig.GamePlayConfig.LevelCompleteDelay,
                () => { StateManager.SetPopupState(PopupState.LevelSuccessPopUp); });
        }

        public void LevelFailed()
        {
            RestartLevel();
            DOVirtual.DelayedCall(_gameConfig.GamePlayConfig.LevelFailDelay,
                () => { StateManager.SetPopupState(Enums.PopupState.LevelFailPopUp); });
        }

        private void RestartLevel()
        {
            StateManager.SetGameState(GameState.Restart);
        }

        #endregion

        private void StartInput()
        {
            // Logic to handle the start of input
        }


        void AddListeners(InputHandler inputHandler)
        {
            inputHandler.OnMouseDown += _gameController.MouseDown;
            inputHandler.OnMouseUp += _gameController.MouseUp;
            inputHandler.OnMouseDrag += _gameController.MouseDrag;
            inputHandler.StartInput += StartInput;
        }

        void RemoveListeners(InputHandler inputHandler)
        {
            if (_gameController != null)
            {
                inputHandler.OnMouseDown -= _gameController.MouseDown;
                inputHandler.OnMouseUp -= _gameController.MouseUp;
                inputHandler.OnMouseDrag -= _gameController.MouseDrag;
            }

            inputHandler.StartInput -= StartInput;
        }

        #region Controller Methods

        public void AddController<T>(T controller) where T : IController
        {
            _controllers.Add(typeof(T), controller);
        }

        public T GetController<T>() where T : IController
        {
            if (_controllers.TryGetValue(typeof(T), out var controller))
            {
                return (T)controller;
            }

            return default;
        }

        private void CreateControllers()
        {
            // _levelBuilder henüz null olabilir (StartNewLevel'da oluşturuluyor)
            // GameController'a null geçebiliriz, StartNewLevel'da yeniden set edilecek
            _gameController = new GameController(null);
            AddController(_gameController);
        }

        private void StartControllers(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("LevelManager.StartControllers: levelData null!");
                return;
            }
            
            if (_controllers == null)
            {
                Debug.LogError("LevelManager.StartControllers: _controllers null!");
                return;
            }
            
            if (_gameConfig == null)
            {
                Debug.LogError("LevelManager.StartControllers: _gameConfig null!");
                return;
            }
            
            foreach (var controller in _controllers)
            {
                if (controller.Value != null)
                {
                    controller.Value.StartNewLevel(levelData, _gameConfig);
                }
            }
        }

        private void DestroyControllers()
        {
            foreach (var controller in _controllers)
                controller.Value.LevelDestroy();
        }

        private void DisposeControllers()
        {
            foreach (var controller in _controllers)
                controller.Value.Dispose();
        }

        #endregion


        public void Dispose()
        {
            // Event'ten unsubscribe et
            StateManager.OnGameStateChanged -= OnGameStateChange;
            
            RemoveListeners(_inputHandler);
            DisposeControllers();
            
            // Clear references
            _gameConfig = null;
            _inputHandler = null;
            _controllers = null;
            _gameController = null;
            _levelBuilder = null;
        }
    }
}