//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/5 11:01:27      V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.UI.ViewModel;

namespace NV.CT.UI.Controls.Archive
{
    public class ArchiveModel : BaseViewModel
    {     
        private string _aeCaller = string.Empty;
        public string AECaller
        {
            get
            {
                return _aeCaller;
            }
            set
            {
                SetProperty(ref _aeCaller, value);
            }
        }

        private string _aeTitle = string.Empty;
        public string AETitle
        {
            get
            {
                return _aeTitle;
            }
            set
            {
                SetProperty(ref _aeTitle, value);
            }
        }

        private string _host = string.Empty;
        public string Host
        {
            get
            {
                return _host;
            }
            set
            {
                SetProperty(ref _host, value);
            }
        }

        private string _port = string.Empty;
        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
                SetProperty(ref _port, value);
            }
        }

        private string _transferSyntax = string.Empty;
        public string TransferSyntax
        {
            get
            {
                return _transferSyntax;
            }
            set
            {
                SetProperty(ref _transferSyntax, value);
            }
        }

    }
}
