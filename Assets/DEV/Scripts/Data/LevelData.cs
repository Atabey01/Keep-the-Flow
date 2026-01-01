using System;
using System.Collections.Generic;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// Tek bir Box'ı temsil eden veri yapısı
    /// </summary>
    [Serializable]
    public class BoxData
    {
        [SerializeField] public ColorType colorType;
        
        public BoxData(ColorType color)
        {
            colorType = color;
        }
    }
    
    /// <summary>
    /// Bir satırdaki (row) tüm Box'ları tutan veri yapısı
    /// </summary>
    [Serializable]
    public class BoxRowData
    {
        [SerializeField] public List<BoxData> boxes = new List<BoxData>();
        
        public BoxRowData()
        {
            boxes = new List<BoxData>();
        }
        
        /// <summary>
        /// Row'a yeni bir box ekler
        /// </summary>
        public void AddBox(ColorType colorType)
        {
            boxes.Add(new BoxData(colorType));
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