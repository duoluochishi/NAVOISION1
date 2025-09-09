using NV.CT.CTS.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class WorkflowStatusStyleConverter : BasicsConverter
    {
        private const string STYLE_NOTSTARTED = "NotStartedPath";
        private const string STYLE_SCANING = "ScaningPath";
        private const string STYLE_RIGHT = "RightPath";

        /// <summary>
        /// 转换WorkflowStatus为Style
        /// </summary>
        /// <param name="value">必须是WorkflowStatus的值</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">N.A.</param>
        /// <param name="culture"></param>
        /// <returns>Style?</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return null;
            }

            var studyStatus = Enum.Parse<WorkflowStatus>((string)value, true);
            Style? resultStyle;
            switch (studyStatus)
            {
                case WorkflowStatus.NotStarted:
                    resultStyle = Application.Current.Resources[STYLE_NOTSTARTED] as Style;
                    break;
                case WorkflowStatus.Examinating:
                case WorkflowStatus.ExaminationStarting:
                    resultStyle = Application.Current.Resources[STYLE_SCANING] as Style;
                    break;
                case WorkflowStatus.ExaminationClosing:
                case WorkflowStatus.ExaminationClosed:
                    resultStyle = Application.Current.Resources[STYLE_RIGHT] as Style;
                    break;
                case WorkflowStatus.ExaminationDiscontinue:
                    resultStyle = Application.Current.Resources[STYLE_RIGHT] as Style;
                    break;
                default:
                    resultStyle = null;
                    break;
            }

            return resultStyle;
        }

    }
}
