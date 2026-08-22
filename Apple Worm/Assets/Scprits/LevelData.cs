using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Level",
    menuName = "Game/Level"
)]
public class LevelData : ScriptableObject
{
    // =========================================================
    // LEVEL INFO
    // =========================================================

    public int levelNumber;


    // =========================================================
    // MAP
    // =========================================================

    public int rows = 30;
    public int cols = 20;

    // Unity không serialize được int[][]
    // nên lưu map dạng List<int>
    //
    // index = row * cols + col
    //
    // Ví dụ:
    // row 0: index 0 -> cols - 1
    // row 1: index cols -> 2 * cols - 1
    public List<int> map = new List<int>();


    // =========================================================
    // WORM
    // =========================================================

    public List<Vector2> wormPartPos =
        new List<Vector2>();


    // =========================================================
    // INITIAL DIRECTION
    // =========================================================

    public Vector2 initialDirection =
        Vector2.right;


    // =========================================================
    // GET MAP
    // =========================================================

    public int[][] GetMap()
    {
        int[][] result = new int[rows][];

        for (int row = 0; row < rows; row++)
        {
            result[row] = new int[cols];

            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;

                if (index < map.Count)
                {
                    result[row][col] = map[index];
                }
                else
                {
                    result[row][col] = 0;
                }
            }
        }

        return result;
    }
}