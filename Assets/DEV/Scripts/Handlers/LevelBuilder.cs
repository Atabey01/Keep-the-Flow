using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.GamePlay;
using DEV.Scripts.Managers;
using Dreamteck.Splines;
using System.Collections.Generic;
using DEV.Scripts.Enums;
using UnityEngine;
using CannonColumn = DEV.Scripts.GamePlay.CannonColumn;

namespace DEV.Scripts.Handlers
{
    public class LevelBuilder
    {
        private LevelData _levelData;
        private GameConfig _gameConfig;
        
        public GameObject LevelParent;
        
        // Parent GameObjects
        private GameObject _conveyorParent;
        private GameObject _boxesParent;
        private GameObject _cannonsParent;
        
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
            if (LevelParent == null)
            {
                Debug.LogError("LevelBuilder: LevelParent null!");
                return;
            }
            
            // Conveyor parent
            _conveyorParent = new GameObject("Conveyors");
            _conveyorParent.transform.SetParent(LevelParent.transform);
            _conveyorParent.transform.localPosition = _gameConfig.GameAssetsConfig.ConveyorParentPosition;
            
            // Boxes parent
            _boxesParent = new GameObject("Boxes");
            _boxesParent.transform.SetParent(LevelParent.transform);
            _boxesParent.transform.localPosition = _gameConfig.GameAssetsConfig.ConveyorParentPosition;
            
            // Cannons parent
            _cannonsParent = new GameObject("Cannons");
            _cannonsParent.transform.SetParent(LevelParent.transform);
            _cannonsParent.transform.localPosition = _gameConfig.GameAssetsConfig.CannonParentPosition;
        }

