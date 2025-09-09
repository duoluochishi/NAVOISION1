//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.MessageService.Contract;

namespace NV.CT.MessageService.Impl;

public class MessageService : IMessageService
{
    private bool _isStatusMessagePageOpen = false;
    public bool IsStatusMessagePageOpen
    {
        get
        {
            return _isStatusMessagePageOpen;
        }
        private set
        {
            if (_isStatusMessagePageOpen != value)
            {
                _isStatusMessagePageOpen = value;
            }
        }
    }

    public event EventHandler<MessageInfo>? MessageNotify;

    public void SendMessage(MessageInfo messageInfo)
    {
        MessageNotify?.Invoke(this, messageInfo);
    }

    public void StatusMessagePageOpen(string isShow)
    {
        bool bl = false;
        if (bool.TryParse(isShow, out bl))
        {
            IsStatusMessagePageOpen = bl;
        }
    }
}