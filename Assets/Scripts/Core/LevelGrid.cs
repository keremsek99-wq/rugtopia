using System;

namespace Rugtopia.Core
{
    /// <summary>Immutable rug data: colours, shuttle path (Atkı Yolu) and covered rows.</summary>
    public sealed class LevelGrid
    {
        public readonly int Rows, Cols;
        public readonly char[,] Color;          // [row, col], row 0 = top
        public readonly bool[] CoveredRow;      // veiled until the row becomes active
        public readonly int[] PathRow, PathCol; // shuttle order: bottom row L->R, next row R->L, ...
        public readonly char[] PathColor;
        public int Length { get { return PathRow.Length; } }

        public LevelGrid(string[] rows, string covered)
        {
            Rows = rows.Length; Cols = rows[0].Length;
            Color = new char[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                if (rows[r].Length != Cols) throw new ArgumentException("ragged rug rows");
                for (int c = 0; c < Cols; c++) Color[r, c] = rows[r][c];
            }
            CoveredRow = new bool[Rows];
            for (int r = 0; r < Rows; r++) CoveredRow[r] = IsCovered(covered, Rows, r);
            int n = Rows * Cols;
            PathRow = new int[n]; PathCol = new int[n]; PathColor = new char[n];
            int i = 0;
            for (int k = 0; k < Rows; k++)
            {
                int r = Rows - 1 - k;
                for (int j = 0; j < Cols; j++)
                {
                    int c = k % 2 == 0 ? j : Cols - 1 - j;
                    PathRow[i] = r; PathCol[i] = c; PathColor[i] = Color[r, c]; i++;
                }
            }
        }

        public static bool IsCovered(string covered, int rows, int r)
        {
            if (string.IsNullOrEmpty(covered)) return false;
            if (covered == "top") return r < rows / 2;
            if (covered == "alt") return (rows - 1 - r) % 2 == 1;
            return false;
        }

        /// <summary>Active row for a shuttle position (-1 when the rug is finished).</summary>
        public int RowOf(int hp) { return hp >= Length ? -1 : Rows - 1 - hp / Cols; }
        /// <summary>+1 = left to right, -1 = right to left.</summary>
        public int DirectionOfRow(int r) { return (Rows - 1 - r) % 2 == 0 ? 1 : -1; }
    }
}
