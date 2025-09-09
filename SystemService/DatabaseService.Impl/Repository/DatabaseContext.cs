//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NV.MPS.Environment;

namespace NV.CT.DatabaseService.Impl.Repository
{
    /// <summary>
    /// 数据库操作基本封装
    /// </summary>
    public class DatabaseContext
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseContext> _logger;

        public DatabaseContext(ILogger<DatabaseContext> logger)
        {
            _connectionString = RuntimeConfig.ConnectionString;
            _logger = logger;
        }

        public MySqlConnection Connection
        {
            get
            {
                var connection = new MySqlConnection(_connectionString);
                return connection;
            }
        }
    }

    public static class DapperContextExtension
    {
        public static T ContextExecute<T>(this MySqlConnection connection, Func<MySqlConnection, T> func, ILogger? logger = null)
        {
            try
            {
                connection.Open();
                return func(connection);
            }
            catch(InvalidOperationException invalidException)
            {
                logger?.LogError(invalidException, $"Failed, invalid operation: {connection.ConnectionString}, {invalidException.Message}");
            }
            catch (MySqlException mysqlException)
            {
                logger?.LogError(mysqlException, $"Failed, invalid database operation: {connection.ConnectionString}, {mysqlException.Message}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Database operation exception: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
            return default(T);
        }

        public static T ContextExecute<T>(this MySqlConnection connection, Func<MySqlConnection, MySqlTransaction, T> func, ILogger? logger = null)
        {
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                var result = func(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch (InvalidOperationException invalidException)
            {
                logger?.LogError(invalidException, $"Failed, invalid operation: {connection.ConnectionString}, {invalidException.Message}");
                transaction?.Rollback();
            }
            catch (MySqlException mysqlException)
            {
                logger?.LogError(mysqlException, $"Failed, invalid database operation: {connection.ConnectionString}, {mysqlException.Message}");
                transaction?.Rollback();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Database operation exception: {ex.Message}");
                transaction?.Rollback();
            }
            finally
            {
                connection.Close();
            }
            return default(T);
        }
    }
}
