//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using FellowOakDicom;


namespace NV.CT.DicomUtility.DicomSQ
{
    /// <summary>
    /// 	(0040,A504)
    /// </summary>
    public class ContentTemplateSequence : IDicomDatasetUpdater
    {
        public MappingResourceCS MappingResource {get;set; }                 //(0008,0105)
           
        public string TemplateIdentifier { get; set; }                      //	(0040,DB00)


        public void Update(DicomDataset ds)
        {
            var content = new DicomDataset();
            content.AddOrUpdate(DicomTag.MappingResource,MappingResource);
            content.AddOrUpdate(DicomTag.TemplateIdentifier,TemplateIdentifier);

            ds.Add(DicomTag.ContentTemplateSequence,content);
        }
        public void Read(DicomDataset ds)
        {
            //Todo: 实现Sequence更新。
        }
    }
}
