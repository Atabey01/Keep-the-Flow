using System.Collections.Generic;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DEV.Scripts.GamePlay
{
    public class SplineGridEditor : MonoBehaviour
    {
        // -------------------- SPLINE --------------------
        [Title("Spline")]
        [Required]
        public SplineComputer splineComputer;

        // -------------------- GRID SETTINGS --------------------
        [Title("Grid Settings")]
        [MinValue(2)] public int gridWidth = 5;
        [MinValue(2)] public int gridHeight = 5;
        [MinValue(0.1f)] public float cellSize = 1f;

        // -------------------- SELECTED CELLS --------------------
        [FoldoutGroup("Selected Cells (Order Matters)", Expanded = false)]
        [ReadOnly]
        public List<Vector2Int> selectedCells = new();

        // -------------------- GRID DRAW AREA --------------------
        [Title("Grid Drawing Area")]
        [ShowInInspector]
        [TableMatrix(
            SquareCells = true,
            HideColumnIndices = true,
            HideRowIndices = true,
            DrawElementMethod = nameof(DrawCell)
        )]
        private bool[,] grid;

        // -------------------- UNITY --------------------
        private void Reset()
        {
            InitGrid();
        }

        private void OnValidate()
        {
            InitGrid();
        }

        private void InitGrid()
        {
            if (grid == null ||
                grid.GetLength(0) != gridWidth ||
                grid.GetLength(1) != gridHeight)
            {
                grid = new bool[gridWidth, gridHeight];
                selectedCells.Clear();
            }
        }

#if UNITY_EDITOR
        // -------------------- GRID DRAW --------------------
        private bool DrawCell(Rect rect, bool value, int col, int row)
        {
            Event e = Event.current;
            Vector2Int cell = new Vector2Int(col, row);
            bool selected = selectedCells.Contains(cell);

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                ToggleCell(cell);
                GUI.changed = true;
                e.Use();
            }

            DrawOdinStyleCell(rect, cell, selected);
            return selected;
        }

        private void DrawOdinStyleCell(Rect rect, Vector2Int cell, bool selected)
        {
            const float padding = 2f;

            // Border
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));

            // Inner
            Rect inner = new Rect(
                rect.x + padding,
                rect.y + padding,
                rect.width - padding * 2,
                rect.height - padding * 2
            );

            int order = selected ? selectedCells.IndexOf(cell) + 1 : -1;

            Color bg = selected
                ? Color.HSVToRGB(order / Mathf.Max(1f, selectedCells.Count), 0.75f, 0.9f)
                : new Color(0.3f, 0.3f, 0.3f);

            EditorGUI.DrawRect(inner, bg);

            if (order > 0)
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.black }
                };
                EditorGUI.LabelField(inner, order.ToString(), style);
            }
        }
#endif

        // -------------------- LOGIC --------------------
        private void ToggleCell(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= grid.GetLength(0) ||
                cell.y < 0 || cell.y >= grid.GetLength(1))
                return;

            if (selectedCells.Contains(cell))
            {
                selectedCells.Remove(cell);
                grid[cell.x, cell.y] = false;
            }
            else
            {
                selectedCells.Add(cell);
                grid[cell.x, cell.y] = true;
            }
        }

        // -------------------- SPLINE CREATE --------------------
        [Button(ButtonSizes.Large)]
        [EnableIf(nameof(CanCreateSpline))]
        public void CreateSpline()
        {
            List<SplinePoint> points = new();

            foreach (var cell in selectedCells)
            {
                // 🔥 Y (row) eksenini ters çeviriyoruz
                int invertedRow = (gridHeight - 1) - cell.y;

                Vector3 localPos = new Vector3(
                    cell.x * cellSize,
                    0f,
                    invertedRow * cellSize
                );

                points.Add(new SplinePoint(localPos));
            }

            splineComputer.SetPoints(points.ToArray(), SplineComputer.Space.Local);

#if UNITY_EDITOR
            EditorUtility.SetDirty(splineComputer);
#endif
        }

        private bool CanCreateSpline()
        {
            return splineComputer != null && selectedCells.Count >= 2;
        }

        // -------------------- CLEAR --------------------
        [Button]
        public void Clear()
        {
            selectedCells.Clear();
            InitGrid();
        }
    }
}
