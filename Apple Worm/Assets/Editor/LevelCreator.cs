#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LevelCreator : EditorWindow
{
    // =========================================================
    // GRID
    // =========================================================

    private const int ROWS = 30;
    private const int COLS = 20;

    // =========================================================
    // TILE VALUE
    // =========================================================

    private const int EMPTY = 0;
    private const int APPLE = 3;
    private const int DIRT = 4;
    private const int GOAL = 5;
    private const int ROCK = 6;

    // =========================================================
    // LEVEL
    // =========================================================

    private int levelNumber = 1;

    private List<int> map;

    private List<Vector2> wormPartPos =
        new List<Vector2>();

    private Vector2 initialDirection =
        Vector2.right;

    // =========================================================
    // TOOL
    // =========================================================

    private int selectedTile = DIRT;

    private bool placingWorm = false;

    private Vector2 scrollPosition;

    // =========================================================
    // MENU
    // =========================================================

    [MenuItem("Game/Level Creator")]
    public static void OpenWindow()
    {
        LevelCreator window =
            GetWindow<LevelCreator>("Level Creator");

        window.minSize =
            new Vector2(700, 800);

        window.Initialize();
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private void Initialize()
    {
        if (map != null)
            return;

        map = new List<int>();

        for (int i = 0; i < ROWS * COLS; i++)
        {
            map.Add(EMPTY);
        }
    }

    // =========================================================
    // GUI
    // =========================================================

    private void OnGUI()
    {
        Initialize();

        DrawHeader();

        EditorGUILayout.Space(10);

        DrawTileSelector();

        EditorGUILayout.Space(10);

        DrawWormControls();

        EditorGUILayout.Space(10);

        DrawGrid();

        EditorGUILayout.Space(10);

        DrawSaveButton();
    }

    // =========================================================
    // HEADER
    // =========================================================

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            "LEVEL CREATOR",
            EditorStyles.boldLabel
        );

        GUILayout.FlexibleSpace();

        GUILayout.Label(
            "Level: " + levelNumber,
            EditorStyles.boldLabel
        );

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        levelNumber =
            EditorGUILayout.IntField(
                "Level Number",
                levelNumber
            );
    }

    // =========================================================
    // TILE SELECTOR
    // =========================================================

    private void DrawTileSelector()
    {
        GUILayout.Label(
            "Tile",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        DrawTileButton(
            "Empty",
            EMPTY
        );

        DrawTileButton(
            "Dirt",
            DIRT
        );

        DrawTileButton(
            "Apple",
            APPLE
        );

        DrawTileButton(
            "Rock",
            ROCK
        );

        DrawTileButton(
            "Goal",
            GOAL
        );

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        GUILayout.Label(
            "Selected: " + GetTileName(selectedTile)
        );
    }

    private void DrawTileButton(
        string label,
        int tile)
    {
        GUIStyle style =
            new GUIStyle(
                GUI.skin.button
            );

        if (selectedTile == tile)
        {
            style.fontStyle =
                FontStyle.Bold;
        }

        if (GUILayout.Button(
            label,
            style,
            GUILayout.Height(35)))
        {
            selectedTile = tile;

            placingWorm = false;
        }
    }

    // =========================================================
    // WORM CONTROLS
    // =========================================================

    private void DrawWormControls()
    {
        GUILayout.Label(
            "Worm",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            placingWorm
                ? "Placing Worm..."
                : "Place Worm",
            GUILayout.Height(30)))
        {
            placingWorm = true;
        }

        if (GUILayout.Button(
            "Clear Worm",
            GUILayout.Height(30)))
        {
            wormPartPos.Clear();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        GUILayout.Label(
            "Initial Direction"
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("←"))
        {
            initialDirection =
                Vector2.left;
        }

        if (GUILayout.Button("→"))
        {
            initialDirection =
                Vector2.right;
        }

        if (GUILayout.Button("↑"))
        {
            initialDirection =
                Vector2.up;
        }

        if (GUILayout.Button("↓"))
        {
            initialDirection =
                Vector2.down;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Label(
            "Direction: " +
            GetDirectionName(initialDirection)
        );

        if (placingWorm)
        {
            EditorGUILayout.HelpBox(
                "Click các ô để đặt từng phần Worm. " +
                "Click theo thứ tự từ đầu đến đuôi.",
                MessageType.Info
            );
        }
    }

    // =========================================================
    // GRID
    // =========================================================

    private void DrawGrid()
    {
        GUILayout.Label(
            "Map " + COLS + " × " + ROWS,
            EditorStyles.boldLabel
        );

        scrollPosition =
            EditorGUILayout.BeginScrollView(
                scrollPosition
            );

        for (int row = 0; row < ROWS; row++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int col = 0; col < COLS; col++)
            {
                DrawCell(row, col);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    // =========================================================
    // CELL
    // =========================================================

    private void DrawCell(
        int row,
        int col)
    {
        int index =
            row * COLS + col;

        int value =
            map[index];

        string text =
            GetTileSymbol(value);

        GUIStyle style =
            new GUIStyle(
                GUI.skin.button
            );

        style.fontStyle =
            FontStyle.Bold;

        style.fontSize = 12;

        // -----------------------------------------
        // WORM
        // -----------------------------------------

        int wormIndex =
            GetWormIndex(row, col);

        if (wormIndex >= 0)
        {
            text =
                wormIndex == 0
                    ? "H"
                    : "W";
        }

        // -----------------------------------------
        // CELL CLICK
        // -----------------------------------------

        if (GUILayout.Button(
            text,
            style,
            GUILayout.Width(30),
            GUILayout.Height(30)))
        {
            HandleCellClick(
                row,
                col
            );
        }
    }

    // =========================================================
    // CELL CLICK
    // =========================================================

    private void HandleCellClick(
        int row,
        int col)
    {
        // -----------------------------------------
        // PLACE WORM
        // -----------------------------------------

        if (placingWorm)
        {
            AddWormPart(
                row,
                col
            );

            Repaint();

            return;
        }

        // -----------------------------------------
        // NORMAL TILE
        // -----------------------------------------

        int index =
            row * COLS + col;

        map[index] =
            selectedTile;

        Repaint();
    }

    // =========================================================
    // ADD WORM PART
    // =========================================================

    private void AddWormPart(
        int row,
        int col)
    {
        Vector2 pos =
            new Vector2(
                row,
                col
            );

        // Không cho đặt trùng
        if (wormPartPos.Contains(pos))
            return;

        // Không cho Worm nằm trên tile khác
        int index =
            row * COLS + col;

        map[index] =
            EMPTY;

        wormPartPos.Add(pos);
    }

    // =========================================================
    // GET WORM INDEX
    // =========================================================

    private int GetWormIndex(
        int row,
        int col)
    {
        Vector2 pos =
            new Vector2(
                row,
                col
            );

        return wormPartPos.IndexOf(pos);
    }

    // =========================================================
    // SAVE
    // =========================================================

    private void DrawSaveButton()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "Clear Map",
            GUILayout.Height(40)))
        {
            ClearMap();
        }

        if (GUILayout.Button(
            "Save Level",
            GUILayout.Height(40)))
        {
            SaveLevel();
        }

        EditorGUILayout.EndHorizontal();
    }

    // =========================================================
    // CLEAR MAP
    // =========================================================

    private void ClearMap()
    {
        if (!EditorUtility.DisplayDialog(
            "Clear Map",
            "Bạn có chắc muốn xóa toàn bộ map?",
            "Yes",
            "Cancel"))
        {
            return;
        }

        for (int i = 0; i < map.Count; i++)
        {
            map[i] =
                EMPTY;
        }

        wormPartPos.Clear();

        Repaint();
    }

    // =========================================================
    // SAVE LEVEL
    // =========================================================

    private void SaveLevel()
    {
        string resourcesFolder =
            "Assets/Resources";

        string levelFolder =
            "Assets/Resources/Levels";

        // -----------------------------------------
        // CREATE RESOURCES
        // -----------------------------------------

        if (!AssetDatabase.IsValidFolder(
            resourcesFolder))
        {
            AssetDatabase.CreateFolder(
                "Assets",
                "Resources"
            );
        }

        // -----------------------------------------
        // CREATE LEVELS
        // -----------------------------------------

        if (!AssetDatabase.IsValidFolder(
            levelFolder))
        {
            AssetDatabase.CreateFolder(
                resourcesFolder,
                "Levels"
            );
        }

        // -----------------------------------------
        // CREATE DATA
        // -----------------------------------------

        LevelData level =
            ScriptableObject.CreateInstance<LevelData>();

        level.levelNumber =
            levelNumber;

        level.rows =
            ROWS;

        level.cols =
            COLS;

        level.map =
            new List<int>(map);

        level.wormPartPos =
            new List<Vector2>(
                wormPartPos
            );

        level.initialDirection =
            initialDirection;

        // -----------------------------------------
        // PATH
        // -----------------------------------------

        string path =
            levelFolder +
            "/Level" +
            levelNumber.ToString("00") +
            ".asset";

        // -----------------------------------------
        // IF EXISTS
        // -----------------------------------------

        LevelData oldLevel =
            AssetDatabase.LoadAssetAtPath<LevelData>(
                path
            );

        if (oldLevel != null)
        {
            EditorUtility.CopySerialized(
                level,
                oldLevel
            );

            EditorUtility.SetDirty(
                oldLevel
            );

            DestroyImmediate(
                level
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject =
                oldLevel;

            Debug.Log(
                "Updated " + path
            );

            return;
        }

        // -----------------------------------------
        // CREATE
        // -----------------------------------------

        AssetDatabase.CreateAsset(
            level,
            path
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject =
            level;

        Debug.Log(
            "Created " + path
        );
    }

    // =========================================================
    // TILE NAME
    // =========================================================

    private string GetTileName(
        int value)
    {
        switch (value)
        {
            case EMPTY:
                return "Empty";

            case APPLE:
                return "Apple";

            case DIRT:
                return "Dirt";

            case GOAL:
                return "Goal";

            case ROCK:
                return "Rock";

            default:
                return "Unknown";
        }
    }

    // =========================================================
    // TILE SYMBOL
    // =========================================================

    private string GetTileSymbol(
        int value)
    {
        switch (value)
        {
            case EMPTY:
                return "";

            case APPLE:
                return "A";

            case DIRT:
                return "D";

            case GOAL:
                return "G";

            case ROCK:
                return "R";

            default:
                return "?";
        }
    }

    // =========================================================
    // DIRECTION NAME
    // =========================================================

    private string GetDirectionName(
        Vector2 direction)
    {
        if (direction == Vector2.left)
            return "Left";

        if (direction == Vector2.right)
            return "Right";

        if (direction == Vector2.up)
            return "Up";

        if (direction == Vector2.down)
            return "Down";

        return "Unknown";
    }
}

#endif