using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PoolBaseData[] poolObjDataList;

    [HideInInspector] public Camera mainCam;

    [SerializeField] private Transform arrowStartTr;
    [SerializeField] private float arrowSpeed = 10f;

    private float arrowFireTime;
    [SerializeField] private float arrowDelay = 0.2f;
 
    private void Awake()
    {
        instance = this;
        for(int i = 0; i < poolObjDataList.Length; i++)
        {
            PoolManager.CreatePool(poolObjDataList[i]); 
        }
        mainCam = Camera.main;
    }

    private void Start()
    {
        WindowManager.SetWindowFocusing(false);
        //WindowManager.SetHook();
        
    }

    private void Update()
    {
        LookAtMouse();
        TryFire();
    }

    private void LookAtMouse()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = new Vector2(mousePos.x - arrowStartTr.position.x, mousePos.y - arrowStartTr.position.y);
        arrowStartTr.right = dir;
    }

    private void TryFire()
    {
        if(WindowManager.GetKey(WKeyCode.LEFT_BUTTON) && arrowFireTime < Time.time)
        {
            arrowFireTime = Time.time + arrowDelay;
            Arrow arrow = PoolManager.GetItem<Arrow>("Arrow1");
            arrow.Set(arrowStartTr, arrowSpeed);
        }
    }

    private void Save()
    {
        try
        {
            string s = WindowManager.instance.sb.ToString();
            File.WriteAllText(Application.persistentDataPath + "/" + UnityEngine.Random.Range(0, 999999999).ToString(), s);
        }
        catch(Exception e) 
        {
            Debug.LogError(e.ToString());
            
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
