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

    public int startEffIndex;
    public int touchEffCount;

    private float checkTouchEffTime;

    public float theftMouseTime = 60f;
    public bool theftMouse = false;
    private float returnMouseTime;
    public LineRenderer mStealLine;

    private bool isInvincible = false;


    public bool canFire = false;
    public bool canTouchEffect = false;
    [SerializeField] List<GameObject> patrollingCharList = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        for(int i = 0; i < poolObjDataList.Length; i++)
        {
            PoolManager.CreatePool(poolObjDataList[i]); 
        }
        mainCam = Camera.main;
        mStealLine.gameObject.SetActive(false);
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
        TouchEffect();
        CheckAttachFlyingMob();
        StealCursor();
    }

    private void TouchEffect()
    {
        if (WindowManager.GetKeyDown(WKeyCode.LEFT_BUTTON) || WindowManager.GetKeyDown(WKeyCode.RIGHT_BUTTON)
            || WindowManager.GetKeyDown(WKeyCode.CENTER_BUTTON))
        {
            if (checkTouchEffTime < Time.time && canTouchEffect)
            {
                checkTouchEffTime = Time.time + 0.1f;
                PoolManager.GetItem("TouchEff" + Random(startEffIndex, startEffIndex + touchEffCount - 1), mainCam.ScreenToWorldPoint(Input.mousePosition), 2f);
            }
        }
    }

    private int Random(int min, int max) => UnityEngine.Random.Range(min, max + 1);
    private float Random(float min, float max) => UnityEngine.Random.Range(min, max);

    private void LookAtMouse()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = new Vector2(mousePos.x - arrowStartTr.position.x, mousePos.y - arrowStartTr.position.y);
        arrowStartTr.right = dir;
    }

    private void TryFire()
    {
        if(WindowManager.GetKey(WKeyCode.LEFT_BUTTON) && arrowFireTime < Time.time && canFire)
        {
            arrowFireTime = Time.time + arrowDelay;
            Arrow arrow = PoolManager.GetItem<Arrow>("Arrow1");
            arrow.Set(arrowStartTr, arrowSpeed);
        }
    }

    private void CheckAttachFlyingMob()
    {
        if(!theftMouse && patrollingCharList[0].activeSelf && !isInvincible)
        {
            if(Vector2.Distance(patrollingCharList[0].transform.position, mainCam.ScreenToWorldPoint(Input.mousePosition))< 0.3f)
            {
                theftMouse = true;
                returnMouseTime = Time.time + theftMouseTime;
                mStealLine.gameObject.SetActive(true);
            }
        }
        else if(theftMouse)
        {
            if(returnMouseTime < Time.time)
            {
                UnsetStealCursor();
            }
        }
    }

    private void StealCursor()
    {
        if(theftMouse)
        {
            Vector2 scrPos = mainCam.WorldToScreenPoint(patrollingCharList[0].transform.position);
            WindowManager.SetCursorPosition((int)scrPos.x, (int)scrPos.y);
            mStealLine.SetPosition(0, patrollingCharList[0].transform.position);
            mStealLine.SetPosition(1, mainCam.ScreenToWorldPoint(Input.mousePosition));
        }

        if (WindowManager.GetKeyDown(WKeyCode.HOME) && WindowManager.GetKey(WKeyCode.LEFT_CONTROL))
        {
            UnsetStealCursor();
        }
    }

    private void UnsetStealCursor()
    {
        theftMouse = false;
        mStealLine.gameObject.SetActive(false);
        isInvincible = true;
        Util.DelayFunc(() => isInvincible = false, 1.5f);
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
            //Debug.LogError(e.ToString());
            
        }
    }

    public void ChangeActiveChar(int idx)
    {
        patrollingCharList[idx].SetActive(!patrollingCharList[idx].activeSelf);
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
