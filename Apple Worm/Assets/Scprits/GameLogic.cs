using System.Collections.Generic;
using UnityEngine;
using static GameLevel;

public class GameLogic : MonoBehaviour
{
    public event System.Action LevelCompleted;

    // =========================================================
    // PREFAB
    // =========================================================

    [SerializeField] public GameObject worm;
    [SerializeField] public GameObject head;
    [SerializeField] public GameObject apple;
    [SerializeField] public GameObject dirt;
    [SerializeField] public GameObject goal;
    [SerializeField] public GameObject partOfWorm;
    [SerializeField] public GameObject rock;


    // =========================================================
    // CONSTANT
    // =========================================================

    public const int EMPTY = 0;
    public const int WORM_HEAD = 1;
    public const int WORM_BODY = 2;
    public const int APPLE = 3;
    public const int DIRT = 4;
    public const int GOAL = 5;
    public const int ROCK = 6;

    public const int MAP_ROWS = 30;
    public const int MAP_COLS = 20;

    // Chỉ hiển thị 10 × 20
    public const int VIEW_ROWS = 10;


    // =========================================================
    // MAP
    // =========================================================

    public int[][] map;


    // =========================================================
    // WORM POSITION
    // =========================================================

    public List<Vector2> wormPartPos = new List<Vector2>
    {
        new Vector2(2, 9),
        new Vector2(2, 10),
        new Vector2(2, 11),
        new Vector2(2, 12)
    };


    // =========================================================
    // LEVEL
    // =========================================================

    public void ApplyLevel(LevelData levelData)
    {
        if (levelData == null)
            return;

        if (levelData.map != null)
            SetMap(levelData.GetMap());

        if (levelData.wormPartPos != null &&
            levelData.wormPartPos.Count > 0)
        {
            wormPartPos = new List<Vector2>(levelData.wormPartPos);
        }
        else
        {
            ResetWormState();
        }
    }


    // =========================================================
    // OBJECT
    // =========================================================

    public GameObject wormClone;

    public List<GameObject> gameObjects = new List<GameObject>();

    public List<GameObject> wormPartObjects = new List<GameObject>();

    public List<GameObject> rockObjects = new List<GameObject>();

    // Các object chịu physics cần theo dõi
    public List<GameObject> physicsObjects = new List<GameObject>();


    // =========================================================
    // PREFAB DICTIONARY
    // =========================================================

    private Dictionary<int, GameObject> indexObject =
        new Dictionary<int, GameObject>();


    // =========================================================
    // PHYSICS STATE
    // =========================================================

    private bool physicsWasMoving = false;


    // =========================================================
    // RESET WORM
    // =========================================================

    public void ResetWormState()
    {
        wormPartPos.Clear();

        wormPartPos.Add(new Vector2(2, 9));
        wormPartPos.Add(new Vector2(2, 10));
        wormPartPos.Add(new Vector2(2, 11));
        wormPartPos.Add(new Vector2(2, 12));
    }


    // =========================================================
    // SET MAP
    // =========================================================

    public void SetMap(int[][] newMap)
    {
        if (newMap == null)
            return;

        ResetWormState();

        // Deep copy rows
        map = new int[newMap.Length][];

        for (int i = 0; i < newMap.Length; i++)
        {
            map[i] = new int[newMap[i].Length];

            for (int j = 0; j < newMap[i].Length; j++)
            {
                map[i][j] = newMap[i][j];
            }
        }
    }


    // =========================================================
    // INITIALIZE PREFABS
    // =========================================================

    public void InitializePrefabs()
    {
        indexObject.Clear();

        indexObject.Add(WORM_HEAD, head);
        indexObject.Add(WORM_BODY, partOfWorm);
        indexObject.Add(APPLE, apple);
        indexObject.Add(DIRT, dirt);
        indexObject.Add(GOAL, goal);
        indexObject.Add(ROCK, rock);
    }


    // =========================================================
    // MAP → UI
    // =========================================================

