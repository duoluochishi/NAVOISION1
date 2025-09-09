//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/15 14:10:27    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------



namespace NV.CT.DicomUtility.DicomCodeStringLib
{
    public class CodeStringItem
    {
        public string CodeValue { get; }
        public string Scheme​ {  get;  }

        public string CodeMeaning { get;  }
        public CodeStringItem( string codeMeaning,string codeValue,string scheme)
        {
            Scheme​ = scheme;
            CodeMeaning = codeMeaning;
            CodeValue = codeValue;
        }
    }
}
