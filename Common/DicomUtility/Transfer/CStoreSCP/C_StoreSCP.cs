//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using FellowOakDicom.Media;
using FellowOakDicom.Network;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace NV.CT.DicomUtility.Transfer.CStoreSCP
{
    public class C_StoreSCP : DicomService, IDicomServiceProvider, IDicomCStoreProvider, IDicomCEchoProvider
    {
        private DicomDirectory _dicomDir = new DicomDirectory();
        private int _counter = 0;

        private static readonly DicomTransferSyntax[] _acceptedTransferSyntaxes = new DicomTransferSyntax[]
        {
               DicomTransferSyntax.ExplicitVRLittleEndian,
               DicomTransferSyntax.ExplicitVRBigEndian,
               DicomTransferSyntax.ImplicitVRLittleEndian
        };

        private static readonly DicomTransferSyntax[] _acceptedImageTransferSyntaxes = new DicomTransferSyntax[]
        {
               // Lossless
               DicomTransferSyntax.JPEGLSLossless,
               DicomTransferSyntax.JPEG2000Lossless,
               DicomTransferSyntax.JPEGProcess14SV1,
               DicomTransferSyntax.JPEGProcess14,
               DicomTransferSyntax.RLELossless,
               // Lossy
               DicomTransferSyntax.JPEGLSNearLossless,
               DicomTransferSyntax.JPEG2000Lossy,
               DicomTransferSyntax.JPEGProcess1,
               DicomTransferSyntax.JPEGProcess2_4,
               // Uncompressed
               DicomTransferSyntax.ExplicitVRLittleEndian,
               DicomTransferSyntax.ExplicitVRBigEndian,
               DicomTransferSyntax.ImplicitVRLittleEndian
        };

        public C_StoreSCP(       
            INetworkStream stream, 
            Encoding fallbackEncoding, 
            ILogger log, 
            DicomServiceDependencies dependencies)
            : base(stream, fallbackEncoding, log, dependencies)
        {
            
        }       
        

        public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
        {
            HandleMessage(LogLevel.Information, GetFormatedAssociationInfo(association));

            var calledAE = C_StorageSCPAdapter.Instance.GetCalledAETitle();
            var callingAEList = C_StorageSCPAdapter.Instance.GetAcceptedCallingAETitle();

            if (!string.Equals(association.CalledAE,calledAE,StringComparison.OrdinalIgnoreCase))
            {
                HandleMessage(LogLevel.Warning,$"CalledAE无法识别 {association.CalledAE}");
                return SendAssociationRejectAsync(
                    DicomRejectResult.Permanent,
                    DicomRejectSource.ServiceUser,
                    DicomRejectReason.CalledAENotRecognized);
            }

            if(!callingAEList.Any(x=>string.Equals(association.CallingAE, x, StringComparison.OrdinalIgnoreCase)))
            {
                HandleMessage(LogLevel.Warning, $"CalledingAE无法识别 {association.CallingAE}");
                return SendAssociationRejectAsync(
                    DicomRejectResult.Permanent,
                    DicomRejectSource.ServiceUser,
                    DicomRejectReason.CallingAENotRecognized);
            }

            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.Verification)
                {
                    pc.AcceptTransferSyntaxes(_acceptedTransferSyntaxes);
                }
                else if (pc.AbstractSyntax.StorageCategory != DicomStorageCategory.None)
                {
                    pc.AcceptTransferSyntaxes(_acceptedImageTransferSyntaxes);
                }
            }
            
            association.MaxAsyncOpsInvoked = 1;
            association.MaxAsyncOpsPerformed = 1;

            return SendAssociationAcceptAsync(association);
        }


        public Task OnReceiveAssociationReleaseRequestAsync()
        {
            HandleMessage(LogLevel.Information, $"连接释放。$CallingAE:{Association.CallingAE},$RemoteHost:{Association.RemoteHost}。");
            HandleMessage(LogLevel.Information, $"本次连接共完成{_counter}存储请求");
            return SendAssociationReleaseResponseAsync();
        }


        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
            HandleMessage(LogLevel.Warning, $"连接中断。中断来源：{source}，中断原因：{reason}");
        }


        public void OnConnectionClosed(Exception exception)
        {
            HandleMessage(LogLevel.Information, $"连接断开。$CallingAE:{Association.CallingAE},$RemoteHost:{Association.RemoteHost}。");
            if ( exception is not null )
            {
                HandleMessage(LogLevel.Warning, $"连接断开异常信息:\n {exception}");
            }
            C_StorageSCPAdapter.Instance.RaiseStorageComplete(Association, _dicomDir);
        }


        public async Task<DicomCStoreResponse> OnCStoreRequestAsync(DicomCStoreRequest request)
        {
            var studyUid = request.Dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID).Trim();
            var seriesUid = request.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID).Trim();
            var instUid = request.SOPInstanceUID.UID;

            var path = Path.GetFullPath(C_StorageSCPAdapter.Instance.GetStorageRootPath());
            path = Path.Combine(path, studyUid, seriesUid);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, instUid) + ".dcm";
            await request.File.SaveAsync(path);

            //添加当前内容到Dicom目录对象中。
            _dicomDir.AddFile(request.File);
            _counter++;
            return new DicomCStoreResponse(request, DicomStatus.Success);
        }


        public Task OnCStoreRequestExceptionAsync(string tempFileName, Exception e)
        {
            HandleMessage(LogLevel.Warning, $"处理存储请求({tempFileName})失败,异常信息；\n {e}");
            
            return Task.CompletedTask;
        }


        public Task<DicomCEchoResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
        {
            HandleMessage(LogLevel.Information, $"处理来自CEcho请求");

            return Task.FromResult(new DicomCEchoResponse(request, DicomStatus.Success));
        }

        private void HandleMessage(LogLevel level,string msg)
        {
            C_StorageSCPAdapter.Instance.RaiseMessage((int)level, msg);
        }

        private string GetFormatedAssociationInfo(DicomAssociation ass)
        {

            var sb = new StringBuilder();
            sb.AppendFormat("$CallingAE:{0}", ass.CallingAE);
            sb.AppendFormat("$CalledAE:{0}", ass.CalledAE);
            sb.AppendFormat("$Remotehost:{0}", ass.RemoteHost);
            sb.AppendFormat("$Remoteport:{0}", ass.RemotePort);
            sb.AppendFormat("$ImplementationClass:{0}", ass.RemoteImplementationClassUID ?? DicomImplementation.ClassUID);
            sb.AppendFormat("$ImplementationVersion:{0}", ass.RemoteImplementationVersion ?? DicomImplementation.Version);
            sb.AppendFormat("$MaximumPDULength:{0}", ass.MaximumPDULength);
            sb.AppendFormat("$AsyncOpsInvoked:{0}", ass.MaxAsyncOpsInvoked);
            sb.AppendFormat("$AsyncOpsPerformed:{0}", ass.MaxAsyncOpsPerformed);
            sb.AppendFormat("$PresentationContexts:{0}", ass.PresentationContexts.Count);
            foreach (var pc in ass.PresentationContexts)
            {
                sb.AppendFormat("$PresentationContext:{0}[{1}]", pc.ID, pc.Result);
                if (pc.AbstractSyntax.Name != "Unknown")
                {
                    sb.AppendFormat("$$AbstractSyntax:{0}", pc.AbstractSyntax.Name);
                }
                else
                {
                    sb.AppendFormat("$$AbstractSyntax:{0}", pc.AbstractSyntax);
                }

                foreach (var tx in pc.GetTransferSyntaxes())
                {
                    sb.AppendFormat("$$TransferSyntax:{0}", tx.UID.Name);
                }
            }

            if (ass.ExtendedNegotiations.Count > 0)
            {
                sb.AppendFormat("￥ExtendedNegotiations:{0}", ass.ExtendedNegotiations.Count);
                foreach (DicomExtendedNegotiation ex in ass.ExtendedNegotiations)
                {
                    if (ex.SopClassUid.Name != "Unknown")
                    {
                        sb.AppendFormat("$Extended Negotiation:{0}", ex.SopClassUid.Name);
                    }
                    else
                    {
                        sb.AppendFormat("$$Extended Negotiation:{0}", ex.SopClassUid);
                    }

                    if (ex.RequestedApplicationInfo is not null)
                    {
                        sb.AppendFormat("$$ApplicationInfo:{0}", ex.GetApplicationInfo());
                    }

                    if (ex.ServiceClassUid is not null)
                    {
                        sb.AppendFormat("$$ServiceClass:{0}", ex.ServiceClassUid);
                    }

                    if (ex.RelatedGeneralSopClasses.Any())
                    {
                        sb.AppendFormat("$$RelatedSOPClasses:{0}", ex.RelatedGeneralSopClasses.Count);
                        foreach (var rel in ex.RelatedGeneralSopClasses)
                        {
                            sb.AppendFormat("$$$RelatedSOPClass:{0}", rel);
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