    public void MapToUI()
    {
        // =========================================
        // CLEAN PREVIOUS OBJECTS
        // =========================================

        foreach (var obj in gameObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        gameObjects.Clear();


        foreach (var obj in rockObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        rockObjects.Clear();


        foreach (var obj in wormPartObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        wormPartObjects.Clear();


        if (wormClone != null)
            Destroy(wormClone);

        wormClone = null;

        physicsObjects.Clear();


        // =========================================
        // WORM
        // =========================================

        wormClone = Instantiate(
            worm,
            IndexToPos(new Vector2(0, 0)),
            Quaternion.identity
        );

        physicsObjects.Add(wormClone);


        for (int i = 0; i < wormPartPos.Count; i++)
        {
            int type = i == 0
                ? WORM_HEAD
                : WORM_BODY;

            GameObject part = Instantiate(
                indexObject[type],
                IndexToPos(wormPartPos[i]),
                Quaternion.identity
            );

            part.transform.SetParent(wormClone.transform);

            wormPartObjects.Add(part);
        }


        // =========================================
        // SPAWN VISIBLE REGION ONLY
        // 10 × 20
        // =========================================

        for (int row = 0; row < VIEW_ROWS; row++)
        {
            for (int col = 0; col < MAP_COLS; col++)
            {
                Vector2 index = new Vector2(row, col);

                int cell = map[row][col];

                if (cell == EMPTY)
                    continue;


                // Worm đã được tạo riêng
                if (wormPartPos.Contains(index))
                    continue;


                GameObject obj = Instantiate(
                    indexObject[cell],
                    IndexToPos(index),
                    Quaternion.identity
                );


                // =========================================
                // ROCK
                // =========================================

                if (cell == ROCK)
                {
                    rockObjects.Add(obj);
                    physicsObjects.Add(obj);
                }
                else
                {
                    gameObjects.Add(obj);
                }
            }
        }
    }


    // =========================================================
    // CHECK PHYSICS
    // =========================================================

    public bool AllPhysicsStable()
    {
        foreach (GameObject obj in physicsObjects)
        {
            if (obj == null)
                continue;

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

            if (rb == null)
                continue;

            if (rb.linearVelocity.sqrMagnitude > 0.001f)
                return false;
        }

        return true;
    }


    // =========================================================
    // UPDATE MAP FROM OBJECTS
    // =========================================================
    //
    // MAP KHÔNG lưu worm.
    //
    // Worm được quản lý bằng wormPartPos.
    //
    // Map chỉ cần đồng bộ những object động như ROCK.
    // =========================================================

    public void UpdateMapFromObjects()
    {
        ClearDynamicObjectsFromMap();


        // =========================================
        // ROCK
        // =========================================

        foreach (GameObject rockObject in rockObjects)
        {
            if (rockObject == null)
                continue;

            Vector2 index =
                PosToIndex(rockObject.transform.position);

            if (!IsInsideMap(index))
                continue;

            int row = (int)index.x;
            int col = (int)index.y;

            map[row][col] = ROCK;
        }
    }


    // =========================================================
    // CLEAR DYNAMIC OBJECTS
    // =========================================================

    public void ClearDynamicObjectsFromMap()
    {
        for (int row = 0; row < MAP_ROWS; row++)
        {
            for (int col = 0; col < MAP_COLS; col++)
            {
                if (map[row][col] == WORM_HEAD ||
                    map[row][col] == WORM_BODY ||
                    map[row][col] == ROCK)
                {
                    map[row][col] = EMPTY;
                }
            }
        }
    }


    // =========================================================
    // CHECK MAP POSITION
    // =========================================================

    public bool IsInsideMap(Vector2 index)
    {
        return index.x >= 0 &&
               index.x < MAP_ROWS &&
               index.y >= 0 &&
               index.y < MAP_COLS;
    }


    // =========================================================
    // CHECK WORM POSITION
    // =========================================================
    //
    // Đây là phần quan trọng.
    //
    // Không kiểm tra worm bằng map.
    // Kiểm tra trực tiếp wormPartPos.
    // =========================================================

    public bool IsWormAt(Vector2 index)
    {
        return wormPartPos.Contains(index);
    }


    // =========================================================
    // CHECK WHETHER WORM CAN ENTER POSITION
    // =========================================================

    public bool CanWormEnter(Vector2 index)
    {
        // Ngoài map
        if (!IsInsideMap(index))
            return false;

        // Không được đi vào chính thân worm
        if (IsWormAt(index))
            return false;

        return true;
    }


    // =========================================================
    // IS STRAIGHT
    // =========================================================

    public bool IsStraight()
    {
        if (wormPartPos.Count == 0)
            return false;

        float col = wormPartPos[0].y;

        for (int i = 1; i < wormPartPos.Count; i++)
        {
            if (wormPartPos[i].y != col)
                return false;
        }

        return true;
    }


    // =========================================================
    // MOVE
    // =========================================================

    public void Move(Vector2 direction)
    {
        // =========================================
        // LẤY VỊ TRÍ THỰC TẾ CỦA WORM
        // =========================================

        SetWormPartPos();


        // =========================================
        // KHÔNG CÓ WORM
        // =========================================

        if (wormPartPos.Count == 0)
            return;


        // =========================================
        // VỊ TRÍ TIẾP THEO
        // =========================================

        Vector2 nextPosition =
            wormPartPos[0] + direction;


        // =========================================
        // NGOÀI MAP
        // =========================================

        if (!IsInsideMap(nextPosition))
            return;


        // =========================================
        // KHÔNG ĐƯỢC ĐI VÀO THÂN
        // =========================================

        if (wormPartPos.Contains(nextPosition))
            return;


        // =========================================
        // ĐỌC OBJECT TRONG MAP
        // =========================================

        int row = (int)nextPosition.x;
        int col = (int)nextPosition.y;

        int cell = map[row][col];


        // =========================================
        // XỬ LÝ Ô TIẾP THEO
        // =========================================

        switch (cell)
        {
            // -----------------------------------------
            // TRỐNG
            // -----------------------------------------

            case EMPTY:

                MoveWorm(nextPosition);

                break;


            // -----------------------------------------
            // APPLE
            // -----------------------------------------

            case APPLE:

                Eat(nextPosition);

                break;


            // -----------------------------------------
            // ROCK
            // -----------------------------------------

            case ROCK:

                PushRock(nextPosition, direction);

                break;


            // -----------------------------------------
            // GOAL
            // -----------------------------------------

            case GOAL:

                Complete(nextPosition);

                break;


            // -----------------------------------------
            // WORM
            //
            // Không cần thiết vì worm không được lưu
            // trong map nữa.
            // Nhưng giữ lại để phòng map cũ còn dữ liệu.
            // -----------------------------------------

            case WORM_HEAD:
            case WORM_BODY:

                return;
        }
    }


    // =========================================================
    // MOVE WORM
    // =========================================================

    public void MoveWorm(Vector2 newHeadPosition)
    {
        // =========================================
        // DỊCH BODY TỪ ĐUÔI → ĐẦU
        // =========================================

        for (int i = wormPartPos.Count - 1; i > 0; i--)
        {
            wormPartPos[i] =
                wormPartPos[i - 1];
        }


        // =========================================
        // ĐẶT ĐẦU
        // =========================================

        wormPartPos[0] =
            newHeadPosition;


        // =========================================
        // CẬP NHẬT OBJECT
        // =========================================

        SetWormPartObjects();
    }


    // =========================================================
    // READ WORM POSITION FROM OBJECTS
    // =========================================================

    public void SetWormPartPos()
    {
        wormPartPos.Clear();

        foreach (GameObject item in wormPartObjects)
        {
            if (item == null)
                continue;

            Vector2 index =
                PosToIndex(item.transform.position);

            wormPartPos.Add(index);
        }
    }


    // =========================================================
    // UPDATE WORM OBJECTS
    // =========================================================

    public void SetWormPartObjects()
    {
        int count =
            Mathf.Min(
                wormPartPos.Count,
                wormPartObjects.Count
            );

        for (int i = 0; i < count; i++)
        {
            if (wormPartObjects[i] == null)
                continue;

            wormPartObjects[i].transform.position =
                IndexToPos(wormPartPos[i]);
        }
    }


    // =========================================================
    // EAT APPLE
    // =========================================================

    public void Eat(Vector2 index)
    {
        // =========================================
        // THÊM ĐẦU MỚI
        // =========================================

        wormPartPos.Insert(0, index);


        // =========================================
        // TẠO BODY MỚI
        // =========================================

        GameObject newPart = Instantiate(
            indexObject[WORM_BODY],
            Vector2.zero,
            Quaternion.identity
        );

        newPart.transform.SetParent(wormClone.transform);

        wormPartObjects.Add(newPart);


        // =========================================
        // XÓA APPLE
        // =========================================

        GameObject eatenApple =
            GetObjectAtIndex(index);

        if (eatenApple != null)
        {
            gameObjects.Remove(eatenApple);

            Destroy(eatenApple);
        }


        // =========================================
        // MAP Ô APPLE → EMPTY
        // =========================================

        map[(int)index.x][(int)index.y] =
            EMPTY;


        // =========================================
        // UPDATE WORM OBJECT
        // =========================================

        SetWormPartObjects();
    }


    // =========================================================
    // PUSH ROCK
    // =========================================================

    public void PushRock(
        Vector2 rockIndex,
        Vector2 direction)
    {
        // =========================================
        // VỊ TRÍ MỚI CỦA ROCK
        // =========================================

        Vector2 newRockIndex =
            rockIndex + direction;


        // =========================================
        // ROCK RA NGOÀI MAP
        // =========================================

        if (!IsInsideMap(newRockIndex))
            return;


        // =========================================
        // KHÔNG ĐƯỢC ĐẨY ROCK VÀO WORM
        // =========================================

        if (wormPartPos.Contains(newRockIndex))
            return;


        int row = (int)newRockIndex.x;
        int col = (int)newRockIndex.y;


        // =========================================
        // Ô PHÍA SAU ROCK PHẢI TRỐNG
        // =========================================

        if (map[row][col] != EMPTY)
            return;


        // =========================================
        // LẤY ROCK OBJECT
        // =========================================

        GameObject rockObject =
            GetRockAtIndex(rockIndex);

        if (rockObject == null)
            return;


        // =========================================
        // UPDATE MAP
        // =========================================

        map[(int)rockIndex.x][(int)rockIndex.y] =
            EMPTY;

        map[row][col] =
            ROCK;


        // =========================================
        // ĐẨY ROCK
        // =========================================

        rockObject.transform.position =
            IndexToPos(newRockIndex);


        // =========================================
        // ĐÁNH THỨC RIGIDBODY
        // =========================================

        Rigidbody2D rb =
            rockObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.WakeUp();
        }


        // =========================================
        // WORM ĐI VÀO Ô CŨ CỦA ROCK
        // =========================================

        MoveWorm(rockIndex);
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    public void Complete(Vector2 index)
    {
        if (LevelCompleted != null)
            LevelCompleted();


        foreach (GameObject item in wormPartObjects)
        {
            if (item != null)
                Destroy(item);
        }

        wormPartObjects.Clear();
        wormPartPos.Clear();
    }


    // =========================================================
    // FIND OBJECT
    // =========================================================

    public GameObject GetObjectAtIndex(Vector2 index)
    {
        foreach (GameObject obj in gameObjects)
        {
            if (obj == null)
                continue;

            if (PosToIndex(obj.transform.position) == index)
                return obj;
        }

        return null;
    }


    // =========================================================
    // FIND ROCK
    // =========================================================

    public GameObject GetRockAtIndex(Vector2 index)
    {
        foreach (GameObject obj in rockObjects)
        {
            if (obj == null)
                continue;

            if (PosToIndex(obj.transform.position) == index)
                return obj;
        }

        return null;
    }


    // =========================================================
    // INDEX → WORLD POSITION
    // =========================================================

    public Vector2 IndexToPos(Vector2 index)
    {
        return new Vector2(
            index.y - 9.5f,
            -index.x + 4.5f
        );
    }


    // =========================================================
    // WORLD POSITION → INDEX
    // =========================================================

    public Vector2 PosToIndex(Vector2 pos)
    {
        return new Vector2(
            Mathf.Round(-pos.y + 4.5f),
            Mathf.Round(pos.x + 9.5f)
        );
    }
}