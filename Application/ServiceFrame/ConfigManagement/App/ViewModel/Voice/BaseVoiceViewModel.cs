//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.ConfigManagement.ViewModel;

public class BaseVoiceViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private int _internalId = 0;
    public int InternalId
    {
        get => _internalId;
        set => SetProperty(ref _internalId, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _bodyPart = string.Empty;
    public string BodyPart
    {
        get => _bodyPart;
        set => SetProperty(ref _bodyPart, value);
    }

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    private bool _isFront = false;
    public bool IsFront
    {
        get => _isFront;
        set => SetProperty(ref _isFront, value);
    }

    public string IsFrontDisalpy
    {
        get
        {
            if (IsFront)
            {
                return "Pre-voice";
            }
            return "Post-voice";
        }
    }

    private int _voiceLength = 0;
    public int VoiceLength
    {
        get => _voiceLength;
        set => SetProperty(ref _voiceLength, value);
    }

    private bool _isFactory = false;
    public bool IsFactory
    {
        get => _isFactory;
        set => SetProperty(ref _isFactory, value);
    }

    private string _language = string.Empty;
    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    private bool _isValid = false;
    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }
}