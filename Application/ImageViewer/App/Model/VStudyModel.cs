//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.ImageViewer.Model;

public class VStudyModel : BaseViewModel
{
	private string _id = string.Empty;
	private string _studyId = string.Empty;
	private string _patientName = string.Empty;
	private string _firstName = string.Empty;
	private string _lastName = string.Empty;
	private string _patientId = string.Empty;
	private string _bodyPart = string.Empty;
	private DateTime? _examStartTime;
	private DateTime? _examEndTime;
	private DateTime? _createTime;
	private Gender _patientSex;
	private string _studyStatus=string.Empty;
	private int? _patientType;

	public int? PatientType
	{
		get => _patientType;
		set => SetProperty(ref _patientType, value);
	}

	public Gender PatientSex
	{
		get => _patientSex;
		set => SetProperty(ref _patientSex, value);
	}

	public string PatientSexString => PatientSex.ToString();

	public string PatientTypeString
	{
		get
		{
			var str = string.Empty;
			switch (PatientType)
			{
				case (int)NV.CT.CTS.Enums.PatientType.Emergency:
					str = "Emergency patient";
					break;
				case (int)NV.CT.CTS.Enums.PatientType.PreRegistration:
					str = "Pre-registered patient";
					break;
				case (int)NV.CT.CTS.Enums.PatientType.Local:
				default:
					str = "Local patient";
					break;
			}
			return str;
		}
	}

	public string StudyStatus
	{
		get => _studyStatus;
		set => SetProperty(ref _studyStatus, value);
	}

	public string Id
	{
		get => _id;
		set => SetProperty(ref _id, value);
	}
	public string StudyId
	{
		get => _studyId;
		set => SetProperty(ref _studyId, value);
	}

	public string PatientName
	{
		get => _patientName;
		set => SetProperty(ref _patientName, value);
	}

	public string FirstName
	{
		get => _firstName;
		set => SetProperty(ref _firstName, value);
	}

	public string LastName
	{
		get => _lastName;
		set => SetProperty(ref _lastName, value);
	}

	public string PatientId
	{
		get => _patientId;
		set => SetProperty(ref _patientId, value);
	}
	public string BodyPart
	{
		get => _bodyPart;
		set => SetProperty(ref _bodyPart, value);
	}
	public DateTime? ExamStartTime
	{
		get => _examStartTime;
		set => SetProperty(ref _examStartTime, value);
	}
	public DateTime? ExamEndTime
	{
		get => _examEndTime;
		set => SetProperty(ref _examEndTime, value);
	}
	public DateTime? CreateTime
	{
		get => _createTime;
		set => SetProperty(ref _createTime, value);
	}

	private DateTime? _birthday;
	public DateTime? Birthday
	{
		get => _birthday;
		set => SetProperty(ref _birthday, value);
	}
}