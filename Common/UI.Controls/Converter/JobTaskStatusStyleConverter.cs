using NV.CT.CTS.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class JobTaskStatusStyleConverter : BasicsConverter
    {
        private const string JOB_STATUS_STYLE_PROCESSING = "JobStatusProcessing";
        private const string JOB_STATUS_STYLE_COMPLETED = "JobStatusCompleted";
        private const string JOB_STATUS_STYLE_PARTLY_COMPLETED = "JobStatusPartlyCompleted";
        private const string JOB_STATUS_STYLE_ERROR = "JobStatusError";

        /// <summary>
        /// 转换JobTaskStatus为Style
        /// </summary>
        /// <param name="value">必须是JobTaskStatus的值</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">N.A.</param>
        /// <param name="culture"></param>
        /// <returns>Style?</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || value is not JobTaskStatus)
            {
                return null;
            }

            var jobTaskStatus = (JobTaskStatus)value;
            Style? resultStyle;
            switch (jobTaskStatus)
            {
                case JobTaskStatus.Queued:
                case JobTaskStatus.Processing:
                    resultStyle = Application.Current.Resources[JOB_STATUS_STYLE_PROCESSING] as Style;
                    break;
                case JobTaskStatus.Completed:
                    resultStyle = Application.Current.Resources[JOB_STATUS_STYLE_COMPLETED] as Style;
                    break;
                case JobTaskStatus.PartlyCompleted:
                    resultStyle = Application.Current.Resources[JOB_STATUS_STYLE_PARTLY_COMPLETED] as Style;
                    break;
                case JobTaskStatus.Failed:
                case JobTaskStatus.Cancelled:
                case JobTaskStatus.Paused:
                    resultStyle = Application.Current.Resources[JOB_STATUS_STYLE_ERROR] as Style;
                    break;
                default:
                    resultStyle = null;
                    break;
            }
            return resultStyle;
        }

    }
}
