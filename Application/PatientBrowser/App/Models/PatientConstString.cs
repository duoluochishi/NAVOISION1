//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/6 13:55:07           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.PatientBrowser.Models;
public class PatientConstString
{
    public const string COMMAND_ADD_NEW = "AddNewCommand";
    public const string COMMAND_SAVE = "SaveCommand";
    public const string COMMAND_DELETE = "DeleteCommand";
    public const string COMMAND_ADD_PROCEDURE = "AddProcedureCommand";
    public const string COMMAND_GO_TO_EXAM = "GoToExamCommand";
    public const string COMMAND_ADD_EMERGENCY_PATIENT = "AddEmergencyPatientCommand";
    public const string COMMAND_CLEAR = "ClearCommand";
    public const string COMMAND_CLICK_PATIENT_ID = "ClickPatientIdCommand";    

    public const string COMMNAD_REFRESH_WORK_LIST = "RefreshWorkListCommand";
    public const string COMMAND_SEARCH = "SearchCommand";

    public const string COMMAND_ADD_ANONYMOUS_PATIENT = "AddAnonymousPatientCommand";
    public const string COMMAND_FAST_GENERATE = "FastGenerateCommand";
    public static string COMMAND_REFRESH_WORKLIST = "RefreshWorkListCommand";

    public const string PATIENT_FIRST_NAME = "First name";
    public const string PATIENT_GENDER = "Gender";
    public const string PATIENT_HEIGHT = "Height";
    public const string PATIENT_WEIGHT = "Weight";
    public const string PATIENT_ADMITTING_DIAGNOSIS = "Admitting diagnosis";
    public const string PATIENT_WARD = "Ward";
    public const string PATIENT_DESCRIPTION = "Description";
    public const string PATIENT_ACCESSION_NO = "Accession no";
    public const string PATIENT_ID = "ID";
    public const string PATIENT_COMMENTS = "Comments";
    public const string PATIENT_INSTITUTION_NAME = "Institution name";
    public const string PATIENT_INSTITUTION_ADDRESS = "Institution address";
    public const string PATIENT_REFERRING_PHSICIAN = "Referring phsician";
    public const string PATIENT_PERFORMING_PHYSICIAN = "Performing physician";

    public const string COMMAND_CLOSED = "CloseCommand";
    public const string COMMAND_FILTER = "FilterCommand";
    public const string COMMAND_SELECT_WORKLIST_PATIENT = "SelectPatientInWorkListCommand";
    public const string COMMAND_CHANGE_SELECTED_DATAGRID_ITEM = "SelectedDataGridItemChangedCommand";
}