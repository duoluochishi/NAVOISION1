//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.Transfer;
using NV.CT.JobService.Contract;
using NV.CT.JobService.Contract.Model;
using NV.CT.MessageService.Contract;
using System.IO;

namespace NV.CT.JobService;

public class DicomFileService : IDicomFileService
{
    #region Members 

    private readonly IMapper _mapper;
    private readonly IStudyService _studyService;
    private readonly IJobTaskService _jobTaskService;
    private readonly IMessageService _messageService;
    private readonly ILogger<DicomFileService> _logger;

    #endregion

    #region Events

    public event EventHandler<List<ImageInstanceModel>>? LoadImageInstancesCompleted;

    #endregion

    #region Constructor

    public DicomFileService(IJobTaskService jobTaskService, 
                            IStudyService studyService, 
                            IMessageService messageService, 
                            IMapper mapper, 
                            ILogger<DicomFileService> logger)
    {
        _mapper = mapper;
        _studyService = studyService;
        _messageService = messageService;
        _jobTaskService = jobTaskService;
        _logger = logger;
    }

    #endregion

    #region Public methods

    public LoadImageInstanceCommandResult LoadImageInstances(string pathName)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            _logger.LogWarning("Invalid parameter of empty pathName");
            return new LoadImageInstanceCommandResult { Status = CommandExecutionStatus.Failure };
        }

        bool isFile = File.Exists(pathName);
        bool isDirectory = Directory.Exists(pathName);

        //if it is neither a file nor a diretory, then return.
        if (!isFile && !isDirectory)
        {
            _logger.LogWarning($"Invalid parameter of pathName:{pathName}");
            return new LoadImageInstanceCommandResult { Status = CommandExecutionStatus.Failure };
        }

        var fileList = new List<string>();
        if (isDirectory)
        {
            var dir = new DirectoryInfo(pathName);
            fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
        }
        else if (isFile)
        {
            fileList.Add(pathName);
        }

        Task.Run(() => { ResolveImageInstanceFiles(fileList.ToArray()); 
                       });

        return new LoadImageInstanceCommandResult { Status = CommandExecutionStatus.Success };
    }

    public JobTaskCommandResult UpdateDICOM(UpdateDicomRequest request)
    {
        if(request is null)
        {
            this._logger.LogDebug("Empty parameter request of UpdateDICOM");
            return new JobTaskCommandResult { Status = CommandExecutionStatus.Failure };
        }

        var fileList = new List<string>();
        foreach (string seriesPath in request.SeriesFolders)
        {
            if (File.Exists(seriesPath))
            {
                fileList.Add(seriesPath);
            }
            else if(Directory.Exists(seriesPath))
            {
                var dir = new DirectoryInfo(seriesPath);
                fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
            }
        }

        var dicomUpdateInfo = new DicomUpdateInfo(request.PatientID, 
                                                  request.PatientName,
                                                  request.PatientBirthDate,
                                                  request.PatientBirthTime,
                                                  request.PatientSex,
                                                  request.PatientAge,
                                                  request.PatientSize,
                                                  request.PatientWeight,
                                                  request.AccessionNumber,
                                                  request.ReferringPhysicianName,
                                                  request.StudyDescription);
        this.UpdateDICOMFiles(fileList.ToArray(), dicomUpdateInfo);

        return new JobTaskCommandResult { Status = CommandExecutionStatus.Success };

    }

    #endregion

    #region Private Methods      

    private void ResolveImageInstanceFiles(string[] imageFiles)
    {
        var imageInstances = new List<ImageInstanceModel>();
        foreach (var imageFile in imageFiles)
        {
            //validate file
            if (!DicomFile.HasValidHeader(imageFile))
            {
                continue;
            }

            try
            {
                var dicomFile = DicomFile.Open(imageFile);
                if (!dicomFile.Dataset.Contains(DicomTag.StudyID))
                {
                    continue;
                }

                if (!dicomFile.Dataset.TryGetString(DicomTag.InstanceNumber, out var instanceNumber))
                {
                    continue;
                }

                if (!int.TryParse(instanceNumber, out var number))
                {
                    continue;
                }

                var imageInstanceModel = new ImageInstanceModel();
                imageInstanceModel.Id = number.ToString();
                imageInstanceModel.ImageNumber = number;
                imageInstanceModel.ImageTime = (new FileInfo(imageFile)).LastWriteTime;
                imageInstanceModel.Path = imageFile;

                imageInstances.Add(imageInstanceModel);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"ResolveImageInstanceFiles: failed to open DICOM file:{imageFile}. The exception is:{ex.Message}");
                continue;
            }
        }

        LoadImageInstancesCompleted?.Invoke(this, imageInstances.OrderBy(imageInstance => imageInstance.ImageNumber).ToList());
    }

    private void UpdateDICOMFiles(string[] imageFiles, DicomUpdateInfo dicomUpdateInfo)
    {
        foreach (var imageFile in imageFiles)
        {
            //validate file
            if (!DicomFile.HasValidHeader(imageFile))
            {
                continue;
            }

            try
            {
                var dicomFile = DicomFile.Open(imageFile,FileReadOption.ReadAll);
                var newFile = dicomFile.Clone();                

                var dataSet = newFile.Dataset;
                if (!dataSet.Contains(DicomTag.StudyID))
                {
                    continue;
                }

                if (!dataSet.TryGetString(DicomTag.InstanceNumber, out var instanceNumber))
                {
                    continue;
                }

                if (!int.TryParse(instanceNumber, out var number))
                {
                    continue;
                }

                newFile.Dataset.AddOrUpdate(DicomTag.PatientID, dicomUpdateInfo.PatientID);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientName, dicomUpdateInfo.PatientName);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientBirthDate, dicomUpdateInfo.PatientBirthDate);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientBirthTime, dicomUpdateInfo.PatientBirthTime);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientAge, dicomUpdateInfo.PatientAge);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientSex, dicomUpdateInfo.PatientSex);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientSize, dicomUpdateInfo.PatientSize);
                newFile.Dataset.AddOrUpdate(DicomTag.PatientWeight, dicomUpdateInfo.PatientWeight);
                newFile.Dataset.AddOrUpdate(DicomTag.ReferringPhysicianName, dicomUpdateInfo.ReferringPhysicianName);
                newFile.Dataset.AddOrUpdate(DicomTag.AccessionNumber, dicomUpdateInfo.AccessionNumber);
                newFile.Dataset.AddOrUpdate(DicomTag.StudyDescription, dicomUpdateInfo.StudyDescription);

                //验证后，在保存前删除原文件
                dicomFile.File.Delete();
                newFile.Save(imageFile, FellowOakDicom.IO.Writer.DicomWriteOptions.Default);

            }
            catch (Exception ex)
            {
                _logger.LogDebug($"UpdateDICOMFiles: failed to update DICOM file:{imageFile}. The exception is:{ex.Message}");
                continue;
            }
        }
    }

    #endregion

}