using System;
using System.Collections.Generic;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// Bir satırdaki (row) tüm Box'ları tutan veri yapısı
    /// </summary>
    [Serializable]
    public class BoxRowData
    {
        [SerializeField] public List<ColorType> boxes = new List<ColorType>();
        
        public BoxRowData()
        {
            boxes = new List<ColorType>();
        }
        
        /// <summary>
        /// Row'a yeni bir box ekler
        /// </summary>
        public void AddBox(ColorType colorType)
        {
            boxes.Add(colorType);
        }
        
        /// <summary>
        /// Belirli index'teki box'ı siler
        /// </summary>
        public void RemoveBoxAt(int index)
        {
            if (index >= 0 && index < boxes.Count)
                boxes.RemoveAt(index);
        }
    }
    
    /// <summary>
    /// Grid hücresindeki veri yapısı
    /// </summary>
    [Serializable]
    public class CellData
    {
        [SerializeField] public Vector2Int gridPosition;
        [SerializeField] public ColorType colorType;
        
        public CellData(Vector2Int position, ColorType color)
        {
            gridPosition = position;
            colorType = color;
        }
    }
    
    /// <summary>
    /// Cannon Column (her sütun bir renk listesi)
    /// </summary>
    [Serializable]
    public class CannonColumn
    {
        [SerializeField] public List<ColorType> colors = new List<ColorType>();
        
        public CannonColumn()
        {
            colors = new List<ColorType>();
        }
    }
    
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Data/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Settings")] 
        [SerializeField] public string levelName;
        [SerializeField] public bool skipInLoop = false;
        [SerializeField] public LevelDifficultyType LevelDifficultyType = LevelDifficultyType.Normal;
        
        [Header("Level Grid")]
        [Tooltip("Leveldaki tüm satırlar (her satır box'lardan oluşur)")]
        [SerializeField] public List<BoxRowData> rows = new List<BoxRowData>();
        
        [Header("Grid Area")]
        [Tooltip("Grid row count")]
        [SerializeField] public int gridSatirSayisi = 5;
        
        [Tooltip("Grid column count")]
        [SerializeField] public int gridSutunSayisi = 5;
        
        [Header("Conveyor Points")]
        [Tooltip("Conveyor path points for spline")]
        [SerializeField] public List<Vector2Int> conveyorPoints = new List<Vector2Int>();
        
        [Header("Cell Data")]
        [Tooltip("Colored cells data")]
        [SerializeField] public List<CellData> cellDataList = new List<CellData>();
        
        [Header("Cannon Columns")]
        [Tooltip("Cannon columns grid row count (vertical arrangement of columns)")]
        [SerializeField] public int cannonColumnGridRowCount = 1;
        
        [Tooltip("Cannon columns (each column contains a list of color types)")]
        [SerializeField] public List<CannonColumn> cannonColumns = new List<CannonColumn>();
        
        /// <summary>
        /// Level'a yeni bir satır ekler
        /// </summary>
        public void AddRow()
        {
            rows.Add(new BoxRowData());
        }
        
        /// <summary>
        /// Belirli index'teki satırı siler
        /// </summary>
        public void RemoveRowAt(int index)
        {
            if (index >= 0 && index < rows.Count)
                rows.RemoveAt(index);
        }
        
        /// <summary>
        /// Belirli bir satıra box ekler
        /// </summary>
        public void AddBoxToRow(int rowIndex, ColorType colorType)
        {
            if (rowIndex >= 0 && rowIndex < rows.Count)
                rows[rowIndex].AddBox(colorType);
        }
        
        /// <summary>
        /// Level'daki toplam satır sayısı
        /// </summary>
        public int GetRowCount() => rows.Count;
        
        /// <summary>
        /// Belirli bir satırdaki box sayısı
        /// </summary>
        public int GetBoxCountInRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < rows.Count)
                return rows[rowIndex].boxes.Count;
            return 0;
        }
    }
}