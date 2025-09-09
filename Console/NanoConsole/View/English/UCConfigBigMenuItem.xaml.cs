//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.View.English;

public partial class UCConfigBigMenuItem : UserControl
{
    #region Fields

    /// <summary>
    /// 页面信息
    /// </summary>
    private string _pagename = string.Empty;

    public string ClickUri = string.Empty;

    /// <summary>
    /// 鼠标悬浮控件border颜色.
    /// </summary>
    private SolidColorBrush hover = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A5A89"));

    /// <summary>
    /// 鼠标离开控件border颜色.
    /// </summary>
    private SolidColorBrush leave = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16162B"));
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(UCConfigBigMenuItem), new PropertyMetadata(""));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(UCConfigBigMenuItem), new PropertyMetadata(""));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(UCConfigBigMenuItem), new PropertyMetadata(""));

    #endregion Fields
    public UCConfigBigMenuItem()
    {
        InitializeComponent();
    }

    [Category("外观"), Browsable(true), Description("图标路径")]
    public Geometry Icon
    {
        get => (Geometry)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    [Category("外观"), Browsable(true), Description("标题")]
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [Category("外观"), Browsable(true), Description("项说明")]
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #region Methods

    /// <summary>
    /// 初始化控件.
    /// </summary>
    /// <param name="name">配置菜单名字.</param>
    /// <param name="describe">配置菜单描述.</param>
    /// <param name="imgUri">The imgUri<see cref="string"/>.</param>
    /// <param name="pageName">点击后要打开的页面名称.</param>
    //public void InitUI(string name, string describe, string imgUri, string pageName)
    //{
    //    this.lblName.Content = name;
    //    this.lblDescribe.Content = describe;
    //    this.path.Data = (Geometry)new ResourceDictionary { Source = new Uri(@"pack://application:,,,/NV.CT.UI.Controls;component/Style/Themes/Geometries.xaml", UriKind.RelativeOrAbsolute) }[imgUri];
    //    this.path.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A5A89"));
    //    _pagename = pageName;
    //    ClickUri = pageName;
    //}

    //public Geometry GetIcon(string icon)
    //{
    //    if (!String.IsNullOrEmpty(icon))
    //    {
    //        return (Geometry)new ResourceDictionary { Source = new Uri(@"pack://application:,,,/NV.CT.UI.Controls;component/Style/Themes/Geometries.xaml", UriKind.RelativeOrAbsolute) }[icon];
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    /// <summary>
    /// 按钮悬浮事件.
    /// </summary>
    /// <param name="sender">.</param>
    /// <param name="e">.</param>
    private void Border_MouseEnter(object sender, MouseEventArgs e)
    {
        borderName.BorderBrush = hover;
        lblName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        lblDescribe.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        path.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
    }

    /// <summary>
    /// 按钮离开事件.
    /// </summary>
    /// <param name="sender">.</param>
    /// <param name="e">.</param>
    private void BorderName_MouseLeave(object sender, MouseEventArgs e)
    {
        borderName.BorderBrush = leave;
        lblName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A5A89"));
        lblDescribe.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A5A89"));
        path.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A5A89"));
    }

    /// <summary>
    /// 切换功能模块
    /// </summary>
    /// <param name="sender">.</param>
    /// <param name="e">.</param>
    private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var type = Type.GetType(_pagename);
        if (type == null)
        {
            return;
        }
        //switch (type.Name)
        //{
        //    case "ProtocolManagement":
        //        ProtocolManagementTreeView protocolManagement = new ProtocolManagementTreeView();
        //        protocolManagement.WindowState = WindowState.Maximized;
        //        protocolManagement.ShowDialog();
        //        break;

        //    case "DoseConfig":
        //        DoseConfig doseConfig = new DoseConfig();
        //        doseConfig.WindowState = WindowState.Maximized;
        //        doseConfig.ShowDialog();
        //        break;

        //    case "ViewLogInfo":
        //        ViewLogInfo viewLogInfo = ViewLogInfo.GetInstance(); ;
        //        viewLogInfo.WindowState = WindowState.Maximized;
        //        viewLogInfo.ShowDialog();
        //        break;

        //    case "RoutineManagement":
        //        RoutineManagement routineManagement = new RoutineManagement();
        //        routineManagement.WindowState = WindowState.Maximized;
        //        routineManagement.ShowDialog();
        //        break;

        //    case "NotifyInfo":
        //        NotifyInfo notifyInfo = new NotifyInfo();
        //        notifyInfo.WindowState = WindowState.Maximized;
        //        notifyInfo.ShowDialog();
        //        break;

        //    case "SystemSetting":
        //        SystemSetting systemSetting = new SystemSetting();
        //        systemSetting.WindowState = WindowState.Maximized;
        //        systemSetting.ShowDialog();
        //        break;

        //    case "HardWareUpdate":
        //        if (CT.CST.Common.DataCommon.oServiceToolCommunicationProxy.CheckAlive("Exam"))
        //        {
        //            CT.CST.Common.DataCommon.oServiceToolCommunicationProxy.SendString("", "Exam", (ushort)(MessageID.EXAM_MESSAGE_ID_MRSUPDATE), false);
        //            Commonlog.Debug("HardWareUpdate>>EXAM_MESSAGE_ID_MRSUPDATE");
        //        }
        //        else
        //        {
        //            CommonDialog.ShowMessage(this, "Tip", "MRS is not connected.");
        //        }
        //        break;

        //    case "ComponentManagement":
        //        ComponentManagement compma = new ComponentManagement();
        //        compma.WindowState = WindowState.Maximized;
        //        compma.ShowDialog();
        //        break;

        //    case "UserConfig":
        //        UserConfig userConfig = new UserConfig();
        //        userConfig.WindowState = WindowState.Maximized;
        //        userConfig.ShowDialog();
        //        break;

        //    case "CloudData":
        //        break;

        //    case "LockLogin":
        //        LockLogin locklogin = new LockLogin();
        //        locklogin.ShowDialog();
        //        break;
        //}
    }

    /// <summary>
    /// 启动升级进程
    /// </summary>
    private void StartUpgradeTool()
    {
        //ProcessStartInfo si = new ProcessStartInfo();
        //si.Arguments = String.Format("{0}", "True");
        //si.FileName = AppDomain.CurrentDomain.BaseDirectory + "CT.CST.Upgrade.exe";
        //Process ps = new Process();
        //ps.StartInfo = si;
        //ps.Start();
    }

    /// <summary>
    /// 判断升级进程是否启动
    /// </summary>
    private void StartUpgradeProcess()
    {
        //Process[] psL = Process.GetProcessesByName("CT.CST.Upgrade");
        //if ((psL == null) || (psL.Length == 0))
        //{
        //    StartUpgradeTool();
        //}
    }

    /// <summary>
    /// 判断返回状态调用启动升级进程方法
    /// </summary>
    /// <param name="obj">The obj<see cref="object"/>.</param>
    private void UpdateProcessStart(object obj)
    {
        if (obj.ToString() == "OK")
        {
            StartUpgradeProcess();
        }
    }

    #endregion Methods
}
