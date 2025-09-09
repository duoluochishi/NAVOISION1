//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;
using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomSQ
{
    /// <summary>
    /// (0040,A043)
    /// </summary>
    public class ConceptNameCodeSequence:IDicomDatasetUpdater
    {
        #region TYPE1
        public string CodeValue { get; set; }
        public string CodingSchemeDesignator { get; set; }
        public string CodeMeaning { get; set; }

        #endregion TYPE1
        
        public void Update(DicomDataset ds)
        {
            ds.Add(DicomTag.ConceptNameCodeSequence, new DicomCodeItem(CodeValue, CodingSchemeDesignator, CodeMeaning));
        }
        public void Read(DicomDataset ds)
        {
            //Todo: 实现Sequence更新。
        }
    }
}
