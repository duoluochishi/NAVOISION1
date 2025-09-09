using Microsoft.Extensions.Logging;
using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace NV.CT.NP.Tools.DataTransfer.Service
{
    public class SqliteService : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _dbPath;

        private SQLiteConnection _sharedConnection;
        private readonly object _lock = new object();

        private bool _disposed = false;

        ILogger<SqliteService> _logger;

        public SqliteService(string dbPath, ILogger<SqliteService> logger)
        {
            _dbPath = dbPath;
            _connectionString = $"Data Source={_dbPath};Version=3;";
            _logger = logger;

            InitializeDatabase();
            InitializeSharedConnection();
            _logger.LogInformation($"SqliteService initialized");
        }

        private void InitializeSharedConnection()
        {
            try
            {
                _sharedConnection = new SQLiteConnection(_connectionString);
                _sharedConnection.Open();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitializeSharedConnection failed!");
                throw;
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                }

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS t_export (
                            PatientId TEXT NOT NULL,
                            StudyInstanceUID TEXT NOT NULL,
                            JobStatus TEXT,
                            ErrorMessage TEXT,
                            PRIMARY KEY(PatientId, StudyInstanceUID)
                        )";

                    using (var command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitializeDatabase failed!");
                throw;
            }
        }

        private SQLiteConnection GetSharedConnection()
        {
            lock (_lock)
            {
                if (_sharedConnection == null || _sharedConnection.State != System.Data.ConnectionState.Open)
                {
                    InitializeSharedConnection();
                }
                return _sharedConnection;
            }
        }

        public (string jobStatus, string errorMessage) GetJobStatusByPatientIdAndStudyUid(string patientId, string studyInstanceUid)
        {
            try
            {
                var connection = GetSharedConnection();

                string query = @"SELECT JobStatus, ErrorMessage 
                                 FROM t_export 
                                 WHERE PatientId = @PatientId AND StudyInstanceUID = @StudyInstanceUid";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.Add("@PatientId", System.Data.DbType.String).Value = patientId;
                    command.Parameters.Add("@StudyInstanceUid", System.Data.DbType.String).Value = studyInstanceUid;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (reader["JobStatus"]?.ToString(), reader["ErrorMessage"]?.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Search JobStatus ErrorMessage failed: {patientId}, {studyInstanceUid}");
            }

            return (null, null);
        }

        public void InsertExportRecord(string patientId, string studyInstanceUid, string jobStatus, string errorMessage)
        {
            try
            {
                var connection = GetSharedConnection();

                string upsertQuery = @"
                    INSERT OR REPLACE INTO t_export (PatientId, StudyInstanceUID, JobStatus, ErrorMessage)
                    VALUES (@PatientId, @StudyInstanceUID, @JobStatus, @ErrorMessage)";

                using (var command = new SQLiteCommand(upsertQuery, connection))
                {
                    command.Parameters.Add("@PatientId", System.Data.DbType.String).Value = patientId;
                    command.Parameters.Add("@StudyInstanceUID", System.Data.DbType.String).Value = studyInstanceUid;
                    command.Parameters.Add("@JobStatus", System.Data.DbType.String).Value = jobStatus;
                    command.Parameters.Add("@ErrorMessage", System.Data.DbType.String).Value = errorMessage;

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Insert or Update failed: {patientId},{studyInstanceUid}, {jobStatus}, {errorMessage}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放共享连接
                if (_sharedConnection != null)
                {
                    lock (_lock)
                    {
                        if (_sharedConnection.State != System.Data.ConnectionState.Closed)
                        {
                            _sharedConnection.Close();
                        }
                        _sharedConnection.Dispose();
                        _sharedConnection = null;
                    }
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}