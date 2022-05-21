using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowOpacity : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = Color.green;
    }

    private void Start()
    {
        cam =Camera.main;
    }

    void Update()
    {
        float mouseDistance = Vector2.Distance(cam.ScreenToWorldPoint(Input.mousePosition), transform.position);
        if (mouseDistance < 0.2f)
        {
            WindowManager.SetWindowFocusing(true);
            WindowManager.SetWindowAlpha(1);
            if (Input.GetMouseButtonDown(0))
            {
                if (WindowManager.instance.focusing)
                {
                    sr.color = Color.red;
                    WindowManager.instance.focusing = false;
                }
                else
                {
                    sr.color = Color.green;
                    WindowManager.instance.focusing = true;
                }
            }
        }
        else
        {
            if (!WindowManager.instance.focusing)
            {
                WindowManager.SetWindowFocusing(false);
                {
                    Color temp = sr.color;
                    temp.a = Mathf.Clamp((2.5f - mouseDistance) / 2.5f, 0f, 1f);
                    sr.color = temp;
                }
                if (WindowManager.instance.hideMode)
                {
                    WindowManager.SetWindowAlpha(Mathf.Clamp((2.5f - mouseDistance) / 2.5f, 0f, 1f));
                }
            }
        }
    }
}
