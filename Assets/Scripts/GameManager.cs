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

    [SerializeField] private Transform arrowStartTr; //화살 시작지점
    [SerializeField] private float arrowSpeed = 10f; //화살 스피드

    private float arrowFireTime;  //화살 발사 가능 시간
    [SerializeField] private float arrowDelay = 0.2f; //화살 쿨

    public int startEffIndex;  //터치 이펙트 시작 인덱스
    public int touchEffCount;  //터치 이펙트 수

    private float checkTouchEffTime;  //터치 이펙트 가능 시간
    public float touchEffectCool = 0.06f; //터치 이펙트 쿨

    public float theftMouseTime = 60f;  //커서 뺏기는 시간
    public bool theftMouse = false;  //커서 뺏긴 상태인지
    private float returnMouseTime;  //커서 반환 시간
    public LineRenderer mStealLine;  

    private bool isInvincible = false;  //커서 안뺏기는 무적상태인지


    public bool canFire = false;  //화살 쏠 수 있는 상태인지
    public bool canTouchEffect = false;  //터치이펙트 나오는 상태인지
    [SerializeField] List<GameObject> patrollingCharList = new List<GameObject>();  //돌아다니는 옵젝 리스트

    private bool changeStateEnabled = true;  //상태 전환 가능한가? (옵젝 끄거나 키기나 화살이나 터치 이펙트 (비)활성화 등 가능한지)

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
        WindowManager.SetWindowFocusing(false);  //게임창에 커서가 포커스되지 않도록 함
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

    private void TouchEffect()  //터치 이펙트
    {
        if (WindowManager.GetKeyDown(WKeyCode.LEFT_BUTTON) || WindowManager.GetKeyDown(WKeyCode.RIGHT_BUTTON)
            || WindowManager.GetKeyDown(WKeyCode.CENTER_BUTTON))
        {
            if (checkTouchEffTime < Time.time && canTouchEffect)
            {
                checkTouchEffTime = Time.time + touchEffectCool;
                PoolManager.GetItem("TouchEff" + Random(startEffIndex, startEffIndex + touchEffCount - 1), mainCam.ScreenToWorldPoint(Input.mousePosition), 2f);
            }
        }
    }

    private int Random(int min, int max) => UnityEngine.Random.Range(min, max + 1);
    private float Random(float min, float max) => UnityEngine.Random.Range(min, max);

    private void LookAtMouse()  //화살 발사를 위해 발사 지점 회전시킴
    {
        if (canFire)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = new Vector2(mousePos.x - arrowStartTr.position.x, mousePos.y - arrowStartTr.position.y);
            arrowStartTr.right = dir;
        }
    }

    private void TryFire()  //화살 발사 시도
    {
        if(WindowManager.GetKey(WKeyCode.LEFT_BUTTON) && arrowFireTime < Time.time && canFire)
        {
            arrowFireTime = Time.time + arrowDelay;
            Arrow arrow = PoolManager.GetItem<Arrow>("Arrow1");
            arrow.Set(arrowStartTr, arrowSpeed);
        }
    }

    private void CheckAttachFlyingMob() //마우스 뺏을 수 있는지 체크
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

    private void StealCursor()  //마우스 커서 뺏음
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

    private void UnsetStealCursor()  //마우스 놓아준 상태로 해줌
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

    public void ChangeActiveChar(int idx)  //해당 인덱스의 옵젝을 키거나 끔
    {
        if (!CanChangeStateEnabled()) return;

        patrollingCharList[idx].SetActive(!patrollingCharList[idx].activeSelf);
       
    }

    private void ChangeStateEnabledTrue()
    {
        changeStateEnabled = true;
    }

    public void SetCanArrowFire()
    {
        if (!CanChangeStateEnabled()) return;
        canFire = !canFire;
    }

    public void SetCanTouchEffect()
    {
        if (!CanChangeStateEnabled()) return;
        canTouchEffect = !canTouchEffect;
    }

    private bool CanChangeStateEnabled()
    {
        if (!changeStateEnabled) return false;

        changeStateEnabled = false;
        Util.DelayFunc(ChangeStateEnabledTrue, 0.5f);
        return true;
    }

    /*private void OnApplicationQuit()
    {
        Save();
    }*/
}
