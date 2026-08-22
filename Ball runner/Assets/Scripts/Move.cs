using UnityEngine;

public class Move : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed;
    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += new Vector3(-speed*Time.deltaTime,0,0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DestroyZone"))
        {
            Destroy(gameObject);
        }
    }
}
