using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Impl;
using NV.CT.DatabaseService.Impl.Repository;
using NV.CT.NP.Tools.DataTransfer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.Utils
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Create RawDataService
        /// </summary>
        /// <param name="mapper">IMapper instance</param>
        /// <param name="repository">SeriesRepository instance</param>
        /// <returns>SeriesService</returns>
        public ISeriesService CreateSeriesService(
            IMapper mapper,
            SeriesRepository repository)
        {
            var assembly = typeof(StudyService).Assembly;
            var seriesServiceType = assembly.GetType(ConstStrings.SERIESSERVICE_TYPENAME);

            Type loggerType = typeof(ILogger<>).MakeGenericType(seriesServiceType);
            var logHelperType = typeof(LogHelper<>).MakeGenericType(seriesServiceType);
            var createLoggerMethod = logHelperType.GetMethod("CreateLogger", new[] { typeof(string) });
            var logger = createLoggerMethod.Invoke(null, new object[] { seriesServiceType.Name });

            return (ISeriesService)CreateInstance(seriesServiceType, new Type[] { typeof(IMapper), typeof(SeriesRepository), loggerType }, new object[] { mapper, repository, logger });
        }

        /// <summary>
        /// Create RawDataService
        /// </summary>
        /// <param name="mapper">IMapper instance</param>
        /// <param name="repository">RawDataRepository instance</param>
        /// <returns>RawDataService实例</returns>
        public IRawDataService CreateRawDataService(
            IMapper mapper,
            RawDataRepository repository)
        {
            var assembly = typeof(StudyService).Assembly;
            var rawDataServiceType = assembly.GetType(ConstStrings.RAWDATASERVICE_TYPENAME);

            Type loggerType = typeof(ILogger<>).MakeGenericType(rawDataServiceType);
            var logHelperType = typeof(LogHelper<>).MakeGenericType(rawDataServiceType);
            var createLoggerMethod = logHelperType.GetMethod("CreateLogger", new[] { typeof(string) });
            var logger = createLoggerMethod.Invoke(null, new object[] { rawDataServiceType.Name });

            return (IRawDataService)CreateInstance(rawDataServiceType, new Type[] { typeof(IMapper), loggerType, typeof(RawDataRepository) }, new object[] { mapper, logger, repository });
        }

        /// <summary>
        /// Create instance via reflection
        /// </summary>
        /// <returns>instace</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private object CreateInstance(Type serviceType, Type[] parameterTypes, object[] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            // Ctor finder
            ConstructorInfo constructor = serviceType.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                parameterTypes,
                null
            );

            if (constructor == null)
            {
                throw new MissingMethodException(
                    $"Ctor find failed. parameterTypes: {string.Join(", ", parameterTypes.Select(t => t?.Name))}");
            }

            // create instance
            return constructor.Invoke(parameters);
        }

    }
}
