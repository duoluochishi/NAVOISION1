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

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ErrorCodes;
using NV.CT.MessageService.Contract;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.NanoConsole.ViewModel;

public class MessagesViewModel : BaseViewModel
{
    private readonly IMessageService _messageService;
    private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;
    private bool IsUIChange = true;
    private KeyValuePair<int, string> _selectSource;
    public KeyValuePair<int, string> SelectSource
    {
        get => _selectSource;
        set
        {
            if (SetProperty(ref _selectSource, value) && IsUIChange)
            {
                QueryMessageList();
            }
        }
    }

    private ObservableCollection<KeyValuePair<int, string>> _sourceList = new();
    public ObservableCollection<KeyValuePair<int, string>> SourceList
    {
        get => _sourceList;
        set => SetProperty(ref _sourceList, value);
    }

    private bool _isInfo = false;
    public bool IsInfo
    {
        get => _isInfo;
        set
        {
            if (SetProperty(ref _isInfo, value) && IsUIChange)
            {
                QueryMessageList();
            }
        }
    }

    private bool _isWarning = false;
    public bool IsWarning
    {
        get => _isWarning;
        set
        {
            if (SetProperty(ref _isWarning, value) && IsUIChange)
            {
                QueryMessageList();
            }
        }
    }

    private bool _isError = false;
    public bool IsError
    {
        get => _isError;
        set
        {
            if (SetProperty(ref _isError, value) && IsUIChange)
            {
                QueryMessageList();
            }
        }
    }

    private bool _isFatal = false;
    public bool IsFatal
    {
        get => _isFatal;
        set
        {
            if (SetProperty(ref _isFatal, value) && IsUIChange)
            {
                QueryMessageList();
            }
        }
    }

    public string SearchText = string.Empty;

    private List<MessageModel> _sourceMessageList = new();
    public List<MessageModel> SourceMessageList
    {
        get => _sourceMessageList;
        set => SetProperty(ref _sourceMessageList, value);
    }

    private ObservableCollection<MessageModel> _messageList = new();
    public ObservableCollection<MessageModel> MessageList
    {
        get => _messageList;
        set => SetProperty(ref _messageList, value);
    }

    public MessagesViewModel(IMessageService messageService,
        IRealtimeStatusProxyService realtimeStatusProxyService)
    {
        Commands.Add("SearchCommand", new DelegateCommand<string>(SearchList));
        Commands.Add("CloseCommand", new DelegateCommand<object>(OnClosed, _ => true));
        InitComb();
        _messageService = messageService;
        _realtimeStatusProxyService = realtimeStatusProxyService;
        _messageService.MessageNotify -= OnNotifyMessage;
        _messageService.MessageNotify += OnNotifyMessage;
        _realtimeStatusProxyService.DeviceErrorOccurred -= RealtimeStatusProxyService_DeviceErrorOccurred;
        _realtimeStatusProxyService.DeviceErrorOccurred += RealtimeStatusProxyService_DeviceErrorOccurred;
    }

    [UIRoute]
    private void RealtimeStatusProxyService_DeviceErrorOccurred(object? sender, EventArgs<List<string>> e)
    {
        if (e is null || e.Data is null || e.Data.Count == 0)
        {
            return;
        }
        StringBuilder sb = new StringBuilder();
        var maxLevel = ErrorLevel.None;
        foreach (var errorCode in e.Data)
        {
            var error = ErrorCodeHelper.GetErrorCode(errorCode);
            if (error is not null)
            {
                sb.Append($"{error.Code}, Description:{error.Description};");
                if (error.Level > maxLevel)
                {
                    maxLevel = error.Level;
                }
            }
        }
        MessageModel messageModel = new MessageModel();
        messageModel.ID = IdGenerator.Next();
        messageModel.Sender = MessageSource.System;
        messageModel.Level = MessageLevel.Info;
        messageModel.SendTime = DateTime.Now;
        messageModel.Content = sb.ToString();
        if (maxLevel is ErrorLevel.Error)
        {
            messageModel.Level = MessageLevel.Error;
        }
        else if (maxLevel is ErrorLevel.Warning)
        {
            messageModel.Level = MessageLevel.Warning;
        }
        else if (maxLevel is ErrorLevel.Fatal)
        {
            messageModel.Level = MessageLevel.Fatal;
        }
        InsertMessageList(messageModel);
    }

