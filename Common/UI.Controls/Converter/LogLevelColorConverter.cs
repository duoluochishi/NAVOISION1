using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter
{
    public class LogLevelColorConverter : BasicsConverter
    {
        private const string NORMAL_COLOR = "#FFFFFF";
        private const string FATLE_COLOR = "#FF0000";
        private const string LEVEL_ERR = "ERR";
        private const string LEVEL_FTL = "FTL";
        private const string LEVEL_TRC = "TRC";
        private const string LEVEL_DBG = "DBG";
        private const string LEVEL_INF = "INF";
        private const string LEVEL_WRN = "WRN";

        /// <summary>
        /// 转换LogLevel类型为颜色值
        /// </summary>
        /// <param name="value">LogLevel类型值：</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Style?</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //按项目现有规范，颜色十六进制采用长度6，不使用长度8(含透明度)。此变量改为由外部参数传入，以提高灵活性。
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return NORMAL_COLOR;
            }  

            var logLevel = value.ToString();
            var resultColor = string.Empty;
            switch (logLevel)
            {
                case LEVEL_ERR:
                case LEVEL_FTL:
                    resultColor = FATLE_COLOR;
                    break;
                case LEVEL_TRC:
                case LEVEL_DBG:
                case LEVEL_INF:
                case LEVEL_WRN:
                default:
                    resultColor = NORMAL_COLOR;
                    break;
            }
            return resultColor;
        }
    }
}
