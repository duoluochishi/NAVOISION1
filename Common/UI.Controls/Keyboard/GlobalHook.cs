using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System;

namespace NV.CT.UI.Controls.Keyboard;

public abstract class GlobalHook
{
	#region Fields

	protected const byte LLKHF_ALTDOWN = 0x20;
	protected const byte VK_CAPITAL = 0x14;
	protected const byte VK_LALT = 0xA4;
	protected const byte VK_LCONTROL = 0xA2;
	protected const byte VK_LSHIFT = 0xA0;
	protected const byte VK_NUMLOCK = 0x90;
	protected const byte VK_RALT = 0xA5;
	protected const byte VK_RCONTROL = 0x3;
	protected const byte VK_RSHIFT = 0xA1;
	protected const byte VK_SHIFT = 0x10;
	protected const int WH_KEYBOARD = 2;
	protected const int WH_KEYBOARD_LL = 13;
	protected const int WH_MOUSE = 7;
	protected const int WH_MOUSE_LL = 14;
	protected const int WM_KEYDOWN = 0x100;
	protected const int WM_KEYUP = 0x101;
	protected const int WM_LBUTTONDBLCLK = 0x203;
	protected const int WM_LBUTTONDOWN = 0x201;
	protected const int WM_LBUTTONUP = 0x202;
	protected const int WM_MBUTTONDBLCLK = 0x209;
	protected const int WM_MBUTTONDOWN = 0x207;
	protected const int WM_MBUTTONUP = 0x208;
	protected const int WM_MOUSEMOVE = 0x200;
	protected const int WM_MOUSEWHEEL = 0x020A;
	protected const int WM_RBUTTONDBLCLK = 0x206;
	protected const int WM_RBUTTONDOWN = 0x204;
	protected const int WM_RBUTTONUP = 0x205;
	protected const int WM_SYSKEYDOWN = 0x104;
	protected const int WM_SYSKEYUP = 0x105;

	protected int _handleToHook;
	protected HookProc _hookCallback;
	protected int _hookType;
	protected bool _isStarted;

	#endregion Fields

	#region Constructors

	public GlobalHook()
	{
		//Application.Current.Exit.ApplicationExit += new EventHandler(Application_ApplicationExit);

		//Application.Current.Exit += Current_Exit;
	}

	private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
	{
		if (_isStarted)
		{
			Stop();
		}
	}

	#endregion Constructors

	#region Delegates

	protected delegate int HookProc(int nCode, int wParam, IntPtr lParam);

	#endregion Delegates

	#region Properties

	public bool IsStarted => _isStarted;

	#endregion Properties

	#region Methods

	public void Start()
	{
		if (!_isStarted && _hookType != 0)
		{

			// Make sure we keep a reference to this delegate!
			// If not, GC randomly collects it, and a NullReference exception is thrown
			_hookCallback = new HookProc(HookCallbackProcedure);

			//原先写法是这样的写法，这个已经不适用了！
			var firstMoudle = Assembly.GetExecutingAssembly().GetModules()[0];
			//最核心的地方是这里变了，用这个地址是对的
			var changedAddr = System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress;
			_handleToHook = SetWindowsHookEx(
				_hookType,
				_hookCallback,
				//Marshal.GetHINSTANCE(firstMoudle),
				changedAddr,
				0);

			// Were we able to successfully start hook?
			if (_handleToHook != 0)
			{
				_isStarted = true;
			}

			if (_handleToHook == 0)
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new Win32Exception(errorCode);
			}

		}
	}

	public void Stop()
	{
		if (_isStarted)
		{

			UnhookWindowsHookEx(_handleToHook);

			_isStarted = false;

		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall)]
	protected static extern int CallNextHookEx(
		int idHook,
		int nCode,
		int wParam,
		System.IntPtr lParam);

	[DllImport("user32")]
	protected static extern int GetKeyboardState(byte[] pbKeyState);

	[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
	protected static extern short GetKeyState(int vKey);

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	protected static extern int SetWindowsHookEx(
		int idHook,
		HookProc lpfn,
		IntPtr hMod,
		int dwThreadId);

	[DllImport("user32")]
	protected static extern int ToAscii(
		int uVirtKey,
		int uScanCode,
		byte[] lpbKeyState,
		byte[] lpwTransKey,
		int fuState);

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	protected static extern int UnhookWindowsHookEx(int idHook);

	protected void Application_ApplicationExit(object sender, EventArgs e)
	{
		if (_isStarted)
		{
			Stop();
		}
	}

	protected virtual int HookCallbackProcedure(int nCode, Int32 wParam, IntPtr lParam)
	{
		// This method must be overriden by each extending hook
		return 0;
	}

	#endregion Methods

	#region Nested Types

	[StructLayout(LayoutKind.Sequential)]
	protected class KeyboardHookStruct
	{
		public int vkCode;
		public int scanCode;
		public int flags;
		public int time;
		public int dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	protected class MouseHookStruct
	{
		public POINT pt;
		public int hwnd;
		public int wHitTestCode;
		public int dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	protected class MouseLLHookStruct
	{
		public POINT pt;
		public int mouseData;
		public int flags;
		public int time;
		public int dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	protected class POINT
	{
		public int x;
		public int y;
	}

	#endregion Nested Types
}
