using UnityEngine;

public class Simple3DAI1 : MonoBehaviour
{
    [SerializeField] private float speed, rotSpeed;

    private float checkTime;

    private bool isMoving = false, isRotating = false;
    private Vector3 targetPos;
    private float targetRotY;

    private void Update()
    {
        Judge();
        Move();
        Rotate();
    }

    private void Judge()
    {
        if (!isMoving && !isRotating && checkTime < Time.time)
        {
            int ran = Random.Range(0, 100);
            if (ran < 10)
            {
                checkTime = Time.time + Random.Range(2f, 3.5f);
                isMoving = false;
                isRotating = false;
            }
            else
            {
                isMoving = true;
                targetPos = GameManager.instance.mainCam.ViewportToWorldPoint(new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)));
                targetRotY = transform.position.x > targetPos.x ? -140f : 140f;
                isRotating = true;
            }
        }
    }

    private void Move()
    {
        if (isMoving && !isRotating)
        {
            Vector3 dir = (targetPos - transform.position);
            dir.z = 0;
            transform.position += dir.normalized * speed * Time.deltaTime;

            if (dir.sqrMagnitude < 0.2f)
            {
                isMoving = false;
            }
        }
    }

    private void Rotate()
    {
        if(isRotating)
        {
            if(transform.rotation.y != targetRotY)
            {
                Quaternion q = Quaternion.Euler(0, targetRotY, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotSpeed);
                if(Quaternion.Angle(transform.rotation, q) < 0.1f)
                {
                    isRotating = false;
                }
                
            }
        }
    }
}
