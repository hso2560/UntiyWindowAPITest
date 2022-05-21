
using UnityEngine;

public class SimpleAI1 : MonoBehaviour
{
    [SerializeField] private float speed;

    private SpriteRenderer sprr;

    private float checkTime;

    private bool isMoving = false;
    private Vector3 targetPos;

    private void Awake()
    {
        sprr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Judge();
        Move();
    }

    private void Judge()
    {
        if (!isMoving && checkTime < Time.time)
        {
            int ran = Random.Range(0, 100);
            if (ran < 10)
            {
                checkTime = Time.time + Random.Range(2f, 3.5f);
                isMoving = false;
            }
            else
            {
                isMoving = true;
                targetPos = GameManager.instance.mainCam.ViewportToWorldPoint(new Vector2(Random.Range(0f,1f), Random.Range(0f, 1f)));
                sprr.flipX = transform.position.x > targetPos.x;
            }
        }
    }

    private void Move()
    {
        if(isMoving)
        {
            Vector3 dir = (targetPos - transform.position);
            dir.z = 0;
            transform.position += dir.normalized * speed * Time.deltaTime;

            if(dir.sqrMagnitude < 0.2f)
            {
                isMoving = false;
            }
        }
    }
}
