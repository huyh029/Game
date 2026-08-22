using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    // danh sách màu
    public List<Color> listColor = new List<Color>();
    // biến ballfabs
    public GameObject ballPrefab;
    //biến đại diện cho bóng được chờ bắn
    private GameObject pendingBall;
    // biến đại diện cho vị trí của bóng được chờ bắn
    public Vector2 pendingBallPosition = new Vector2(0f, -4f);
    // biến đại diện cho chiều cao 1 hàng
    private float rowHeight = 0.85f;
    // biến lưu số hàng tối đa trước khi thua
    public int maxRowCount = 7;
    // biến đại diện cho tốc độ bắn
    public float shootSpeed = 10f;
    // biến đại diện cho lưới bóng
    private List<GameObject> ballGrid = new List<GameObject>();
    // biến đại diện cho số lượng bóng trên 1 hàng
    private int ballsPerRow = 10;
    // tạo biên đại diện cho mũi tên
    public GameObject arrow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // tạo biến lưu góc bắn
    private float shootAngle;
    // tạo biến int lưu trạng thái bắn 0: chờ, 1 bay, 2 dừng
    private int shootState = 0;

    private GameController gameController;
    void Start()
    {
        gameController = FindAnyObjectByType<GameController>();
        // tạo bóng chờ bắn
        pendingBall = CreateBall(Random.Range(0, listColor.Count), pendingBallPosition);
        // tạo 1 hàng bóng
        CreateRow();
        // ẩn mũi tên cho đến khi drop
        if (arrow != null) arrow.SetActive(false);
    }

    // hàm tạo 1 clone của ball prefabs bằng nhập vào index của listcolor và position
    private GameObject CreateBall(int index, Vector2 position){
        if (index >= 0 && index < listColor.Count)
        {
            GameObject newBall = Instantiate(ballPrefab, position, Quaternion.identity);
            SpriteRenderer ballRenderer = newBall.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.color = listColor[index];
            }
            return newBall;
        }
        return null;
    }

    // hàm tạo 1 hàng bóng
    private void CreateRow()
    {
        foreach (GameObject ball in ballGrid)
        {
            if (ball != null)
            {
                ball.transform.position += new Vector3(0f, -rowHeight, 0f);
            }
        }
        float startX = -4.5f + ((10-ballsPerRow) * 0.5f); // vị trí x bắt đầu của hàng
        float yPosition = 3.5f; // vị trí y của hàng

        for (int i = 0; i < ballsPerRow; i++)
        {
            int colorIndex = Random.Range(0, listColor.Count); // chọn màu ngẫu nhiên từ danh sách
            Vector2 position = new Vector2(startX + (i * 1f), yPosition); // tính toán vị trí của bóng
            ballGrid.Add(CreateBall(colorIndex, position)); // tạo bóng tại vị trí và màu đã chọn
        }
        // nếu ballsperrow =  10 thì set lại thành 9
        if (ballsPerRow == 10)
        {
            ballsPerRow = 9;
        }
        else
        {
            ballsPerRow = 10;
        }

    }
    // tạo hàm shoot
    public void Shoot()
    {
        // nếu đang ở trạng thái bay hoặc dừng thì không làm gì
        if (shootState != 0) return;
        if (shootState == 0) // nếu đang ở trạng thái chờ
        {
            shootState = 1; // chuyển sang trạng thái bay
            // góc của mũi tên vào biến góc bắn
            shootAngle = arrow.transform.eulerAngles.z;
        }
    }

    // Public API for input handler
    public void ShowArrow()
    {
        if (arrow != null) arrow.SetActive(true);
    }

    public void HideArrow()
    {
        if (arrow != null) arrow.SetActive(false);
    }

    public void SetArrowAngleToWorldPos(Vector3 worldPos)
    {
        if (arrow == null) return;
        worldPos.z = 0f;
        Vector2 dir = (Vector2)worldPos - (Vector2)arrow.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, 10f, 170f);
        arrow.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
    // tạo hàm bay
    private void Fly()
    {
        if (shootState != 1) return; // nếu không ở trạng thái bay thì không làm gì
        if (pendingBall != null)
        {
            // tính toán hướng bay dựa trên góc bắn
            float angleRad = shootAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
            // di chuyển bóng theo hướng bay với tốc độ shootSpeed
            pendingBall.transform.position += (Vector3)(direction * shootSpeed * Time.deltaTime);
        }
    }
    // tạo hàm bật tường công khai
    public void BounceOffWall()
    {
        if (shootState != 1) return; // nếu không ở trạng thái bay thì không làm gì
        if (pendingBall != null)
        {
            // đổi hướng bay theo trục x
            shootAngle = 180f - shootAngle;
        }
    }
    // tạo hàm ngừng bay công khai
    public void StopFlying()
    {
        if (shootState !=1) return; // nếu không ở trạng thái bay thì không làm gì
        shootState = 2; // chuyển sang trạng thái dừng
        NormalizeBall();
        // thêm bóng vào lưới bóng
        if (pendingBall != null)
        {
            ballGrid.Add(pendingBall);
            pendingBall = null;
        }
        // gọi bfs
        if (ballGrid.Count > 0)
        {
            GameObject lastBall = ballGrid[ballGrid.Count - 1];
            SpriteRenderer lastBallRenderer = lastBall.GetComponent<SpriteRenderer>();
            if (lastBallRenderer != null)
            {
                Color targetColor = lastBallRenderer.color;
                HashSet<GameObject> visited = new HashSet<GameObject>();
                BFS(lastBall, targetColor, visited);
            }
        }
        // tạo bóng chờ bắn mới
        pendingBall = CreateBall(Random.Range(0, listColor.Count), pendingBallPosition);
        // tạo 1 hàng bóng mới
        CreateRow();
        //chuyển sang trạng thái chờ
        shootState = 0;
        // ẩn mũi tên cho đến khi drop tiếp theo
        if (arrow != null) arrow.SetActive(false);
    }
    // tạo hàm chuẩn hóa bóng
    private void NormalizeBall(){
        // tạo danh sách tất cả hàng xóm
        List<Vector3> allNeighbors = new List<Vector3>();
        // duyệt lưới bóng
        foreach (GameObject ball in ballGrid)
        {
            // mỗi quả bóng trong lưới bóng đều có 6 vị trí hàng xóm bao gồm vector (+-1,0),(+-0.5,+-0.85)
            // tạo danh sách hàng xóm
            List<Vector3> neighbors = new List<Vector3>{
                ball.transform.position + new Vector3(1f, 0f, 0f),
                ball.transform.position + new Vector3(-1f, 0f, 0f),
                ball.transform.position + new Vector3(0.5f, 0.85f, 0f),
                ball.transform.position + new Vector3(-0.5f, 0.85f, 0f),
                ball.transform.position + new Vector3(0.5f, -0.85f, 0f),
                ball.transform.position + new Vector3(-0.5f, -0.85f, 0f)
            };
            // add tất cả vào tất cả hàng xóm
            allNeighbors.AddRange(neighbors);
        }
        // bỏ trùng lặp trong danh sách tất cả hàng xóm
        HashSet<Vector3> uniqueNeighbors = new HashSet<Vector3>(allNeighbors);
        // bỏ qua các hàng xóm trùng với vị trí của các quả bóng trong lưới bóng
        foreach (GameObject ball in ballGrid)
        {
            uniqueNeighbors.Remove(ball.transform.position);
        }
        // bỏ qua các hàng xóm có vị trí y > 3.5f x<-4.5 x>4.5f
        uniqueNeighbors.RemoveWhere(pos => pos.y > 3.5f || pos.x < -4.5f || pos.x > 4.5f);
        // tạo danh sách các vị trí hàng xóm còn lại
        List<Vector3> remainingNeighbors = new List<Vector3>(uniqueNeighbors);
        // duyệt xem penddingball gần với vị trí nào nhất trong danh sách các vị trí hàng xóm còn lại
        if (pendingBall != null)
        {
            Vector3 closestPosition = remainingNeighbors[0];
            float closestDistance = Vector3.Distance(pendingBall.transform.position, closestPosition);
            foreach (Vector3 pos in remainingNeighbors)
            {
                float distance = Vector3.Distance(pendingBall.transform.position, pos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPosition = pos;
                }
            }
            // set vị trí của penddingball về vị trí gần nhất
            pendingBall.transform.position = closestPosition;
        }
    }
    // tạo hàm bfs duyệt lưới bóng để tìm các quả bóng cùng màu với quả bóng được bắn
    private void BFS(GameObject startBall, Color targetColor, HashSet<GameObject> visited)
    {
        Queue<GameObject> queue = new Queue<GameObject>();
        queue.Enqueue(startBall);
        visited.Add(startBall);

        while (queue.Count > 0)
        {
            GameObject currentBall = queue.Dequeue();
            Vector3 currentPosition = currentBall.transform.position;

            // duyệt tất cả các hàng xóm của quả bóng hiện tại
            List<Vector3> neighbors = new List<Vector3>
            {
                currentPosition + new Vector3(1f, 0f, 0f),
                currentPosition + new Vector3(-1f, 0f, 0f),
                currentPosition + new Vector3(0.5f, 0.85f, 0f),
                currentPosition + new Vector3(-0.5f, 0.85f, 0f),
                currentPosition + new Vector3(0.5f, -0.85f, 0f),
                currentPosition + new Vector3(-0.5f, -0.85f, 0f)
            };

            foreach (Vector3 neighborPos in neighbors)
            {
                // tìm quả bóng tại vị trí hàng xóm
                GameObject neighborBall = ballGrid.Find(ball => ball.transform.position == neighborPos);
                if (neighborBall != null && !visited.Contains(neighborBall))
                {
                    SpriteRenderer neighborRenderer = neighborBall.GetComponent<SpriteRenderer>();
                    if (neighborRenderer != null && neighborRenderer.color == targetColor)
                    {
                        visited.Add(neighborBall);
                        queue.Enqueue(neighborBall);
                    }
                }
            }
        }
        // nếu số lượng quả bóng cùng màu >= 3 thì làm chúng rơi chuyển thành 
        //  dynemic
        if (visited.Count >= 3)
        {
            gameController.AddScore(visited.Count);
            foreach (GameObject ball in visited)
            { 
                ballGrid.Remove(ball); 
                // cho dịch xuống 0.85f trước khi rơi
                ball.transform.position += new Vector3(0f, -0.85f, 0f);
                ball.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            }
        }
        // duyệt trên xuống để tìm các bóng không liên kết với trần
        FindFloatingBalls();
    }
    private void FindFloatingBalls()
    {
        HashSet<GameObject> connectedToTop = new HashSet<GameObject>();
        Queue<GameObject> queue = new Queue<GameObject>();

        // Tìm các bóng ở hàng trên cùng
        foreach (GameObject ball in ballGrid)
        {
            if (IsTopBall(ball))
            {
                connectedToTop.Add(ball);
                queue.Enqueue(ball);
            }
        }

        // BFS tìm toàn bộ bóng còn kết nối với trần
        while (queue.Count > 0)
        {
            GameObject currentBall = queue.Dequeue();

            Vector3 currentPosition = currentBall.transform.position;

            List<Vector3> neighbors = new List<Vector3>
            {
                currentPosition + new Vector3(1f, 0f, 0f),
                currentPosition + new Vector3(-1f, 0f, 0f),
                currentPosition + new Vector3(0.5f, 0.85f, 0f),
                currentPosition + new Vector3(-0.5f, 0.85f, 0f),
                currentPosition + new Vector3(0.5f, -0.85f, 0f),
                currentPosition + new Vector3(-0.5f, -0.85f, 0f)
            };

            foreach (Vector3 neighborPos in neighbors)
            {
                GameObject neighborBall = ballGrid.Find(
                    ball => Vector3.Distance(ball.transform.position, neighborPos) < 0.01f
                );

                if (neighborBall != null && !connectedToTop.Contains(neighborBall))
                {
                    connectedToTop.Add(neighborBall);
                    queue.Enqueue(neighborBall);
                }
            }
        }

        // Những bóng không kết nối với trần sẽ rơi
        List<GameObject> floatingBalls = new List<GameObject>();

        foreach (GameObject ball in ballGrid)
        {
            if (!connectedToTop.Contains(ball))
            {
                floatingBalls.Add(ball);
            }
        }

        foreach (GameObject ball in floatingBalls)
        {
            ballGrid.Remove(ball);

            ball.transform.position += new Vector3(0f, -0.85f, 0f);

            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                gameController.AddScore(1);
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }
    private bool IsTopBall(GameObject ball)
    {
        float topY = 3.5f;

        return Mathf.Abs(ball.transform.position.y - topY) < 0.1f;
    }
    private bool CheckFinish()
    {
        foreach (var item in ballGrid)
        {
            if (item.transform.position.y <= 3.5f - maxRowCount * rowHeight) return true;
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        Fly();

        if (CheckFinish())
        {
            gameController.EndGame();
        }
    }
}