    private void InitComb()
    {
        SourceList = new ObservableCollection<KeyValuePair<int, string>> { new KeyValuePair<int, string>(-1, "All") };
        var list = EnumExtension.EnumToList(typeof(MessageSource));
        foreach (var item in list)
        {
            SourceList.Add(item);
        }
        SelectSource = SourceList[0];
    }

    [UIRoute]
    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        if (e is null)
        {
            return;
        }
        MessageModel messageModel = new MessageModel();
        messageModel.ID = e.Id;
        messageModel.Sender = e.Sender;
        messageModel.Level = e.Level;
        messageModel.SendTime = e.SendTime;
        messageModel.Content = e.Content;

        InsertMessageList(messageModel);
    }

    private void InsertMessageList(MessageModel message)
    {
        if (SourceMessageList.Count >= 20000)
        {
            SourceMessageList.RemoveAt(SourceMessageList.Count - 1);
        }
        SourceMessageList.Insert(0, message);

        QueryMessageList();
    }

    public void SearchList(string searchText)
    {
        SearchText = searchText;

        QueryMessageList();
    }

    private void QueryMessageList()
    {
        MessageList = new ObservableCollection<MessageModel>();
        var query = SourceMessageList.AsQueryable();
        if (SelectSource.Key != -1)
        {
            query = query.Where(t => t.Sender == (MessageSource)SelectSource.Key);
        }
        if (!string.IsNullOrEmpty(SearchText))
        {
            query = query.Where(t => t.Content.Contains(SearchText));
        }
        #region  有空了再来调试这种写法
        //var queryInfo = query.Where(t => t.Level == MessageLevel.Info);
        //var queryWarning = query.Where(t => t.Level == MessageLevel.Warning);
        //var queryError = query.Where(t => t.Level == MessageLevel.Error);
        //var queryFatal = query.Where(t => t.Level == MessageLevel.Fatal);
        //var result = query;
        //if (IsInfo)
        //{
        //    result.Union(queryInfo);
        //}
        //if (IsWarning)
        //{
        //    result.Union(queryWarning);
        //}
        //if (IsError)
        //{
        //    result.Union(queryError);
        //}
        //if (IsFatal)
        //{
        //    result.Union(queryFatal);
        //}
        //MessageList = result.ToList().ToObservableCollection();
        #endregion
        LevelCondition(query.ToList());
    }

    private void LevelCondition(List<MessageModel> query)
    {
        if (IsInfo && !IsWarning && !IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info).ToList();
        }
        if (!IsInfo && IsWarning && !IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Warning).ToList();
        }
        if (!IsInfo && !IsWarning && IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Error).ToList();
        }
        if (!IsInfo && !IsWarning && !IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Fatal).ToList();
        }
        if (IsInfo && IsWarning && !IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Warning).ToList();
        }
        if (IsInfo && !IsWarning && IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Error).ToList();
        }
        if (IsInfo && !IsWarning && !IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Fatal).ToList();
        }
        if (!IsInfo && IsWarning && IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Warning || t.Level == MessageLevel.Error).ToList();
        }
        if (!IsInfo && IsWarning && !IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Warning || t.Level == MessageLevel.Fatal).ToList();
        }
        if (!IsInfo && !IsWarning && IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Error || t.Level == MessageLevel.Fatal).ToList();
        }

        if (IsInfo && IsWarning && IsError && !IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Warning || t.Level == MessageLevel.Error).ToList();
        }
        if (IsInfo && IsWarning && !IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Warning || t.Level == MessageLevel.Fatal).ToList();
        }
        if (IsInfo && !IsWarning && IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Error || t.Level == MessageLevel.Fatal).ToList();
        }
        if (!IsInfo && IsWarning && IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Warning || t.Level == MessageLevel.Error || t.Level == MessageLevel.Fatal).ToList();
        }

        if (IsInfo && IsWarning && IsError && IsFatal)
        {
            query = query.Where(t => t.Level == MessageLevel.Info || t.Level == MessageLevel.Warning || t.Level == MessageLevel.Error || t.Level == MessageLevel.Fatal).ToList();
        }

        MessageList = query.ToObservableCollection();
    }

    public void OnClosed(object parameter)
    {
        if (parameter is Window window)
        {
            _messageService.StatusMessagePageOpen(false.ToString());
            window.Hide();
        }
    }
}