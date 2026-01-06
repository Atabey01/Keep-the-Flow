using System.Collections.Generic;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using Dreamteck.Splines;
using UnityEngine;

namespace DEV.Scripts.GamePlay
{
    public class ConveyorController : MonoBehaviour
    {
        [Header("References")]
        public SplineComputer splineComputer;
        
        [Header("Settings")]
        [Tooltip("Grid cell size used to convert LevelData grid coordinates to local positions")]
        public float gridCellSize = 1f;
        
        /// <summary>
        /// Initializes the conveyor spline using LevelData and GameConfig.
        /// </summary>
        public void Initialize(LevelData levelData, GameConfig gameConfig)
        {
            if (levelData == null || levelData.conveyorPoints == null || levelData.conveyorPoints.Count < 2)
            {
                Debug.LogWarning("ConveyorController: Conveyor için en az 2 nokta gerekli!");
                return;
            }
            
            if (splineComputer == null)
            {
                splineComputer = GetComponent<SplineComputer>();
            }

            if (splineComputer == null)
            {
                Debug.LogError("ConveyorController: SplineComputer referansı bulunamadı!");
                return;
            }

            splineComputer.transform.localPosition = Vector3.zero;
            
            // Grid koordinatlarını SplinePoint'lere çevir (local space)
            List<SplinePoint> splinePoints = new List<SplinePoint>();
            
            int gridWidth = levelData.gridSutunSayisi;
            int gridHeight = levelData.gridSatirSayisi;
            
            foreach (var gridPoint in levelData.conveyorPoints)
            {
                // Grid koordinatlarını local pozisyona çevir
                float xPos = gridPoint.x * gridCellSize;
                float zPos = gridPoint.y * gridCellSize;
                
                // Grid merkezinden offset hesapla (local space)
                float gridCenterX = (gridWidth - 1) * gridCellSize * 0.5f;
                float gridCenterZ = (gridHeight - 1) * gridCellSize * 0.5f;
                
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
            
            Debug.Log($"ConveyorController: Conveyor oluşturuldu! {splinePoints.Count} nokta eklendi.");
        }
    }
}
