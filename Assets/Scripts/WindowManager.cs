using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;
using System.Diagnostics;

public class WindowManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, Func<int,IntPtr,IntPtr, IntPtr> callback, IntPtr hInstance, uint threadId);

    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hInstance);

    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr idHook, int Code, int Wparam, int lParam);

    public static IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            UnityEngine.Debug.Log(vkCode);
            instance.sb.AppendLine(((KeyCode)vkCode).ToString());

            return (IntPtr)1;
        }
        else
            return CallNextHookEx(hWndMain, code, (int)wParam, (int)lParam);
    }

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cxTopWidth;
        public int cxBottomWidth;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);



    const int GWL_EXSTYLE = -20;

    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_EX_APPWINDOW = 0x00040000;
    const uint WS_EX_TOOLWINDOW = 0x00000080;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    public static IntPtr hWndMain;

    const uint LWA_COLORKEY = 0x00000001;
    const uint LWA_ALPHA = 0x00000002;

    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;

    public static WindowManager instance;

    public bool focusing;
    public bool hideMode;

    public bool isAlphaZero = true;

    public StringBuilder sb = new StringBuilder();

    public static IntPtr SetHook()
    {
        using(Process curProcess = Process.GetCurrentProcess()){
            using(ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, HookProc, hWndMain, 0);
            }
        }
    }

    public static void RequestWinMessage(string msg, string title)
    {
        MessageBox(hWndMain, msg, title, 0);
    }

    public static void SetWindowAlpha(float rate)
    {
        SetLayeredWindowAttributes(hWndMain, 0, (byte)(rate * 255), LWA_ALPHA);
    }

    public static void SetWindowFocusing(bool focus)
    {
        if (focus)
        {
            SetWindowLong(hWndMain, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);
        }
        else
        {
            SetWindowLong(hWndMain, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }
        if(instance!=null)
            instance.focusing = focus;
    }

    public static void SetCursorPosition(int x, int y)
    {
        SetCursorPos(x, Screen.height - y);
    }

    public static bool GetKeyDown(WKeyCode key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    public static bool GetKeyUp(WKeyCode key)
    {
        return (GetAsyncKeyState((int)key) & 0x0001) != 0;
    }

    public static bool GetKey(WKeyCode key)
    {
        return (GetAsyncKeyState((int)key) & 0x8001) != 0;

        
    }



#if !UNITY_EDITOR

    private void Awake()
    {
        instance = this;

        IntPtr hWnd = GetActiveWindow();
        hWndMain = hWnd;

        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);

        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

        Application.runInBackground = true;
        focusing = true;
    }

    private void Start()
    {
        SetHook();
    }

    private void Update()
    {
        if (GetKeyDown(WKeyCode.Q) && GetKey(WKeyCode.LEFT_CONTROL))
        {
            Application.Quit();
        }

        else if(GetKeyDown(WKeyCode.P) && GetKey(WKeyCode.LEFT_CONTROL))
        {
            isAlphaZero = !isAlphaZero;
            WindowManager.SetWindowAlpha(isAlphaZero ? 0 : 1);
        }
    }

      private void OnDestroy()
    {
        UnhookWindowsHookEx(hWndMain);
    }

#endif

}

public enum WKeyCode
{
    LEFT_BUTTON = 0x01,
    RIGHT_BUTTON = 0x02,
    CENTER_BUTTON = 0x04,
    BACKSPACE = 0x08,
    TAB = 0x09,
    ENTER = 0x0D,
    LEFT_SHIFT = 0xA0,
    RIGHT_SHIFT = 0xA1,
    LEFT_CONTROL = 0xA2,
    RIGHT_CONTROL = 0xA3,
    LEFT_ALT = 0xA4,
    RIGHT_ALT = 0xA5,
    ESCAPE = 0x1B,
    SPACE = 0x20,
    END = 0x23,
    HOME = 0x24,
    LEFT_ARROW = 0x25,
    UP_ARROW = 0x26,
    RIGHT_ARROW = 0x27,
    DOWN_ARROW = 0x28,
    SELECT = 0x29,
    INSERT = 0x2D,
    DELETE = 0x2E,
    HELP = 0x2F,
    ALPHA0 = 0x30,
    ALPHA1 = 0x31,
    ALPHA2 = 0x32,
    ALPHA3 = 0x33,
    ALPHA4 = 0x34,
    ALPHA5 = 0x35,
    ALPHA6 = 0x36,
    ALPHA7 = 0x37,
    ALPHA8 = 0x38,
    ALPHA9 = 0x39,
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,
    NUMPAD0 = 0x60,
    NUMPAD1 = 0x61,
    NUMPAD2 = 0x62,
    NUMPAD3 = 0x63,
    NUMPAD4 = 0x64,
    NUMPAD5 = 0x65,
    NUMPAD6 = 0x66,
    NUMPAD7 = 0x67,
    NUMPAD8 = 0x68,
    NUMPAD9 = 0x69,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,
    F13 = 0x7C,
    F14 = 0x7D,
    F15 = 0x7E,
    F16 = 0x7F,
    F17 = 0x80,
    F18 = 0x81,
    F19 = 0x82,
    F20 = 0x83,
    F21 = 0x84,
    F22 = 0x85,
    F23 = 0x86,
    F24 = 0x87,
}