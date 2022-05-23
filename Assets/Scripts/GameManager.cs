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

    [SerializeField] private Transform arrowStartTr; //ȭ�� ��������
    [SerializeField] private float arrowSpeed = 10f; //ȭ�� ���ǵ�

    private float arrowFireTime;  //ȭ�� �߻� ���� �ð�
    [SerializeField] private float arrowDelay = 0.2f; //ȭ�� ��

    public int startEffIndex;  //��ġ ����Ʈ ���� �ε���
    public int touchEffCount;  //��ġ ����Ʈ ��

    private float checkTouchEffTime;  //��ġ ����Ʈ ���� �ð�
    public float touchEffectCool = 0.06f; //��ġ ����Ʈ ��

    public float theftMouseTime = 60f;  //Ŀ�� ����� �ð�
    public bool theftMouse = false;  //Ŀ�� ���� ��������
    private float returnMouseTime;  //Ŀ�� ��ȯ �ð�
    public LineRenderer mStealLine;  

    private bool isInvincible = false;  //Ŀ�� �Ȼ���� ������������


    public bool canFire = false;  //ȭ�� �� �� �ִ� ��������
    public bool canTouchEffect = false;  //��ġ����Ʈ ������ ��������
    [SerializeField] List<GameObject> patrollingCharList = new List<GameObject>();  //���ƴٴϴ� ���� ����Ʈ

    private bool changeStateEnabled = true;  //���� ��ȯ �����Ѱ�? (���� ���ų� Ű�⳪ ȭ���̳� ��ġ ����Ʈ (��)Ȱ��ȭ �� ��������)

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
        WindowManager.SetWindowFocusing(false);  //����â�� Ŀ���� ��Ŀ������ �ʵ��� ��
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

    private void TouchEffect()  //��ġ ����Ʈ
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

    private void LookAtMouse()  //ȭ�� �߻縦 ���� �߻� ���� ȸ����Ŵ
    {
        if (canFire)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = new Vector2(mousePos.x - arrowStartTr.position.x, mousePos.y - arrowStartTr.position.y);
            arrowStartTr.right = dir;
        }
    }

    private void TryFire()  //ȭ�� �߻� �õ�
    {
        if(WindowManager.GetKey(WKeyCode.LEFT_BUTTON) && arrowFireTime < Time.time && canFire)
        {
            arrowFireTime = Time.time + arrowDelay;
            Arrow arrow = PoolManager.GetItem<Arrow>("Arrow1");
            arrow.Set(arrowStartTr, arrowSpeed);
        }
    }

    private void CheckAttachFlyingMob() //���콺 ���� �� �ִ��� üũ
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

    private void StealCursor()  //���콺 Ŀ�� ����
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

    private void UnsetStealCursor()  //���콺 ������ ���·� ����
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

    public void ChangeActiveChar(int idx)  //�ش� �ε����� ������ Ű�ų� ��
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