        private void CreateCannons(LevelData levelData)
        {
            if (levelData == null || levelData.cannonColumns == null || levelData.cannonColumns.Count == 0)
            {
                Debug.LogWarning("LevelBuilder: Cannon oluşturmak için cannonColumns verisi yok!");
                return;
            }
            
            var cannonPrefab = _gameConfig.GameAssetsConfig.cannonPrefab;
            if (cannonPrefab == null)
            {
                Debug.LogError("LevelBuilder: CannonPrefab bulunamadı!");
                return;
            }
            
            // Grid boyutlarını hesapla
            int gridRowCount = levelData.cannonColumnGridRowCount;
            int horizontalColumnCount = gridRowCount > 0 ? levelData.cannonColumns.Count / gridRowCount : levelData.cannonColumns.Count;
            if (horizontalColumnCount == 0) horizontalColumnCount = 1;
            
            // Her column için (grid düzeninde)
            for (int columnIndex = 0; columnIndex < levelData.cannonColumns.Count; columnIndex++)
            {
                var columnData = levelData.cannonColumns[columnIndex];
                if (columnData == null || columnData.colors == null)
                    continue;
                
                // Grid pozisyonunu hesapla (row ve col)
                int gridRow = columnIndex / horizontalColumnCount;
                int gridCol = columnIndex % horizontalColumnCount;
                
                // CannonColumn GameObject oluştur (prefab yok, runtime'da oluşturuyoruz)
                GameObject columnGameObject = new GameObject($"CannonColumn_{columnIndex + 1}_R{gridRow + 1}C{gridCol + 1}");
                columnGameObject.transform.SetParent(_cannonsParent.transform);
                
                // Grid pozisyonunu hesapla (local space)
                // X ekseni: gridCol ile yatay dizilim
                float xPos = gridCol * GRID_CELL_SIZE;
                
                // Grid merkezinden X offset hesapla (local space)
                float gridCenterX = (horizontalColumnCount - 1) * GRID_CELL_SIZE * 0.5f;
                
                Vector3 columnOffset = _gameConfig.GameAssetsConfig.CannonColumnOffset;
                float zOffset = 1f + columnOffset.z; // Offset 0 ise 1, offset 0.5 ise 1.5
                float zPos = -(zOffset * gridRow); // Negatif yönde (aşağı doğru)
                
                // Base pozisyon
                Vector3 basePosition = new Vector3(
                    xPos - gridCenterX,
                    0f,
                    zPos
                );
                
                // CannonColumnOffset ekle (X ekseninde offset)
                Vector3 offsetPosition = new Vector3(
                    columnOffset.x * gridCol,
                    columnOffset.y,
                    0f // Z offset zaten zPos'da hesaplandı
                );
                Vector3 localPosition = basePosition + offsetPosition;
                
                columnGameObject.transform.localPosition = localPosition;
                
                // CannonColumn component ekle
                CannonColumn cannonColumn = columnGameObject.AddComponent<CannonColumn>();
                
                // Column içindeki her renk tipi için Cannon oluştur
                for (int colorIndex = 0; colorIndex < columnData.colors.Count; colorIndex++)
                {
                    ColorType colorType = columnData.colors[colorIndex];
                    
                    // ColorType.None ise atla
                    if (colorType == ColorType.None)
                        continue;
                    
                    // Factory ile Cannon oluştur
                    Cannon cannon = Factory.Create<Cannon>(cannonPrefab.gameObject, columnGameObject.transform, usePooling: false);
                    if (cannon == null)
                    {
                        Debug.LogError($"LevelBuilder: Cannon oluşturulamadı! Column: {columnIndex}, Color: {colorType}");
                        continue;
                    }
                    
                    // Pozisyon hesapla (color index'e göre yukarı aşağı, local space)
                    float yPos = colorIndex * GRID_CELL_SIZE;
                    
                    Vector3 cannonLocalPosition = new Vector3(
                        0f,
                        yPos,
                        0f
                    );
                    
                    cannon.transform.localPosition = cannonLocalPosition;
                    cannon.name = $"Cannon_{columnIndex + 1}_{colorIndex + 1}_{colorType}";
                    
                    // Cannon'u column'un listesine ekle
                    cannonColumn.cannons.Add(cannon);
                }
                
                // CannonColumn'u initialize et
                cannonColumn.Initialize();
            }
            
            Debug.Log($"LevelBuilder: Cannon'lar oluşturuldu! Toplam {levelData.cannonColumns.Count} column ({horizontalColumnCount}x{gridRowCount} grid).");
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
                Cube cube = Factory.Create<Cube>(boxPrefab.gameObject, _boxesParent?.transform, usePooling: false);
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
                
                Vector3 localPosition = new Vector3(
                    xPos - gridCenterX,
                    0f,
                    zPos - gridCenterZ
                );
                
                cube.transform.localPosition = localPosition;
                cube.name = $"Box_{cellData.gridPosition.x}_{cellData.gridPosition.y}";
                
                // Initialize çağır (CellData'dan direkt ColorType geçiyoruz)
                cube.Initialize(cellData.colorType, levelData, _gameConfig);
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
            SplineComputer splineComputer = Factory.Create<SplineComputer>(prefabGameObject, _conveyorParent?.transform, usePooling: false);
            if (splineComputer == null)
            {
                Debug.LogError("LevelBuilder: Conveyor oluşturulamadı!");
                return;
            }
            
            splineComputer.name = "Conveyor";
            splineComputer.transform.localPosition = Vector3.zero;
            
            // Grid koordinatlarını SplinePoint'lere çevir
            List<SplinePoint> splinePoints = new List<SplinePoint>();
            
            int gridWidth = levelData.gridSutunSayisi;
            int gridHeight = levelData.gridSatirSayisi;
            
            foreach (var gridPoint in levelData.conveyorPoints)
            {
                // Grid koordinatlarını local pozisyona çevir
                float xPos = gridPoint.x * GRID_CELL_SIZE;
                float zPos = gridPoint.y * GRID_CELL_SIZE;
                
                // Grid merkezinden offset hesapla (local space)
                float gridCenterX = (gridWidth - 1) * GRID_CELL_SIZE * 0.5f;
                float gridCenterZ = (gridHeight - 1) * GRID_CELL_SIZE * 0.5f;
                
                // Local space pozisyonu (Y ekseni 0'da, XZ düzleminde)
                Vector3 localPos = new Vector3(
                    xPos - gridCenterX,
                    0f,
                    zPos - gridCenterZ
                );
                
                // SplinePoint oluştur (zaten local space'de)
                SplinePoint point = new SplinePoint(localPos);
                splinePoints.Add(point);
            }
            
            // SplineComputer'a noktaları ayarla (Local space)
            splineComputer.SetPoints(splinePoints.ToArray(), SplineComputer.Space.Local);
            
            Debug.Log($"LevelBuilder: Conveyor oluşturuldu! {splinePoints.Count} nokta eklendi.");
        }
    }
}
