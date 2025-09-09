using NV.CT.CTS.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class PatientTypeStyleConverter : BasicsConverter
    {
        private const string RESOURCE_PATIENT_TYPE_EMERGENCY = "PatientTypeEmergency";
        private const string RESOURCE_PATIENT_TYPE_PREREGISTERED = "PatientTypePreRegistered";
        private const string RESOURCE_PATIENT_TYPE_LOCAL = "PatientTypeLocal";

        /// <summary>
        /// 转换PatientType为Style
        /// </summary>
        /// <param name="value">必须是PatientType对应的Int类型值</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">N.A.</param>
        /// <param name="culture"></param>
        /// <returns>Style?</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || value is not int)
            {
                return null;
            }

            var targetDiskType = (PatientType)value;
            Style? resultStyle;
            switch(targetDiskType)
            {
                case PatientType.Emergency:
                    resultStyle = (Style)Application.Current.Resources[RESOURCE_PATIENT_TYPE_EMERGENCY];
                    break;
                case PatientType.PreRegistration:
                    resultStyle = (Style)Application.Current.Resources[RESOURCE_PATIENT_TYPE_PREREGISTERED];
                    break;
                case PatientType.Local:
                    resultStyle = (Style)Application.Current.Resources[RESOURCE_PATIENT_TYPE_LOCAL];
                    break;
                default:
                    resultStyle = null;
                    break;
            }
            return resultStyle;
        }

    }
}
