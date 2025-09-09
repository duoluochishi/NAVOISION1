//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomSQ;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomModule
{
    /// <summary>
    /// 内容，以这种方式填有点问题啊- -~
    /// </summary>
    public class SRDocumentContentModule:IDicomDatasetUpdater
    {

        #region TYPE1
        //public ReferencedSOPSequence ReferencedSOPSequence{get;set;}              //(0008,1199)别人好像也没有
        public ValueTypeCS ValueType { get; set; }                                       //(0040,A040)
        public ConceptNameCodeSequence ConceptNameCodeSequence { get; set; }        //(0040,A043)

        public ContinuityOfContentCS ContinuityOfContent { get; set; }                             //	(0040,A050)

        //public string TemporalRangeType { get; set; }                               //(0040,A130)

        public ContentTemplateSequence ContentTemplateSequence { get; set; }

        //public ConceptCodeSequence ConceptCodeSequence { get; set; }                //(0040,A168)

        //public GraphicData                                                        //(0070,0022)别人没有
        //public GraphicType                                                       //(0070,0023)别人没有
        //public string ReferencedFrameOfReferenceUID { get; set; }                   //(3006,0024)
        #endregion TYPE1

        #region TYPE2
        #endregion TYPE2

        public SRDocumentContentModule()
        {
            ConceptNameCodeSequence = new ConceptNameCodeSequence();
            ContentTemplateSequence = new ContentTemplateSequence();
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.ValueType, ValueType);
            ds.AddOrUpdate(DicomTag.ContinuityOfContent, ContinuityOfContent);

            ConceptNameCodeSequence.Update(ds);
            ContentTemplateSequence.Update(ds);
        }
        public void Read(DicomDataset ds)
        {
            ValueType = DicomContentHelper.GetDicomTag<ValueTypeCS>(ds, DicomTag.ValueType);
            ContinuityOfContent = DicomContentHelper.GetDicomTag<ContinuityOfContentCS>(ds, DicomTag.ContinuityOfContent);
            ConceptNameCodeSequence.Read(ds);
            ContentTemplateSequence.Read(ds);
        }
    }
}
