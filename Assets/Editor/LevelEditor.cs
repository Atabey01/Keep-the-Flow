using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;

namespace DEV.Editor
{
    public class LevelEditor : EditorWindow
    {
        private const int TOOLBAR_HEIGHT = 35;
        private const int BUTTON_HEIGHT = 30;
        private const int SPACING = 10;

        private static LevelEditor instance;
        private LevelConfig levelConfig;
        private string levelConfigPath = "Assets/DEV/Data/Config/LevelConfig.asset";
        
        private LevelListPanel levelListPanel;
        private LevelData selectedLevel;
        private int selectedLevelIndex = -1;
        
        // Foldout durumları
        private bool levelDetailsFoldout = true;
        private bool gridAreaFoldout = true;
        
        // Painting mode
        private enum PaintingMode
        {
            Conveyor,
            Block
        }
        
        private PaintingMode currentPaintingMode = PaintingMode.Block;
        private ColorType selectedColorType = ColorType.Red;
        
        // Drag painting
        private bool isDragging = false;
        private HashSet<Vector2Int> paintedCellsThisDrag = new HashSet<Vector2Int>();
        
        // Grid view controls
        private float gridZoom = 1f;
        private Vector2 gridScrollPosition = Vector2.zero;
        private float baseCellSize = 30f;
        private bool showGridCoordinates = false;

        public static LevelEditor Instance
        {
            get
            {
                if (instance == null)
                    instance = GetWindow<LevelEditor>("Level Editor");
                return instance;
            }
        }

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            Instance.Show();
            Instance.LoadLevelConfig();
        }

        private void OnEnable()
        {
            instance = this;
            LoadLevelConfig();
            
            if (levelConfig != null)
            {
                levelListPanel = new LevelListPanel(this, levelConfig);
            }
        }

        private void LoadLevelConfig()
        {
            levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(levelConfigPath);
            
            if (levelConfig == null)
            {
                Debug.LogError($"LevelConfig bulunamadı: {levelConfigPath}");
            }
        }

        public bool IsLevelSelected(LevelData level)
        {
            return selectedLevel == level;
        }

        public void SelectLevel(LevelData level, int index)
        {
            selectedLevel = level;
            selectedLevelIndex = index;
            Repaint();
        }

        public void PlayLevel(LevelData level)
        {
            if (level == null) return;
            
            levelConfig.TestLevel = level;
            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            
            EditorApplication.isPlaying = true;
            Debug.Log($"Test level olarak ayarlandı: {level.levelName}");
        }

        public void OnLevelDeleted()
        {
            selectedLevel = null;
            selectedLevelIndex = -1;
            Repaint();
        }

        private void OnGUI()
        {
            if (levelConfig == null)
            {
                EditorGUILayout.HelpBox("LevelConfig yüklenemedi! Lütfen path'i kontrol edin.", MessageType.Error);
                if (GUILayout.Button("LevelConfig'i Yükle"))
                {
                    LoadLevelConfig();
                    if (levelConfig != null)
                    {
                        levelListPanel = new LevelListPanel(this, levelConfig);
                    }
                }
                return;
            }

            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            
            // Sol Panel - Level Listesi
            if (levelListPanel != null)
            {
                levelListPanel.Draw();
            }
            
            // Sağ Panel - Level Detayları
            DrawLevelDetailsPanel();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            var toolbarStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = TOOLBAR_HEIGHT,
                padding = new RectOffset(SPACING, SPACING, 5, 5)
            };

            EditorGUILayout.BeginHorizontal(toolbarStyle);
            
