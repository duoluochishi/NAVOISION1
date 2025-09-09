//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/17 13:16:10     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS
{
    public class Constants
    {
        public const string SOP_CLASS_UID_SR = "1.2.840.10008.5.1.4.1.1.88.67";
        public const string SERIES_TYPE_SR = "Dose SR";
        public const string SERIES_TYPE_IMAGE = "image";
        public const string SERIES_TYPE_DOSE_REPORT = "DoseReport";
        public const string IMAGE_TYPE_TOMO = "Tomo";
        public const string IMAGE_TYPE_TOPO = "Topo";
        public const string TOPOGRAM = "Topogram";
        public const string DOSE_REPORT = "Dose Report";
        public const string TEMPORARY_BURNING_PATH = "TempBurn";
        public const string NAME_OF_BURNING = "Nanovision DICOM Viewer";

        public const string REGEX_ILLEGAL_CHARACTER = "^[0-9a-zA-Z\u4E00-\u9FA5]+$";
        public const string REGEX_ILLEGAL_CHARACTER_ALLOW_SINGLE_QUOTES = "^[0-9a-zA-Z\u4E00-\u9FA5']+$";
        public const string REGEX_ILLEGAL_CHARACTER_ALLOW_UNDERLINE = "^[0-9a-zA-Z\u4E00-\u9FA5_]+$";

        public const string DICOM_SEX_MALE = "M";
        public const string DICOM_SEX_FEMALE = "F";
        public const string DICOM_SEX_OTHER = "O";

        public const string DICOM_AGETYPE_YEAR = "Y";
        public const string DICOM_AGETYPE_MONTH = "M";
        public const string DICOM_AGETYPE_WEEK = "W";
        public const string DICOM_AGETYPE_DAY = "D";

        public const string DICOM_ORIENTATION_PORTRAIT = "PORTRAIT";
        public const string DICOM_ORIENTATION_LANDSCAPE = "LANDSCAPE";

        public const string SPECIFIC_CHARACTER_SET = "ISO_IR 192";
    }
}
