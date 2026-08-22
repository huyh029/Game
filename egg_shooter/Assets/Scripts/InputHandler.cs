using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public GameLogic gameLogic;
    public float dragThreshold = 20f; // pixels

    private bool pointerDown = false;
    private Vector2 pointerStart;
    private bool isDragging = false;

    private Camera mainCamera;

    void Start()
    {
        if (gameLogic == null)
        {
            gameLogic = FindAnyObjectByType<GameLogic>();
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (gameLogic == null) return;

        if (Input.touchCount > 0)
        {
            HandleTouch(Input.GetTouch(0));
        }
        else
        {
            HandleMouse();
        }
    }

    // =========================================================
    // TOUCH
    // =========================================================

    private void HandleTouch(Touch touch)
    {
        Vector2 screenPosition = touch.position;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandlePointerDown(screenPosition, touch.fingerId);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                HandlePointerMove(screenPosition);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                HandlePointerUp(screenPosition);
                break;
        }
    }

    // =========================================================
    // MOUSE
    // =========================================================

    private void HandleMouse()
    {
        Vector2 screenPosition = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandlePointerDown(screenPosition);
        }

        if (Input.GetMouseButton(0))
        {
            HandlePointerMove(screenPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandlePointerUp(screenPosition);
        }
    }

    // =========================================================
    // POINTER DOWN
    // =========================================================

    private void HandlePointerDown(Vector2 screenPosition, int fingerId = -1)
    {
        if (IsPointerOverUI(fingerId))
        {
            ResetInput();
            return;
        }

        pointerDown = true;
        pointerStart = screenPosition;
        isDragging = false;

        gameLogic.HideArrow();
    }

    // =========================================================
    // POINTER MOVE
    // =========================================================

    private void HandlePointerMove(Vector2 screenPosition)
    {
        if (!pointerDown)
            return;

        // Chưa drag → kiểm tra xem đã vượt threshold chưa
        if (!isDragging)
        {
            if (Vector2.Distance(pointerStart, screenPosition) <= dragThreshold)
                return;

            StartDragging(screenPosition);
        }

        // Đang drag → liên tục cập nhật hướng
        if (isDragging)
        {
            UpdateArrowDirection(screenPosition);
        }
    }

    // =========================================================
    // BẮT ĐẦU DRAG
    // =========================================================

    private void StartDragging(Vector2 screenPosition)
    {
        isDragging = true;

        gameLogic.ShowArrow();

        UpdateArrowDirection(screenPosition);
    }

    // =========================================================
    // POINTER UP
    // =========================================================

    private void HandlePointerUp(Vector2 screenPosition)
    {
        if (!pointerDown)
        {
            ResetInput();
            return;
        }

        // Nếu chỉ click/tap, chưa drag
        if (!isDragging)
        {
            UpdateArrowDirection(screenPosition);
        }

        gameLogic.Shoot();

        ResetInput();
    }

    // =========================================================
    // CẬP NHẬT HƯỚNG MŨI TÊN
    // =========================================================

    private void UpdateArrowDirection(Vector2 screenPosition)
    {
        Vector3 worldPosition = ScreenToWorldPosition(screenPosition);

        gameLogic.SetArrowAngleToWorldPos(worldPosition);
    }

    // =========================================================
    // SCREEN → WORLD
    // =========================================================

    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        float cameraDistance = -mainCamera.transform.position.z;

        return mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                cameraDistance
            )
        );
    }

    // =========================================================
    // KIỂM TRA UI
    // =========================================================

    private bool IsPointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null)
            return false;

        if (fingerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(fingerId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    // =========================================================
    // RESET INPUT
    // =========================================================

    private void ResetInput()
    {
        pointerDown = false;
        isDragging = false;
    }
}