            GUILayout.Label($"📋 Level Editor", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Save butonu
            if (selectedLevel != null)
            {
                if (GUILayout.Button("💾 Save", GUILayout.Height(BUTTON_HEIGHT - 5)))
                {
                    SaveSelectedLevel();
                }
            }
            
            var settingsContent = EditorGUIUtility.IconContent("Settings");
            settingsContent.tooltip = "LevelConfig'i Aç";
            if (GUILayout.Button(settingsContent, GUILayout.Width(30), GUILayout.Height(BUTTON_HEIGHT - 5)))
            {
                Selection.activeObject = levelConfig;
                EditorGUIUtility.PingObject(levelConfig);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void SaveSelectedLevel()
        {
            if (selectedLevel == null) return;
            
            EditorUtility.SetDirty(selectedLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ Level saved: {selectedLevel.levelName}");
        }


        private void DrawLevelDetailsPanel()
        {
            EditorGUILayout.BeginVertical();
            
            if (selectedLevel == null)
            {
                EditorGUILayout.Space(50);
                EditorGUILayout.LabelField("Bir level seçin...", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawSelectedLevelDetails();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedLevelDetails()
        {
            // Level Detayları Foldout
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            levelDetailsFoldout = EditorGUILayout.Foldout(levelDetailsFoldout, 
                $"Level {selectedLevelIndex + 1} Detayları", true, EditorStyles.foldoutHeader);
            
            if (levelDetailsFoldout)
            {
                EditorGUILayout.Space(5);
                
                EditorGUI.BeginChangeCheck();
                
                // Level Name
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Level Adı:", GUILayout.Width(120));
                selectedLevel.levelName = EditorGUILayout.TextField(selectedLevel.levelName);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Skip in Loop
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Loop'ta Atla:", GUILayout.Width(120));
                selectedLevel.skipInLoop = EditorGUILayout.Toggle(selectedLevel.skipInLoop);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Difficulty Type
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Zorluk:", GUILayout.Width(120));
                selectedLevel.LevelDifficultyType = (LevelDifficultyType)EditorGUILayout.EnumPopup(selectedLevel.LevelDifficultyType);
                EditorGUILayout.EndHorizontal();
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(selectedLevel);
                    AssetDatabase.SaveAssets();
                }
                
                EditorGUILayout.Space(10);
                
                // Asset butonları
                if (GUILayout.Button("🔍 Asset'i Seç", GUILayout.Height(30)))
                {
                    Selection.activeObject = selectedLevel;
                    EditorGUIUtility.PingObject(selectedLevel);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            // Grid Alan Bölümü
            EditorGUILayout.Space(10);
            DrawGridAreaSection();
        }

        public void CreateNewLevel()
        {
            string savePath = "Assets/Resources/Levels";
            
            // Klasör yapısını oluştur
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            
            if (!AssetDatabase.IsValidFolder(savePath))
                AssetDatabase.CreateFolder("Assets/Resources", "Levels");
            
            // Yeni level numarasını bul
            int levelNumber = GetNextLevelNumber();
            string levelName = $"Level{levelNumber}";
            string assetPath = $"{savePath}/{levelName}.asset";
            
            // Eğer aynı isimde dosya varsa, numara artır
            int counter = levelNumber;
            while (AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null)
            {
                counter++;
                levelName = $"Level{counter}";
                assetPath = $"{savePath}/{levelName}.asset";
            }
            
            // Yeni level oluştur
            var newLevel = CreateInstance<LevelData>();
            newLevel.levelName = levelName;
            
            AssetDatabase.CreateAsset(newLevel, assetPath);
            AssetDatabase.SaveAssets();
            
            // LevelConfig'e ekle
            levelConfig.Levels.Add(newLevel);
            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            
            // Panel'i yenile ve seç
            levelListPanel?.Refresh();
            SelectLevel(newLevel, levelConfig.Levels.Count - 1);
            
            Debug.Log($"✅ Yeni level oluşturuldu: {levelName}");
        }

        public void DuplicateLevel(LevelData originalLevel)
        {
            if (originalLevel == null) return;

            string savePath = "Assets/Resources/Levels";
            
            // Yeni isim bul
            string baseName = originalLevel.levelName;
            int copyNumber = 1;
            string newName = $"{baseName}_Copy";
            string assetPath = $"{savePath}/{newName}.asset";
            
            while (AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null)
            {
                copyNumber++;
                newName = $"{baseName}_Copy{copyNumber}";
                assetPath = $"{savePath}/{newName}.asset";
            }
            
            // Level'i kopyala
            var newLevel = Instantiate(originalLevel);
            newLevel.name = newName;
            newLevel.levelName = newName;
            
            AssetDatabase.CreateAsset(newLevel, assetPath);
            AssetDatabase.SaveAssets();
            
            // Orijinal level'in hemen sonrasına ekle
            int originalIndex = levelConfig.Levels.IndexOf(originalLevel);
            levelConfig.Levels.Insert(originalIndex + 1, newLevel);
            
            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            
            // Panel'i yenile ve seç
            levelListPanel?.Refresh();
            SelectLevel(newLevel, originalIndex + 1);
            
            Debug.Log($"✅ Level kopyalandı: {newName}");
        }

        private int GetNextLevelNumber()
        {
            int maxNumber = 0;
            
            foreach (var level in levelConfig.Levels)
            {
                if (level == null) continue;
                
                // Level isminden numarayı çıkar (Level1, Level2, vb.)
                string name = level.levelName;
                if (name.StartsWith("Level"))
                {
                    string numberPart = name.Substring(5);
                    if (int.TryParse(numberPart, out int number))
                    {
                        maxNumber = Mathf.Max(maxNumber, number);
                    }
                }
            }
            
            return maxNumber + 1;
        }

        private void DrawGridAreaSection()
        {
            if (selectedLevel == null) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Grid Alan Foldout
            gridAreaFoldout = EditorGUILayout.Foldout(gridAreaFoldout, 
                "Grid Alan", true, EditorStyles.foldoutHeader);
            
            if (gridAreaFoldout)
            {
                EditorGUILayout.Space(5);
                
                EditorGUI.BeginChangeCheck();
                
                // Grid Boyutları
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Grid Boyutu:", GUILayout.Width(120));
                int newSatir = EditorGUILayout.IntField(selectedLevel.gridSatirSayisi, GUILayout.Width(60));
                EditorGUILayout.LabelField("x", GUILayout.Width(20));
                int newSutun = EditorGUILayout.IntField(selectedLevel.gridSutunSayisi, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
                
                if (newSatir != selectedLevel.gridSatirSayisi || newSutun != selectedLevel.gridSutunSayisi)
                {
                    selectedLevel.gridSatirSayisi = Mathf.Max(1, newSatir);
                    selectedLevel.gridSutunSayisi = Mathf.Max(1, newSutun);
                }
                
                EditorGUILayout.Space(5);
                
                // Grid Oluştur Butonu
                if (GUILayout.Button("Grid Oluştur", GUILayout.Height(30)))
                {
                    CreateGrid();
                }
                
                EditorGUILayout.Space(10);
                
                // Painting Mode Selection
                EditorGUILayout.LabelField("Painting Mode:", EditorStyles.boldLabel);
                currentPaintingMode = (PaintingMode)EditorGUILayout.EnumPopup(currentPaintingMode);
                
                EditorGUILayout.Space(5);
                
                // Color Palette (Block mode)
                if (currentPaintingMode == PaintingMode.Block)
                {
                    DrawColorPalette();
                }
                else
                {
                    EditorGUILayout.HelpBox("Conveyor Mode: Click cells to add conveyor points", MessageType.Info);
                }
                
                EditorGUILayout.Space(10);
                
                // Grid View Controls
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("View Controls:", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Zoom:", GUILayout.Width(60));
                gridZoom = EditorGUILayout.Slider(gridZoom, 0.5f, 2f, GUILayout.Width(150));
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                {
                    gridZoom = 1f;
                    gridScrollPosition = Vector2.zero;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Cell Size:", GUILayout.Width(60));
                baseCellSize = EditorGUILayout.Slider(baseCellSize, 20f, 50f, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
                
                showGridCoordinates = EditorGUILayout.Toggle("Show Coordinates", showGridCoordinates);
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Grid Alan Görsel Gösterimi
                DrawGridVisualization();
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(selectedLevel);
                    AssetDatabase.SaveAssets();
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawGridVisualization()
        {
            // Grid görselleştirmesi
            int satirSayisi = selectedLevel.gridSatirSayisi;
            int sutunSayisi = selectedLevel.gridSutunSayisi;
            
            // Hücre boyutu ve aralık (zoom ve base size ile)
            float cellSize = baseCellSize * gridZoom;
            float cellSpacing = 4f * gridZoom; // Hücreler arası boşluk
            float totalCellSize = cellSize + cellSpacing;
            
            // Grid alanı hesapla
            float totalWidth = (sutunSayisi * totalCellSize) - cellSpacing;
            float totalHeight = (satirSayisi * totalCellSize) - cellSpacing;
            
            // Padding
            float padding = 10f;
            
            // Scroll view için içerik boyutu
            Vector2 contentSize = new Vector2(totalWidth + (padding * 2), totalHeight + (padding * 2) + 20f);
            
            // Scroll view başlat (sabit yükseklik, horizontal scroll için)
            gridScrollPosition = EditorGUILayout.BeginScrollView(gridScrollPosition, 
                false, true, // horizontal ve vertical scroll
                GUILayout.Height(350), GUILayout.ExpandWidth(true));
            
            // İçerik alanı
            Rect contentRect = GUILayoutUtility.GetRect(contentSize.x, contentSize.y, GUILayout.ExpandWidth(false));
            
            // Mouse wheel zoom kontrolü
            Event currentEvent = Event.current;
            if (contentRect.Contains(currentEvent.mousePosition) && currentEvent.type == EventType.ScrollWheel && !currentEvent.shift)
            {
                float zoomDelta = -currentEvent.delta.y * 0.1f;
                gridZoom = Mathf.Clamp(gridZoom + zoomDelta, 0.5f, 2f);
                currentEvent.Use();
                Repaint();
            }
            
            // Arka plan
            EditorGUI.DrawRect(contentRect, new Color(0.12f, 0.12f, 0.12f, 1f));
            
            // Grid çizgileri
            Handles.BeginGUI();
            
            // Grid bilgisi (üstte kompakt)
            Handles.color = Color.white;
            string gridInfo = $"{satirSayisi} x {sutunSayisi} Grid | Zoom: {gridZoom:F2}x";
            GUI.Label(new Rect(contentRect.x + 5, contentRect.y + 2, 300, 18), 
                gridInfo, EditorStyles.miniLabel);
            
            // Grid'i ortala
            float gridAreaX = contentRect.x + padding + (contentRect.width - totalWidth - (padding * 2)) * 0.5f;
            float gridAreaY = contentRect.y + 20f + padding; // Label için sadece 20f
            
            // Her hücreyi buton gibi çiz
            for (int row = 0; row < satirSayisi; row++)
            {
                for (int col = 0; col < sutunSayisi; col++)
                {
                    float cellX = gridAreaX + (col * totalCellSize);
                    float cellY = gridAreaY + (row * totalCellSize);
                    Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                    
                    // Hücre rengi (buton stili)
                    Color cellColor = new Color(0.25f, 0.25f, 0.25f, 1f);
                    
                    // Hücre arka planı (buton gibi)
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Gölge efekti (alt ve sağ kenarlar)
                    Color shadowColor = new Color(0f, 0f, 0f, 0.3f);
                    Rect shadowRect = new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.width, cellRect.height);
                    EditorGUI.DrawRect(shadowRect, shadowColor);
                    
                    // Parlak kenarlık (üst ve sol)
                    Color highlightColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    Handles.color = highlightColor;
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.y, 0),
                        new Vector3(cellRect.xMax, cellRect.y, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.y, 0),
                        new Vector3(cellRect.x, cellRect.yMax, 0)
                    );
                    
                    // Koyu kenarlık (alt ve sağ)
                    Color darkBorderColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                    Handles.color = darkBorderColor;
                    Handles.DrawLine(
                        new Vector3(cellRect.xMax, cellRect.y, 0),
                        new Vector3(cellRect.xMax, cellRect.yMax, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.yMax, 0),
                        new Vector3(cellRect.xMax, cellRect.yMax, 0)
                    );
                    
                    // Koordinat gösterimi
                    if (showGridCoordinates && cellSize > 25f)
                    {
                        GUIStyle coordStyle = new GUIStyle(EditorStyles.miniLabel);
                        coordStyle.alignment = TextAnchor.UpperLeft;
                        coordStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                        coordStyle.fontSize = Mathf.Max(8, (int)(8 * gridZoom));
                        GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.width, 12), 
                            $"{col},{row}", coordStyle);
                    }
                }
            }
            
            // Tıklama ve sürükleme kontrolü (scroll view koordinatlarına göre)
            if (currentEvent.type == EventType.MouseDown && !currentEvent.alt && contentRect.Contains(currentEvent.mousePosition))
            {
                isDragging = true;
                paintedCellsThisDrag.Clear();
                
                bool isRightClick = currentEvent.button == 1;
                Vector2 mousePos = currentEvent.mousePosition;
                
                for (int row = 0; row < satirSayisi; row++)
                {
                    for (int col = 0; col < sutunSayisi; col++)
                    {
                        float cellX = gridAreaX + (col * totalCellSize);
                        float cellY = gridAreaY + (row * totalCellSize);
                        Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                        
                        if (cellRect.Contains(mousePos))
                        {
                            Vector2Int cellPos = new Vector2Int(col, row);
                            
                            if (isRightClick)
                            {
                                OnCellRightClicked(row, col);
                            }
                            else
                            {
                                OnCellClicked(row, col);
                                paintedCellsThisDrag.Add(cellPos);
                            }
                            currentEvent.Use();
                            Repaint();
                            break;
                        }
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseDrag && isDragging && !currentEvent.alt && contentRect.Contains(currentEvent.mousePosition))
            {
                bool isRightClick = currentEvent.button == 1;
                Vector2 mousePos = currentEvent.mousePosition;
                
                for (int row = 0; row < satirSayisi; row++)
                {
                    for (int col = 0; col < sutunSayisi; col++)
                    {
                        float cellX = gridAreaX + (col * totalCellSize);
                        float cellY = gridAreaY + (row * totalCellSize);
                        Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                        
                        if (cellRect.Contains(mousePos))
                        {
                            Vector2Int cellPos = new Vector2Int(col, row);
                            
                            // Aynı hücreyi tekrar boyamayı önle
                            if (!paintedCellsThisDrag.Contains(cellPos))
                            {
                                if (isRightClick)
                                {
                                    OnCellRightClicked(row, col);
                                }
                                else
                                {
                                    OnCellClicked(row, col);
                                    paintedCellsThisDrag.Add(cellPos);
                                }
                                Repaint();
                            }
                            currentEvent.Use();
                            break;
                        }
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                isDragging = false;
                paintedCellsThisDrag.Clear();
            }
            
            // Hücreleri renklendir (conveyor points ve colored cells)
            DrawCellColors(gridAreaX, gridAreaY, cellSize, totalCellSize, satirSayisi, sutunSayisi);
            
            Handles.EndGUI();
            
            // Scroll view bitir
            EditorGUILayout.EndScrollView();
        }

        private void DrawColorPalette()
        {
            EditorGUILayout.LabelField("Color Palette:", EditorStyles.miniLabel);
            
            // Tüm ColorType'ları göster
            ColorType[] colorTypes = (ColorType[])System.Enum.GetValues(typeof(ColorType));
            int colorsPerRow = 4;
            
            for (int i = 0; i < colorTypes.Length; i += colorsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int j = 0; j < colorsPerRow && (i + j) < colorTypes.Length; j++)
                {
                    ColorType colorType = colorTypes[i + j];
                    Color color = GetColorFromColorType(colorType);
                    
                    // Seçili renk kontrolü
                    bool isSelected = selectedColorType == colorType;
                    Color buttonColor = isSelected ? new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f, 1f) : color;
                    
                    // Renk butonu
                    Rect buttonRect = GUILayoutUtility.GetRect(40, 30, GUILayout.Width(40));
                    
                    // Arka plan
                    EditorGUI.DrawRect(buttonRect, buttonColor);
                    
                    // Seçili ise kenarlık
                    if (isSelected)
                    {
                        Handles.BeginGUI();
                        Handles.color = Color.white;
                        Vector3[] borderVerts = new Vector3[]
                        {
                            new Vector3(buttonRect.x, buttonRect.y, 0),
                            new Vector3(buttonRect.xMax, buttonRect.y, 0),
                            new Vector3(buttonRect.xMax, buttonRect.yMax, 0),
                            new Vector3(buttonRect.x, buttonRect.yMax, 0)
                        };
                        Handles.DrawPolyLine(borderVerts);
                        Handles.DrawLine(borderVerts[3], borderVerts[0]);
                        Handles.EndGUI();
                    }
                    
                    // Tıklama kontrolü
                    Event currentEvent = Event.current;
                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        if (buttonRect.Contains(currentEvent.mousePosition))
                        {
                            selectedColorType = colorType;
                            currentEvent.Use();
                            Repaint();
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private Color GetColorFromColorType(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.Red:
                    return new Color(0.8f, 0.2f, 0.2f, 1f);
                case ColorType.Blue:
                    return new Color(0.2f, 0.4f, 0.8f, 1f);
                case ColorType.Green:
                    return new Color(0.2f, 0.8f, 0.2f, 1f);
                case ColorType.Yellow:
                    return new Color(0.9f, 0.9f, 0.2f, 1f);
                case ColorType.Purple:
                    return new Color(0.6f, 0.2f, 0.8f, 1f);
                case ColorType.Orange:
                    return new Color(1f, 0.5f, 0f, 1f);
                case ColorType.Cyan:
                    return new Color(0.2f, 0.8f, 0.8f, 1f);
                case ColorType.Pink:
                    return new Color(1f, 0.4f, 0.7f, 1f);
                default:
                    return new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }

        private void OnCellClicked(int row, int col)
        {
            if (selectedLevel == null) return;
            
            Vector2Int cellPosition = new Vector2Int(col, row);
            
            if (currentPaintingMode == PaintingMode.Conveyor)
            {
                // Conveyor point ekle
                if (!selectedLevel.conveyorPoints.Contains(cellPosition))
                {
                    selectedLevel.conveyorPoints.Add(cellPosition);
                    
                    // Bu hücredeki rengi kaldır (cellDataList'ten sil)
                    CellData cellToRemove = selectedLevel.cellDataList.FirstOrDefault(c => c.gridPosition == cellPosition);
                    if (cellToRemove != null)
                    {
                        selectedLevel.cellDataList.Remove(cellToRemove);
                        Debug.Log($"Cell color removed from conveyor point: ({col}, {row})");
                    }
                    
                    EditorUtility.SetDirty(selectedLevel);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Conveyor point added: ({col}, {row})");
                }
            }
            else if (currentPaintingMode == PaintingMode.Block)
            {
                // Conveyor point olan hücreleri boyama
                if (selectedLevel.conveyorPoints != null && selectedLevel.conveyorPoints.Contains(cellPosition))
                {
                    Debug.Log($"Cannot paint conveyor point cell: ({col}, {row})");
                    return;
                }
                
                // Cell data ekle veya güncelle
                CellData existingCell = selectedLevel.cellDataList.FirstOrDefault(c => c.gridPosition == cellPosition);
                
                if (existingCell != null)
                {
                    existingCell.colorType = selectedColorType;
                }
                else
                {
                    selectedLevel.cellDataList.Add(new CellData(cellPosition, selectedColorType));
                }
                
                EditorUtility.SetDirty(selectedLevel);
                AssetDatabase.SaveAssets();
                Debug.Log($"Cell painted: ({col}, {row}) with color {selectedColorType}");
            }
        }

        private void OnCellRightClicked(int row, int col)
        {
            if (selectedLevel == null) return;
            
            Vector2Int cellPosition = new Vector2Int(col, row);
            bool somethingRemoved = false;
            
            // Cell data'dan sil
            CellData cellToRemove = selectedLevel.cellDataList.FirstOrDefault(c => c.gridPosition == cellPosition);
            if (cellToRemove != null)
            {
                selectedLevel.cellDataList.Remove(cellToRemove);
                somethingRemoved = true;
                Debug.Log($"Cell color removed: ({col}, {row})");
            }
            
            // Conveyor point'ten sil
            if (selectedLevel.conveyorPoints != null && selectedLevel.conveyorPoints.Contains(cellPosition))
            {
                selectedLevel.conveyorPoints.Remove(cellPosition);
                somethingRemoved = true;
                Debug.Log($"Conveyor point removed: ({col}, {row})");
            }
            
            if (somethingRemoved)
            {
                EditorUtility.SetDirty(selectedLevel);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawCellColors(float gridAreaX, float gridAreaY, float cellSize, float totalCellSize, int satirSayisi, int sutunSayisi)
        {
            // Colored cells çiz (önce çiz ki conveyor üstte görünsün)
            if (selectedLevel.cellDataList != null)
            {
                foreach (var cellData in selectedLevel.cellDataList)
                {
                    Vector2Int pos = cellData.gridPosition;
                    if (pos.x >= 0 && pos.x < sutunSayisi && pos.y >= 0 && pos.y < satirSayisi)
                    {
                        float cellX = gridAreaX + (pos.x * totalCellSize);
                        float cellY = gridAreaY + (pos.y * totalCellSize);
                        Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                        Color cellColor = GetColorFromColorType(cellData.colorType);
                        EditorGUI.DrawRect(cellRect, cellColor);
                    }
                }
            }
            
            // Conveyor points çiz (üstte, "C" harfi ile)
            if (selectedLevel.conveyorPoints != null)
            {
                foreach (var point in selectedLevel.conveyorPoints)
                {
                    if (point.x >= 0 && point.x < sutunSayisi && point.y >= 0 && point.y < satirSayisi)
                    {
                        float cellX = gridAreaX + (point.x * totalCellSize);
                        float cellY = gridAreaY + (point.y * totalCellSize);
                        Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                        EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 1f, 0.4f));
                        
                        // "C" harfi göster
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
                        labelStyle.alignment = TextAnchor.MiddleCenter;
                        labelStyle.normal.textColor = Color.white;
                        labelStyle.fontSize = 16;
                        GUI.Label(cellRect, "C", labelStyle);
                    }
                }
            }
        }

        private void CreateGrid()
        {
            if (selectedLevel == null) return;
            
            int satirSayisi = selectedLevel.gridSatirSayisi;
            int sutunSayisi = selectedLevel.gridSutunSayisi;
            
            // Mevcut rows'u temizle
            selectedLevel.rows.Clear();
            
            // Yeni grid oluştur
            for (int i = 0; i < satirSayisi; i++)
            {
                var row = new BoxRowData();
                // Her sütun için box ekle (varsayılan olarak Red)
                for (int j = 0; j < sutunSayisi; j++)
                {
                    row.AddBox(ColorType.Red);
                }
                selectedLevel.rows.Add(row);
            }
            
            EditorUtility.SetDirty(selectedLevel);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"✅ {satirSayisi}x{sutunSayisi} grid oluşturuldu!");
            Repaint();
        }

    }
}

