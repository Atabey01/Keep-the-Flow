using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Sirenix.OdinInspector;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

namespace DEV.Scripts.GamePlay
{
    public class SplineObject : MonoBehaviour
    {
        [Title("Spline Settings")]
        [SerializeField] private SplineContainer splineContainer;
        
        [Title("Spline Visualization")]
        [SerializeField] private Color splineColor = Color.cyan;
        [SerializeField] private float splineThickness = 0.1f;
        [SerializeField] private int splineResolution = 50;
        
        [Title("Grid Settings")]
        [InfoBox("Grid boyutunu ayarlayın. Grid'de hücrelere tıklayarak spline noktaları seçin.")]
        [SerializeField, MinValue(2), MaxValue(20)] 
        private int gridWidth = 10;
        
        [SerializeField, MinValue(2), MaxValue(20)] 
        private int gridHeight = 10;
        
        [Title("Grid Scale")]
        [InfoBox("Grid'in dünya uzayındaki boyutunu ayarlayın")]
        [SerializeField, MinValue(0.1f)] 
        private float gridScale = 1f;
        
        [Title("Spline Points")]
        [InfoBox("Seçilen hücreler")]
        [SerializeField, HideInInspector] private List<Vector2Int> selectedCells = new List<Vector2Int>();
        
#if UNITY_EDITOR
        [Title("Grid Çizim Alanı")]
        [OnInspectorGUI("DrawSplineGrid", append: false)]
        [HideLabel]
        [ShowInInspector, ReadOnly]
        private string _splineGridPlaceholder = "";
        
        private const float GRID_DISPLAY_SIZE = 400f;
#endif
        
        public SplineContainer SplineContainer => splineContainer;
        public Color SplineColor => splineColor;
        public float SplineThickness => splineThickness;
        public int SplineResolution => splineResolution;
        public List<Vector2Int> SelectedCells => selectedCells;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float GridScale => gridScale;
        
        public void ToggleCell(Vector2Int cell)
        {
            if (selectedCells.Contains(cell))
                selectedCells.Remove(cell);
            else
                selectedCells.Add(cell);
        }
        
        public void ClearSelectedCells()
        {
            selectedCells.Clear();
        }
        
        public bool IsCellSelected(Vector2Int cell)
        {
            return selectedCells.Contains(cell);
        }
        
