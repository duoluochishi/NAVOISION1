//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Entities;
using System;
using System.Text;

namespace NV.CT.DatabaseService.Impl.Repository
{
    public class PatientRepository //: IPatientRepository
    {
        private DatabaseContext _context;
        private readonly ILogger<PatientRepository> _logger;

        public PatientRepository(DatabaseContext context, ILogger<PatientRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Insert(PatientEntity entity)
        {
            //_logger.LogInformation("Insert patient");
            string sql = "insert into t_patient (Id,PatientId,PatientName,PatientSex,PatientBirthDate,CreateTime,Creator,UpdateTime,Updater) value (@Id,@PatientId,@PatientName,@PatientSex,@PatientBirthDate,@CreateTime,@Creator,@UpdateTime,@Updater);";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    ID = entity.Id,
                    PatientID = entity.PatientId,
                    PatientSex = entity.PatientSex,
                    PatientName = entity.PatientName,
                    PatientBirthDate = entity.PatientBirthDate,
                    CreateTime = entity.CreateTime,
                    Creator = entity.Creator,
                    UpdateTime = entity.UpdateTime,
                    Updater = entity.Updater,
                });
            }, _logger);
            return result == 1;
        }
        public bool Update(PatientEntity entity)
        {
            //_logger.LogInformation("Update patient");
            string sql = "UPDATE t_patient p SET p.PatientId=@PatientId,p.PatientSex=@PatientSex,p.PatientName=@PatientName,p.PatientBirthDate=@PatientBirthDate,UpdateTime=@UpdateTime,Updater=@Updater WHERE p.Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    PatientID = entity.PatientId,
                    PatientSex = entity.PatientSex,
                    PatientName = entity.PatientName,
                    PatientBirthDate = entity.PatientBirthDate,
                    UpdateTime = entity.UpdateTime,
                    Updater = entity.Updater,
                    ID = entity.Id,
                });
            }, _logger);
            return result == 1;
        }

        public bool Delete(PatientEntity entity)
        {
            //_logger.LogInformation("Delete patient");
            string sql = "delete from t_patient  WHERE t_patient.Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    ID = entity.Id
                });
            }, _logger);
            return result == 1;
        }

        public PatientEntity Get(string patientId)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<PatientEntity>("select * from t_patient where patientId = @PatientId", new { PatientId = patientId });
            }, _logger);
            return entity;
        }

        public PatientEntity GetPatientById(string id)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<PatientEntity>("select * from t_patient where Id = @Id", new { Id = id });
            }, _logger);
            return entity;
        }

        public bool DeleteByGuid(string patientGuid)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("delete from t_patient_correction where InternalPatientId = @PatientGuid;");
            sb.AppendLine("delete from t_patient where Id = @PatientGuid;");
            _context.Connection.ContextExecute((connection) => {
                var effectRows = connection.Execute(sb.ToString(), new
                {
                    PatientGuid = patientGuid
                });
                return effectRows > 0;
            }, _logger);
            return true;
        }

        public bool GotoEmergencyExamination(PatientEntity patientEntity, StudyEntity studyEntity)
        {
            return _context.Connection.ContextExecute((connection, transaction) =>
            {
                string sql = "insert into t_patient (Id,PatientId,PatientName,PatientSex,PatientBirthDate,CreateTime,Creator,UpdateTime,Updater) value(@Id,@PatientId,@PatientName,@PatientSex,@PatientBirthDate,@CreateTime,@Creator,@UpdateTime,@Updater);";
                connection.Execute(sql, new
                {
                    ID = patientEntity.Id,
                    PatientID = patientEntity.PatientId,
                    PatientSex = patientEntity.PatientSex,
                    PatientName = patientEntity.PatientName,
                    PatientBirthDate = patientEntity.PatientBirthDate,
                    CreateTime = patientEntity.CreateTime,
                    Creator = patientEntity.Creator,
                    UpdateTime = patientEntity.UpdateTime,
                    Updater = patientEntity.Updater,

                }, transaction);
                sql = "insert into t_study (Id,InternalPatientId,Age,AgeType,PatientSize,StudyInstanceUID,PatientWeight,AdmittingDiagnosisDescription,Ward,ReferringPhysicianName,BodyPart,AccessionNo,StudyId,Comments,InstitutionName,InstitutionAddress,StudyStatus,PatientType,StudyTime,CreateTime,Creator,UpdateTime,Updater,IsLocalModified) value (@Id,@InternalPatientId,@Age,@AgeType,@PatientSize,@StudyInstanceUID,@PatientWeight,@AdmittingDiagnosisDescription,@Ward,@ReferringPhysicianName,@BodyPart,@AccessionNo,@StudyId,@Comments,@InstitutionName,@InstitutionAddress,@StudyStatus,@PatientType,@StudyTime,@CreateTime,@Creator,@UpdateTime,@Updater,@IsLocalModified);";
                connection.Execute(sql, new
                {
                    Id = studyEntity.Id,
                    InternalPatientId = studyEntity.InternalPatientId,
                    Age = studyEntity.Age,
                    AgeType = studyEntity.AgeType,
                    PatientSize = studyEntity.PatientSize,
                    PatientWeight = studyEntity.PatientWeight,
                    StudyInstanceUID = studyEntity.StudyInstanceUID,
                    AdmittingDiagnosisDescription = studyEntity.AdmittingDiagnosisDescription,
                    Ward = studyEntity.Ward,
                    ReferringPhysicianName = studyEntity.ReferringPhysicianName,
                    BodyPart = studyEntity.BodyPart,
                    AccessionNo = studyEntity.AccessionNo,
                    StudyId = studyEntity.StudyId,
                    Comments = studyEntity.Comments,
                    InstitutionName = studyEntity.InstitutionName,
                    InstitutionAddress = studyEntity.InstitutionAddress,
                    StudyStatus = studyEntity.StudyStatus,
                    PatientType = studyEntity.PatientType,
                    StudyTime = studyEntity.StudyTime,
                    CreateTime = studyEntity.CreateTime,
                    Creator = studyEntity.Creator,
                    UpdateTime = studyEntity.UpdateTime,
                    Updater = studyEntity.Updater,
                    IsLocalModified = studyEntity.IsLocalModified,
                }, transaction);
                return true;
            });
        }

        public List<PatientEntity> GetExistingPatientList(string patientId, string patientName, Gender sex, DateTime birthday)
        {
            string sql = "SELECT Id, PatientId, PatientName, PatientSex, PatientBirthDate, IsDeleted, CreateTime, Creator, UpdateTime, Updater, PatientComments FROM t_patient WHERE IsDeleted = 0 AND PatientId = @PatientId AND PatientName = @PatientName AND PatientSex=@PatientSex";
            var entities = _context.Connection.ContextExecute((connection) => {
                return connection.Query<PatientEntity>(sql, new { PatientId = patientId, PatientName = patientName, PatientSex = (int)sex });
            }, _logger);

            //说明：Patient表的历史数据存在Birthday的时分秒部分不为0的情况，导致数据库的Where条件匹配性能很差，所以使用LINQ基于缩小范围的查询结果来提高查询性能
            var dateOfBirthday = birthday.Date;
            return entities.Where(p => p.PatientBirthDate.Date == dateOfBirthday).ToList();  
        }
    }
}
