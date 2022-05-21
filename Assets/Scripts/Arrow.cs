
using UnityEngine;

public class Arrow : MonoBehaviour
{

    private Rigidbody2D rigid;
    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();    
    }
    
    public void Set(Transform sPoint, float velocity)
    {
        transform.position = sPoint.position;
        transform.rotation = sPoint.rotation;
        rigid.velocity = sPoint.right * velocity;
    }

    private void Update()
    {
        if(transform.position.y < -5f)
        {
            gameObject.SetActive(false);
        }
        transform.right = rigid.velocity;
    }
}