        [Button("Temizle", ButtonSizes.Medium)]
        [HorizontalGroup("Controls")]
        private void ClearSelectedCellsButton()
        {
            #if UNITY_EDITOR
            Undo.RecordObject(this, "Clear Selected Cells");
            #endif
            ClearSelectedCells();
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        
        [Button("Oluştur", ButtonSizes.Medium)]
        [HorizontalGroup("Controls")]
        [EnableIf("@selectedCells != null && selectedCells.Count >= 2")]
        private void CreateSplineButton()
        {
            #if UNITY_EDITOR
            Undo.RecordObject(this, "Create Spline");
            #endif
            CreateSplineFromCells();
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        
#if UNITY_EDITOR
        private void DrawSplineGrid()
        {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.Title("Grid Çizim Alanı", "", TextAlignment.Center, false);
            
            EditorGUILayout.Space(5);
            SirenixEditorGUI.InfoMessageBox("Grid'de hücrelere tıklayarak seçin. 'Oluştur' butonuna basarak spline'ı oluşturun.");
            EditorGUILayout.Space(5);
            
            // Grid alanı için rect al
            Rect gridArea = GUILayoutUtility.GetRect(GRID_DISPLAY_SIZE, GRID_DISPLAY_SIZE);
            
            // Grid arka planı
            EditorGUI.DrawRect(gridArea, new Color(0.15f, 0.15f, 0.15f, 1f));
            
            // Hücre boyutlarını hesapla
            float cellWidth = gridArea.width / gridWidth;
            float cellHeight = gridArea.height / gridHeight;
            
            // Event kontrolü - tıklama algılama
            Event currentEvent = Event.current;
            bool cellClicked = false;
            Vector2Int clickedCell = Vector2Int.zero;
            
            // Tıklama algılama - önce event'i kontrol et
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                if (gridArea.Contains(currentEvent.mousePosition))
                {
                    // Tıklanan hücreyi hesapla
                    int clickedCol = Mathf.FloorToInt((currentEvent.mousePosition.x - gridArea.x) / cellWidth);
                    int clickedRow = Mathf.FloorToInt((currentEvent.mousePosition.y - gridArea.y) / cellHeight);
                    
                    // Sınır kontrolü
                    clickedCol = Mathf.Clamp(clickedCol, 0, gridWidth - 1);
                    clickedRow = Mathf.Clamp(clickedRow, 0, gridHeight - 1);
                    
                    clickedCell = new Vector2Int(clickedCol, clickedRow);
                    cellClicked = true;
                    currentEvent.Use();
                }
            }
            
            // Her hücreyi çiz
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    Vector2Int cell = new Vector2Int(col, row);
                    Rect cellRect = new Rect(
                        gridArea.x + col * cellWidth,
                        gridArea.y + row * cellHeight,
                        cellWidth,
                        cellHeight
                    );
                    
                    // Hücre rengi (seçiliyse sarı, değilse koyu gri)
                    Color cellColor = IsCellSelected(cell) 
                        ? new Color(1f, 0.8f, 0f, 0.8f) 
                        : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                    
                    // Hücre arka planını çiz
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Hücre sınırları
                    Color borderColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), borderColor); // Üst
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), borderColor); // Sol
                    
                    // Seçili hücrelerde işaret
                    if (IsCellSelected(cell))
                    {
                        float centerX = cellRect.x + cellRect.width * 0.5f;
                        float centerY = cellRect.y + cellRect.height * 0.5f;
                        float radius = Mathf.Min(cellWidth, cellHeight) * 0.25f;
                        
                        // Sarı daire çiz
                        EditorGUI.DrawRect(
                            new Rect(centerX - radius, centerY - radius, radius * 2, radius * 2),
                            Color.yellow
                        );
                    }
                }
            }
            
            // Tıklama işlemini uygula
            if (cellClicked)
            {
                Undo.RecordObject(this, "Toggle Cell Selection");
                ToggleCell(clickedCell);
                EditorUtility.SetDirty(this);
                GUI.changed = true;
                
                // Inspector'ı yeniden çiz
                if (EditorWindow.focusedWindow != null)
                {
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            
            // Sağ ve alt sınırları çiz
            EditorGUI.DrawRect(new Rect(gridArea.x, gridArea.yMax - 1, gridArea.width, 1), new Color(0.5f, 0.5f, 0.5f, 1f)); // Alt
            EditorGUI.DrawRect(new Rect(gridArea.xMax - 1, gridArea.y, 1, gridArea.height), new Color(0.5f, 0.5f, 0.5f, 1f)); // Sağ
            
            // Dış sınır (cyan)
            Color borderColorCyan = Color.cyan;
            float borderThickness = 2f;
            EditorGUI.DrawRect(new Rect(gridArea.x, gridArea.y, borderThickness, gridArea.height), borderColorCyan); // Sol
            EditorGUI.DrawRect(new Rect(gridArea.xMax - borderThickness, gridArea.y, borderThickness, gridArea.height), borderColorCyan); // Sağ
            EditorGUI.DrawRect(new Rect(gridArea.x, gridArea.y, gridArea.width, borderThickness), borderColorCyan); // Üst
            EditorGUI.DrawRect(new Rect(gridArea.x, gridArea.yMax - borderThickness, gridArea.width, borderThickness), borderColorCyan); // Alt
            
            // Seçili hücre sayısı bilgisi
            if (selectedCells != null && selectedCells.Count > 0)
            {
                EditorGUILayout.Space(5);
                SirenixEditorGUI.InfoMessageBox($"Seçili Hücre Sayısı: {selectedCells.Count}");
            }
            
            SirenixEditorGUI.EndBox();
        }
#endif
        
        private void Awake()
        {
            if (splineContainer == null)
            {
                splineContainer = GetComponent<SplineContainer>();
                if (splineContainer == null)
                {
                    splineContainer = gameObject.AddComponent<SplineContainer>();
                }
            }
        }
        
        /// <summary>
        /// Seçili hücrelerden spline oluşturur
        /// </summary>
        public void CreateSplineFromCells()
        {
            if (selectedCells == null || selectedCells.Count < 2)
            {
                Debug.LogWarning("En az 2 hücre seçilmiş olmalı!");
                return;
            }
            
            if (splineContainer == null)
            {
                splineContainer = gameObject.AddComponent<SplineContainer>();
            }
            
            var spline = splineContainer.Spline;
            spline.Clear();
            
            // Seçili hücreleri dünya koordinatlarına çevir
            foreach (var cell in selectedCells)
            {
                // Hücre koordinatlarını normalized pozisyona çevir (0-1 arası)
                float normalizedX = gridWidth > 1 ? (float)cell.x / (gridWidth - 1) : 0.5f;
                float normalizedZ = gridHeight > 1 ? (float)cell.y / (gridHeight - 1) : 0.5f;
                
                // Grid merkezinden offset hesapla (-0.5 ile 0.5 arası)
                float offsetX = (normalizedX - 0.5f) * gridScale * gridWidth;
                float offsetZ = (normalizedZ - 0.5f) * gridScale * gridHeight;
                
                // World space pozisyonu
                Vector3 worldPos = transform.position + new Vector3(offsetX, 0, offsetZ);
                
                // Local space'e çevir ve float3'e dönüştür
                float3 localPos = transform.InverseTransformPoint(worldPos);
                spline.Add(new BezierKnot(localPos));
            }
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}
