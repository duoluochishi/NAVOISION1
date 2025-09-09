using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NV.CT.Screenshot;

[System.Security.SuppressUnmanagedCodeSecurity]
public class HotKey
{
    #region Constants

    /// <summary>
    /// 热键消息.....
    /// </summary>
    internal const int WM_HOTKEY = 0x312;

    #endregion Constants

    #region Fields

    /// <summary>
    /// Defines the action.
    /// </summary>
    private Action action;
    /// <summary>
    /// Defines the source.
    /// </summary>
    private HwndSource source;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Prevents a default instance of the <see cref="HotKey"/> class from being created.
    /// </summary>
    /// <param name="window">The window <see cref="Window"/>.</param>
    /// <param name="modifiers">The modifiers <see cref="ModifierKeys"/>.</param>
    /// <param name="key">The key <see cref="Keys"/>.</param>
    /// <param name="action">The action <see cref="Action"/>.</param>
    private HotKey(Window window, ModifierKeys modifiers, Keys key, Action action)
    {
        Modifiers = modifiers;
        Key = key;
        this.action = action;
        try
        {
            var helper = new WindowInteropHelper(window);
            var hwnd = helper.Handle;
            source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);
            var strKey = GetString(modifiers, key);
            Id = GlobalFindAtom(strKey);
            if (Id != 0)
            {
                UnregisterHotKey(hwnd, Id);
            }
            else
            {
                Id = GlobalAddAtom(strKey);
            }
            if (!RegisterHotKey(hwnd, Id, modifiers, key))
                throw new Exception();
        }
        catch (Exception e)
        {
            throw new HotKeyRegisterFailException(e);
        }
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Gets the Id.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// Gets the Key.
    /// </summary>
    public Keys Key { get; }
    /// <summary>
    /// Gets the Modifiers.
    /// </summary>
    public ModifierKeys Modifiers { get; }

    #endregion Properties


    public static HotKey Register(Window window, ModifierKeys modifiers, Keys key, Action action)
    {
        return new HotKey(window, modifiers, key, action);
    }

    public void Unregister()
    {
        UnregisterHotKey(source.Handle, Id);
        GlobalDeleteAtom(GetString(Modifiers, Key));
        source.RemoveHook(WndProc);
    }

    internal static string GetString(ModifierKeys modifiers, Keys key) => $"Saar:{modifiers}+{key}";

    /// <summary>
    /// 向原子表中添加全局原子.
    /// </summary>
    [DllImport("kernel32", SetLastError = true)]
    internal static extern ushort GlobalAddAtom(string lpString);

    /// <summary>
    /// 在表中删除全局原子.
    /// </summary>
    [DllImport("kernel32", SetLastError = true)]
    internal static extern ushort GlobalDeleteAtom(string nAtom);

    /// <summary>
    /// 在表中搜索全局原子.
    /// </summary>
    [DllImport("kernel32", SetLastError = true)]
    internal static extern ushort GlobalFindAtom(string lpString);

    /// <summary>
    /// 注册热键.
    /// </summary>
    /// <param name="hWnd">The hWnd <see cref="IntPtr"/>.</param>
    /// <param name="id">The id <see cref="int"/>.</param>
    /// <param name="fsModifuers">The fsModifuers <see cref="ModifierKeys"/>.</param>
    /// <param name="vk">The vk <see cref="Keys"/>.</param>
    /// <returns>The <see cref="bool"/>.</returns>
    [DllImport("user32", SetLastError = true)]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifuers, Keys vk);

    /// <summary>
    /// 注销热键.
    /// </summary>
    /// <param name="hWnd">The hWnd <see cref="IntPtr"/>.</param>
    /// <param name="id">The id <see cref="int"/>.</param>
    /// <returns>The <see cref="bool"/>.</returns>
    [DllImport("user32", SetLastError = true)]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>
    /// The WndProc.
    /// </summary>
    /// <param name="hwnd">The hwnd <see cref="IntPtr"/>.</param>
    /// <param name="msg">The msg <see cref="int"/>.</param>
    /// <param name="wParam">The wParam <see cref="IntPtr"/>.</param>
    /// <param name="lParam">The lParam <see cref="IntPtr"/>.</param>
    /// <param name="handle">The handle <see cref="bool"/>.</param>
    /// <returns>The <see cref="IntPtr"/>.</returns>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
    {
        if (msg == WM_HOTKEY && (int)wParam == Id)
        {
            action();
            handle = true;
        }
        return IntPtr.Zero;
    }
}

public class HotKeyRegisterFailException : Exception
{
    public HotKeyRegisterFailException(Exception e) : base("注册热键失败。", e)
    {
    }

    public HotKeyRegisterFailException(string message, Exception e) : base(message, e)
    {
    }

}