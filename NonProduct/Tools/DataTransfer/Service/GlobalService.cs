using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Impl;
using NV.CT.DatabaseService.Impl.Repository;
using NV.CT.NP.Tools.DataTransfer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.NP.Tools.DataTransfer.Service
{
    public class GlobalService
    {
        private static Lazy<GlobalService> _instance = new Lazy<GlobalService>(() => new GlobalService());
        private IPatientService _patientService;
        private IStudyService _studyService;
        private ISeriesService _seriesService;
        private IRawDataService _rawDataService;
        private IScanTaskService _scanTaskService;
        private IReconTaskService _reconTaskService;
        private SqliteService _sqliteService;

        public GlobalService()
        {
            var dataProfile = new ToDataProfile();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(dataProfile);
            });

            // Initialize database context
            IMapper mapper = config.CreateMapper();
            var dbLogger = LogHelper<DatabaseContext>.CreateLogger("DatabaseContext");
            DatabaseContext databaseContext = new DatabaseContext(dbLogger);

            // initialize repositories
            var patientRepoLogger = LogHelper<PatientRepository>.CreateLogger("PatientRepository");
            PatientRepository patientRepository = new PatientRepository(databaseContext, patientRepoLogger);

            var seriesRepoLogger = LogHelper<SeriesRepository>.CreateLogger("SeriesRepository");
            SeriesRepository seriesRepository = new SeriesRepository(databaseContext, seriesRepoLogger);

            var studyRepoLogger = LogHelper<StudyRepository>.CreateLogger("StudyRepository");
            StudyRepository studyRepository = new StudyRepository(databaseContext, studyRepoLogger, patientRepository, seriesRepository);

            var rawDataRepoLogger = LogHelper<RawDataRepository>.CreateLogger("RawDataRepository");
            RawDataRepository rawDataRepository = new RawDataRepository(databaseContext, rawDataRepoLogger);

            var scanTaskRepoLogger = LogHelper<ScanTaskRepository>.CreateLogger("ScanTaskRepository");
            ScanTaskRepository scanTaskRepository = new ScanTaskRepository(databaseContext, scanTaskRepoLogger);

            var reconTaskRepoLogger = LogHelper<ReconTaskRepository>.CreateLogger("ReconTaskRepository");
            ReconTaskRepository reconTaskRepository = new ReconTaskRepository(databaseContext, reconTaskRepoLogger);

            // then initialize services
            _patientService = new PatientService(patientRepository);

            var studySvcLogger = LogHelper<StudyService>.CreateLogger("StudyService");
            _studyService = new StudyService(mapper, patientRepository, studyRepository, studySvcLogger);

            var scanTaskSvcLogger = LogHelper<ScanTaskService>.CreateLogger("ScanTaskService");
            _scanTaskService = new ScanTaskService(mapper, scanTaskRepository, scanTaskSvcLogger);

            var reconTaskSvcLogger = LogHelper<ReconTaskService>.CreateLogger("ReconTaskService");
            _reconTaskService = new ReconTaskService(mapper, reconTaskRepository, seriesRepository, reconTaskSvcLogger);

            ReflectionHelper reflectionHelper = new ReflectionHelper();
            _seriesService = reflectionHelper.CreateSeriesService(mapper, seriesRepository);
            _rawDataService = reflectionHelper.CreateRawDataService(mapper, rawDataRepository);

            // then initialize sqlite service
            var sqliteSvcLogger = LogHelper<SqliteService>.CreateLogger("SqliteService");
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstStrings.SQLITE_NAME);
            _sqliteService = new SqliteService(dbPath, sqliteSvcLogger);
        }

        public static GlobalService Instance => _instance.Value;

        public IPatientService PatientService => _patientService;

        public IStudyService StudyService => _studyService;

        public ISeriesService SeriesService => _seriesService;

        public IRawDataService RawDataService => _rawDataService;

        public IScanTaskService ScanTaskService => _scanTaskService;

        public IReconTaskService ReconTaskService => _reconTaskService;

        public SqliteService SqliteService => _sqliteService;

        public void RunOnUI(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }       
    }
}
