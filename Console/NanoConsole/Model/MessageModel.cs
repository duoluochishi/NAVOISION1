//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/11/6 16:35:59    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.NanoConsole.Model;

public class MessageModel : BaseViewModel
{
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private MessageSource _sender = MessageSource.Unknown;
    public MessageSource Sender
    {
        get => _sender;
        set => SetProperty(ref _sender, value);
    }

    public string SenderDisplay
    {
        get => Sender.ToString("");
    }

    private MessageLevel _level = MessageLevel.Info;
    public MessageLevel Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    public string LevelDisplay
    {
        get => Level.ToString();
    }

    private DateTime _sendTime = DateTime.Now;
    public DateTime SendTime
    {
        get => _sendTime;
        set => SetProperty(ref _sendTime, value);
    }

    public string SendTimeDisplay
    {
        get => SendTime.ToString("HH:mm:ss");
    }

    private string _content = null;
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}