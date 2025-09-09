//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.UI.Exam.Contract;

namespace NV.CT.Examination.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddExaminationAppContainer(this ContainerBuilder builder)
        {
            builder.RegisterType<DicomImageViewModel>().As<IDicomImageViewModel>().SingleInstance();
        }
    }
}
