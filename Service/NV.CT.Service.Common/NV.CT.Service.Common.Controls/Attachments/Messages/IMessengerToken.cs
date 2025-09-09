namespace NV.CT.Service.Common.Controls.Attachments.Messages
{
    /// <summary>
    /// 标识具有消息Token，用于区分来自不同源的消息
    /// </summary>
    public interface IMessengerToken
    {
        string MessengerToken { get; set; }
    }

}
