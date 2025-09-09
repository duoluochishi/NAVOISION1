//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/3/14 10:21:11    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS;

public static class SystemPermissionNames
{
    #region PermissionName
    public const string SECURITY_SETTING_USER_MANAGEMENT = "UserManagement";

    public const string SECURITY_SETTING_VIEW_AUDIT_LOG = "ViewAuditLog";

    public const string SECURITY_SETTING_ARCHIVE_AUDIT_LOG = "ArchiveAuditLog";

    public const string SECURITY_SETTING_DELETE_AUDIT_LOG = "DeleteAuditLog";

    public const string PATIENT_REGISTRATION_CREATE_NEW_EXAM = "CreateNewExam";

    public const string PATIENT_REGISTRATION_EMERGENCY_EXAM = "EmergencyExam";

    public const string EXAM_EXAM = "Exam";

    public const string EXAM_EXCEED_ALERT_SCAN = "ExceedAlertScan";

    public const string EXAM_OFFLINE_RECON = "OfflineRecon";

    public const string PATIENT_MANAGEMENT_ARCHIVE = "Archive";

    public const string PATIENT_MANAGEMENT_EXPORT = "Export";

    public const string PATIENT_MANAGEMENT_IMPORT = "Import";

    public const string PATIENT_MANAGEMENT_QUERY_RETRIEVE = "QueryRetrieve";

    public const string PATIENT_MANAGEMENT_EXPORT_RAW = "ExportRaw";

    public const string PATIENT_MANAGEMENT_IMPORT_RAW = "ImportRaw";

    public const string PATIENT_MANAGEMENT_CORRECT_INFO = "CorrectInfo";

    public const string PATIENT_MANAGEMENT_DATA_PROTECTION = "DataProtection";

    public const string PATIENT_MANAGEMENT_DATA_DELETION = "DataDeletion";

    public const string PATIENT_MANAGEMENT_PRINT = "Print";

    public const string PATIENT_MANAGEMENT_IMAGE_VIEWER = "ImageViewer";

    public const string CLINICAL_TOOL_PROTOCOL_MANAGEMENT = "ProtocolManagement";

    public const string CLINICAL_TOOL_IMAGE_TEXT_CONFIG = "ImageTextConfig";

    public const string CLINICAL_TOOL_PATIENT_REGIST_CONFIG = "PatientRegistConfig";

    public const string CLINICAL_TOOL_FILM_CONFIG = "FilmConfig";

    public const string CLINICAL_TOOL_WINDOWING_PRESET = "WindowingPreset";

    public const string CLINICAL_TOOL_AUTO_ARCHIVE_CONFIG = "AutoArchiveConfig";

    public const string CLINICAL_TOOL_AUTO_PRINT_CONFIG = "AutoPrintConfig";

    public const string CLINICAL_TOOL_AUTO_DELELE_CONFIG = "AutoDeleleConfig";

    public const string CLINICAL_TOOL_HOSPITAL_SETTING = "HospitalSetting";

    public const string CLINICAL_TOOL_MAINAE_CONFIG = "MainAEConfig";

    public const string CLINICAL_TOOL_DICOM_CONFIG = "DicomNodeConfig";

    public const string CLINICAL_TOOL_COMMON_SETTING = "CommonSetting";

    public const string CLINICAL_TOOL_LOG_MANAGEMENT = "LogManagement";

    public const string DEVICE_MAINTENANCE_DAILY_CALIBRATION = "DailyCalibration";

    public const string DEVICE_MAINTENANCE_DEVICE_MAINTAIN = "DeviceMaintain";

    public const string DEVICE_MAINTENANCE_BACKUP_RESTORE = "BackupRestore";

    public const string DEVICE_MAINTENANCE_SERVICE_MAINTAIN = "ServiceMaintain";

    public const string DEVICE_MAINTENANCE_DEVICE_TEST = "DeviceTest";

    public const string DEVICE_MAINTENANCE_DEV_DEBUG = "DevDebug";

    public const string SYSTEM_PARAM_SETTING_SYSTEM_CONFIG = "SystemConfig";

    public const string SYSTEM_PARAM_SETTING_MODE_TYPE_CONFIG = "ModeTypeConfig";
    #endregion

    #region Factory RoleName
    public const string ROLENAME_ADMINISTRATOR = "Administrator";

    public const string ROLENAME_SERVICEENGINEER = "ServiceEngineer";

    public const string ROLENAME_DEVICEMANAGER = "DeviceManager";

    public const string ROLENAME_SENIOR = "Senior";

    public const string ROLENAME_OPERATOR = "Operator";

    public const string ROLENAME_ENGINEER = "Engineer";

    public const string ROLENAME_EMERGENCY = "Emergency";
    #endregion
}