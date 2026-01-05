using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.GamePlay;
using DEV.Scripts.Managers;
using Dreamteck.Splines;
using System.Collections.Generic;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Handlers
{
    public class LevelBuilder
    {
        private LevelData _levelData;
        private GameConfig _gameConfig;
        
        public GameObject LevelParent;
        
        private const float GRID_CELL_SIZE = 1f; // Her grid hücresi 1 unit
        
        public void StartNewLevel(LevelData levelData, GameConfig gameConfig)
        {
            _levelData = levelData;
            _gameConfig = gameConfig;
            
            CreateParents();
            CreateConveyor(levelData);
            CreateBoxes(levelData);
            CreateCannons(levelData);
        }

        private void CreateParents()
        {
            
        }

        private void CreateCannons(LevelData levelData)
        {
            
        }

        private void CreateBoxes(LevelData levelData)
        {
            if (levelData == null || levelData.cellDataList == null || levelData.cellDataList.Count == 0)
            {
                Debug.LogWarning("LevelBuilder: Box oluşturmak için cellDataList verisi yok!");
                return;
            }
            
            var boxPrefab = _gameConfig.GameAssetsConfig.cubePrefab;
            if (boxPrefab == null)
            {
                Debug.LogError("LevelBuilder: BoxPrefab bulunamadı!");
                return;
            }
            
            // cellDataList'ten box'ları oluştur
            foreach (var cellData in levelData.cellDataList)
            {
                if (cellData == null)
                    continue;
                
                // ColorType null ise atla
                if (cellData.colorType == ColorType.None)
                    continue;
                
                // Factory ile Box oluştur
                Cube cube = Factory.Create<Cube>(boxPrefab.gameObject, LevelParent?.transform, usePooling: false);
                if (cube == null)
                {
                    Debug.LogError($"LevelBuilder: Box oluşturulamadı! Grid Position: {cellData.gridPosition}");
                    continue;
                }
                
                // Grid pozisyonundan world pozisyonunu hesapla
                float xPos = cellData.gridPosition.x * GRID_CELL_SIZE;
                float zPos = cellData.gridPosition.y * GRID_CELL_SIZE;
                
                // Grid merkezinden offset hesapla
                float gridCenterX = (levelData.gridSutunSayisi - 1) * GRID_CELL_SIZE * 0.5f;
                float gridCenterZ = (levelData.gridSatirSayisi - 1) * GRID_CELL_SIZE * 0.5f;
                
                Vector3 position = new Vector3(
                    xPos - gridCenterX,
                    0f,
                    zPos - gridCenterZ
                );
                
                cube.transform.position = position;
                cube.name = $"Box_{cellData.gridPosition.x}_{cellData.gridPosition.y}";
                
                // BoxData oluştur ve Initialize çağır
                BoxData boxData = new BoxData(cellData.colorType);
                cube.Initialize(boxData, levelData, _gameConfig);
            }
            
            Debug.Log($"LevelBuilder: Box'lar oluşturuldu! Toplam {levelData.cellDataList.Count} cell data'dan box oluşturuldu.");
        }

        private void CreateConveyor(LevelData levelData)
        {
            if (levelData == null || levelData.conveyorPoints == null || levelData.conveyorPoints.Count < 2)
            {
                Debug.LogWarning("LevelBuilder: Conveyor için en az 2 nokta gerekli!");
                return;
            }
            
            var conveyorPrefab = _gameConfig.GameAssetsConfig.ConveyorPrefab;
            if (conveyorPrefab == null)
            {
                Debug.LogError("LevelBuilder: ConveyorPrefab bulunamadı!");
                return;
            }
            
            // Factory ile SplineComputer oluştur (prefab GameObject olmalı)
            GameObject prefabGameObject = conveyorPrefab.gameObject;
            SplineComputer splineComputer = Factory.Create<SplineComputer>(prefabGameObject, LevelParent?.transform, usePooling: false);
            if (splineComputer == null)
            {
                Debug.LogError("LevelBuilder: Conveyor oluşturulamadı!");
                return;
            }
            
            splineComputer.name = "Conveyor";
            splineComputer.transform.position = Vector3.zero;
            
            // Grid koordinatlarını SplinePoint'lere çevir
            List<SplinePoint> splinePoints = new List<SplinePoint>();
            
            int gridWidth = levelData.gridSutunSayisi;
            int gridHeight = levelData.gridSatirSayisi;
            
            foreach (var gridPoint in levelData.conveyorPoints)
            {
                // Grid koordinatlarını normalized pozisyona çevir (0-1 arası)
                float normalizedX = gridWidth > 1 ? (float)gridPoint.x / (gridWidth - 1) : 0.5f;
                float normalizedZ = gridHeight > 1 ? (float)gridPoint.y / (gridHeight - 1) : 0.5f;
                
                // Grid merkezinden offset hesapla
                float offsetX = (normalizedX - 0.5f) * GRID_CELL_SIZE * gridWidth;
                float offsetZ = (normalizedZ - 0.5f) * GRID_CELL_SIZE * gridHeight;
                
                // World space pozisyonu (Y ekseni 0'da, XZ düzleminde)
                Vector3 worldPos = new Vector3(offsetX, 0f, offsetZ);
                
                // Local space'e çevir (splineComputer'ın transform'una göre)
                Vector3 localPos = splineComputer.transform.InverseTransformPoint(worldPos);
                
                // SplinePoint oluştur
                SplinePoint point = new SplinePoint(localPos);
                splinePoints.Add(point);
            }
            
            // SplineComputer'a noktaları ayarla (Local space)
            splineComputer.SetPoints(splinePoints.ToArray(), SplineComputer.Space.Local);
            
            Debug.Log($"LevelBuilder: Conveyor oluşturuldu! {splinePoints.Count} nokta eklendi.");
        }
    }
}
