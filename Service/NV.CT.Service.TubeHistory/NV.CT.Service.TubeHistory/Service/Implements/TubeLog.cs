using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeHistory.Service.Models;
using NV.CT.Service.TubeHistory.Service.Interfaces;
using NV.MPS.Configuration;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using AutoMapper.Internal;
using NV.MPS.Environment;
using NV.CT.Service.TubeHistory.Enums;
using System.Net.Http.Headers;

namespace NV.CT.Service.TubeHistory.Service.Implements
{
    public partial class TubeLog : ITubeLog
    {
        #region Field

        private readonly ILogService _logger;
        //private readonly IConfigService _configService;
        private int _tubeCount = 24;
        #endregion
        private string _path = "E:\\Logs\\MRS\\";
        private long _GetLogProgress;

        private IList<int> _tubeID;
        private IList<string> _tubeSN;
        private List<Task> _logTasks = new List<Task>();
        private List<string> _logFiles;
        private IList<IList<ComponentInfo>> _tubeHistory = new List<IList<ComponentInfo>>();
        private IList<IList<IList<TubeLogEntityModel>>> _listArray = new List<IList<IList<TubeLogEntityModel>>>();

        public event Action<int, int> TubeLogProgressListHandler;
        public TubeLog(ILogService logger)
        {
            _logger = logger;
            _tubeCount = SystemConfig.DeviceComponentConfig.XRaySourceTankboxes.Count;
            _path = RuntimeConfig.Console.MRSLog.Path;
        }



        private bool GetParameters(ScanParam t, string line)
        {
            if (t == null)
            {
                return false;
            }
            PropertyInfo[] properties = t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return false;
            }
            int startIndex = 0;
            int endIndex = 0;
            foreach (PropertyInfo item in properties)
            {
                string name = item.Name;
                endIndex = line.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (endIndex <= 0) continue;
                if (item.PropertyType.IsArray)
                {
                    startIndex = line.IndexOf('[', endIndex);
                    endIndex = line.IndexOf(']', startIndex);
                    name = line.Substring(startIndex + 1, endIndex - startIndex - 1).Replace("\"", "");
                    item.SetValue(t, name.Split(","));
                }
                else
                {
                    startIndex = line.IndexOf(":", endIndex);
                    endIndex = line.IndexOf(",", startIndex);
                    name = line.Substring(startIndex + 1, endIndex - startIndex - 1).Replace("\"", "");
                    item.SetValue(t, name);
                }
            }
            return true;
        }


        public IEnumerable<IList<TubeLogEntityModel>> GetTubeLog(DateTime? startTime, DateTime? endTime, IList<int> tubeID, IList<string> tubeSN, int TinyArcingCount = -1, CompareType TinyArcing = CompareType.Equal, int StrongArcingCount = -1, CompareType StrongArcing = CompareType.Equal)
        {
            //_logger.LogInformation($"startTime:{startTime} endTime:{endTime} tubeID:{tubeID} tubeSN:{tubeSN} ArcingCount:{ArcingCount}");
            IList<List<TubeLogEntityModel>> lst = new List<List<TubeLogEntityModel>>();
            if (startTime == null || endTime == null)
            {
                startTime = DateTime.MinValue; endTime = DateTime.MaxValue;
                _logFiles = Directory.GetFiles(_path, "TubeLog_*.log").ToList<string>();
            }
            else
            {

                if (endTime <= startTime)
                {
                    TubeLogProgressListHandler?.Invoke(0, 0);
                    return lst;
                }

                long days = DateAndTime.DateDiff(DateInterval.Day, startTime!.Value, endTime!.Value);
                _logFiles = new List<string>();
                for (long day = 0; day <= days; day++)
                {
                    _logFiles.AddRange(Directory.GetFiles(_path, "TubeLog_" + (DateAndTime.DateAdd(DateInterval.Day, day, startTime!.Value)).ToString("yyyyMMdd") + "*.log"));
                }
            }
            //无有效日志文件
            if (_logFiles.Count <= 0)
            {
                TubeLogProgressListHandler?.Invoke(0, 0);
                return lst;
            }

            _tubeID = tubeID;
            if (_tubeID == null || _tubeID.Count == 0)
            {
                _tubeID = new List<int>();
                for (int i = 0; i < _tubeCount; i++)
                {
                    _tubeID.Add(i);
                }
            }

            if (tubeSN == null) tubeSN = new List<string>();
            _tubeHistory.Clear();
            for (int i = 1; i <= _tubeCount; i++)
            {
                var ll = SystemConfig.GetComponentHistory(DeviceComponentType.XRaySourceTankbox, i).Where(
                t => (tubeSN.Count == 0 || tubeSN.Contains(t.SerialNumber)) && t.UsingEndTime >= startTime && t.UsingBeginTime <= endTime
                    );
                _tubeHistory.Add(ll.ToList());
            }

            _logTasks.Clear();
            Interlocked.Exchange(ref _GetLogProgress, 0);
            TubeLogProgressListHandler?.Invoke(0, _logFiles.Count);

            const int ThreadCount = 5;
            //int ThreadSum = files.Count / ThreadCount;
            int j = 0;
            Console.WriteLine($"from:0 to:{_logFiles.Count - 1}");
            _listArray.Clear();
            //for( int i = 0;i<ThreadCount;i++) _listArray.Add(new List<IList<TubeLogEntityModel>>());
            int ind = 0;
            while ((j + ThreadCount) < _logFiles.Count)
            {
                _listArray.Add(new List<IList<TubeLogEntityModel>>());
                StartProcessTubeLogAsync(ind, j, j + ThreadCount, startTime!.Value, endTime!.Value, TinyArcingCount, TinyArcing, StrongArcingCount, StrongArcing);
                Console.WriteLine($"start:{j} end:{j + ThreadCount - 1}");
                //MessageBox.Show($"start:{j} end:{j + ThreadCount - 1}");
                j += ThreadCount;
                ind++;
            }
            if (_logFiles.Count > j)
            {
                _listArray.Add(new List<IList<TubeLogEntityModel>>());
                StartProcessTubeLogAsync(ind, j, _logFiles.Count, startTime!.Value, endTime!.Value, TinyArcingCount, TinyArcing, StrongArcingCount, StrongArcing);
                Console.WriteLine($"start:{j} end:{_logFiles.Count - 1}");
                //MessageBox.Show($"start:{j} end:{files.Count - 1}");
            }
            if (_logTasks.Count > 0)
            {
                Task.WaitAll(_logTasks.ToArray());
            }
            _logTasks.Clear();

            for (int i = 0; i < _tubeCount; i++)
            {
                lst.Add(new List<TubeLogEntityModel>());
            }


            for (int m = 0; m < _tubeCount; m++)
            {
                for (int n = 0; n < _listArray.Count; n++)
                {
                    lst[m].AddRange(_listArray[n][m]);
                }
            }
            return lst;

        }
        private async void StartProcessTubeLogAsync(int index, int start, int end, DateTime startTime, DateTime endTime, int TinyArcingCount, CompareType TinyArcing, int StrongArcingCount, CompareType StrongArcing)
        {
            Task tskStartCropAsync = Task.Run(
                () =>
                {
                    for (int f = start; f < end; ++f)
                    {
                        for (int i = 0; i < _tubeCount; i++)
                        {
                            _listArray[index].Add(new List<TubeLogEntityModel>());
                        }
                        ProcessTubeLogFile(index, startTime, endTime, _logFiles[f], TinyArcingCount, TinyArcing, StrongArcingCount, StrongArcing);
                        Interlocked.Add(ref _GetLogProgress, 1);
                        TubeLogProgressListHandler?.Invoke((int)Interlocked.Read(ref _GetLogProgress), _logFiles.Count);
                    }

                });
            _logTasks.Add(tskStartCropAsync);
            await tskStartCropAsync;
        }
        private void ProcessTubeLogFile(int index, DateTime startTime, DateTime endTime, string filename, int TinyArcingCount, CompareType TinyArcing, int StrongArcingCount, CompareType StrongArcing)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string? scanBegin;
                string? parameters;
                string? tubeInfoBeforeScan;
                string? tubeInfoAfterScan;
                string? detectorInfoBeforeScan;
                string? detectorInfoAfterScan;
                string? tubeDoseInfoDuringScan;
                string? tubeArcInfoDuringScan;
                string? end;
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (true)
                    {
                        scanBegin = sr.ReadLine();
                        //确保对齐
                        if (scanBegin == null) break;
                        if (scanBegin.Length < 32) continue;
                        if (scanBegin[25] != 'S') continue;

                        parameters = sr.ReadLine();
                        if (parameters == null) break;
                        tubeInfoBeforeScan = sr.ReadLine();
                        if (tubeInfoBeforeScan == null) break;
                        tubeInfoAfterScan = sr.ReadLine();
                        if (tubeInfoAfterScan == null) break;
                        detectorInfoBeforeScan = sr.ReadLine();
                        if (detectorInfoBeforeScan == null) break;
                        detectorInfoAfterScan = sr.ReadLine();
                        if (detectorInfoAfterScan == null) break;
                        tubeDoseInfoDuringScan = sr.ReadLine();
                        if (tubeDoseInfoDuringScan == null) break;
                        tubeArcInfoDuringScan = sr.ReadLine();
                        if (tubeArcInfoDuringScan!.EndsWith("End.") || !tubeArcInfoDuringScan!.Contains("TubeArcingInfoDuringScan"))
                        {
                            //if (string.IsNullOrEmpty(tubeArcInfoDuringScan))
                            tubeArcInfoDuringScan = @"2999-12-30 23:59:59.9999 TubeArcingInfoDuringScan:[{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":0,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":3},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-61,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-63,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-124,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-39,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-68,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-82,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-62,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-59,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-43,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-27,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-60,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-53,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-75,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-59,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-71,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-66,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-67,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-77,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-46,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-40,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-72,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-72,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-52,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0},{""TubeSmallArcing1"":0,""TubeLargeArcing1"":0,""preSlope1"":-6556,""TubeSmallArcing2"":0,""TubeLargeArcing2"":0,""preSlope2"":0,""TubeSmallArcing3"":0,""TubeLargeArcing3"":0,""preSlope3"":0,""skip"":0}]";
                        }
                        else
                        {
                            end = sr.ReadLine();
                        }
                        DateTime.TryParse(scanBegin.Substring(0, 24), out DateTime dt);
                        if (dt < startTime) continue;
                        if (dt > endTime) continue;
                        int jsonBegin = tubeArcInfoDuringScan.IndexOf(':', 25);
                        var tubeArcInfo = JsonSerializer.Deserialize<TubeArcing[]>(tubeArcInfoDuringScan.Substring(jsonBegin + 1, tubeArcInfoDuringScan.Length - jsonBegin - 1));
                        var __tubeID = _tubeID;
                        if ((TinyArcingCount >= 0) || (StrongArcingCount >= 0)) __tubeID = new List<int>();
                        if ((__tubeID.Count == 0) && (tubeArcInfo != null))
                        {
                            foreach (var i in _tubeID)
                            {
                                if (StrongArcingCount >= 0)
                                {
                                    switch (StrongArcing)
                                    {
                                        case CompareType.LessThanOrEqual:
                                            if ((tubeArcInfo[i].TubeLargeArcing1 <= StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing2 <= StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing3 <= StrongArcingCount)) __tubeID.Add(i); continue;
                                        case CompareType.GreaterThanOrEqual:
                                            if ((tubeArcInfo[i].TubeLargeArcing1 >= StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing2 >= StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing3 >= StrongArcingCount)) __tubeID.Add(i); continue;
                                        case CompareType.GreaterThan:
                                            if ((tubeArcInfo[i].TubeLargeArcing1 > StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing2 > StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing3 > StrongArcingCount)) __tubeID.Add(i); continue;
                                        case CompareType.LessThan:
                                            if ((tubeArcInfo[i].TubeLargeArcing1 < StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing2 < StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing3 < StrongArcingCount)) __tubeID.Add(i); continue;
                                        default://case CompareType.Equal:
                                            if ((tubeArcInfo[i].TubeLargeArcing1 == StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing2 == StrongArcingCount) || (tubeArcInfo[i].TubeLargeArcing3 == StrongArcingCount)) __tubeID.Add(i); continue;
                                    }
                                }
                                if (TinyArcingCount >= 0)
                                {
                                    switch (TinyArcing)
                                    {
                                        case CompareType.LessThanOrEqual:
                                            if ((tubeArcInfo[i].TubeSmallArcing1 <= TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing2 <= TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing3 <= TinyArcingCount)) __tubeID.Add(i); continue;
                                        case CompareType.GreaterThanOrEqual:
                                            if ((tubeArcInfo[i].TubeSmallArcing1 >= TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing2 >= TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing3 >= TinyArcingCount)) __tubeID.Add(i); continue;
                                        case CompareType.GreaterThan:
                                            if ((tubeArcInfo[i].TubeSmallArcing1 > TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing2 > TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing3 > TinyArcingCount)) __tubeID.Add(i); continue;
                                        case CompareType.LessThan:
                                            if ((tubeArcInfo[i].TubeSmallArcing1 < TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing2 < TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing3 < TinyArcingCount)) __tubeID.Add(i); continue;
                                        default:// CompareType.Equal:
                                            if ((tubeArcInfo[i].TubeSmallArcing1 == TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing2 == TinyArcingCount) || (tubeArcInfo[i].TubeSmallArcing3 == TinyArcingCount)) __tubeID.Add(i); continue;
                                    }
                                }
                            }
                        }
                        if(__tubeID.Count <= 0) continue;
                        jsonBegin = tubeInfoBeforeScan.IndexOf(':', 25);
                        var tubeStatusBefore = JsonSerializer.Deserialize<TubeStatus[]>(tubeInfoBeforeScan.Substring(jsonBegin + 1, tubeInfoBeforeScan.Length - jsonBegin - 1));


                        jsonBegin = tubeInfoAfterScan.IndexOf(':', 25);
                        var tubeStatusAfter = JsonSerializer.Deserialize<TubeStatus[]>(tubeInfoAfterScan.Substring(jsonBegin + 1, tubeInfoAfterScan.Length - jsonBegin - 1));
                        jsonBegin = tubeDoseInfoDuringScan.IndexOf(':', 25);
                        var tubeDoseInfo = JsonSerializer.Deserialize<DoseInfo[]>(tubeDoseInfoDuringScan.Substring(jsonBegin + 1, tubeDoseInfoDuringScan.Length - jsonBegin - 1));

                        jsonBegin = detectorInfoBeforeScan.IndexOf(':', 25);
                        var detectorInfoBefore = JsonSerializer.Deserialize<TemperatureInfo>(detectorInfoBeforeScan.Substring(jsonBegin + 1, detectorInfoBeforeScan.Length - jsonBegin - 1));

                        jsonBegin = detectorInfoAfterScan.IndexOf(':', 25);
                        var detectorInfoAfter = JsonSerializer.Deserialize<TemperatureInfo>(detectorInfoAfterScan.Substring(jsonBegin + 1, detectorInfoAfterScan.Length - jsonBegin - 1));

                        jsonBegin = tubeArcInfoDuringScan.IndexOf(':', 25);
                        //var tubeArcInfo = JsonSerializer.Deserialize<TubeArcing[]>(tubeArcInfoDuringScan.Substring(jsonBegin + 1, tubeArcInfoDuringScan.Length - jsonBegin - 1));
                        //if (tubeArcInfo == null)
                        //{
                        //    tubeArcInfo = new TubeArcing[_tubeCount];
                        //}
                        jsonBegin = parameters.IndexOf(':', 25);
                        var sp = JsonSerializer.Deserialize<ScanParam>(parameters.Substring(jsonBegin + 1, parameters.Length - jsonBegin - 1), new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
                        foreach (var tID in __tubeID)
                        {
                            var th = _tubeHistory[tID].OrderBy(t => t.UsingBeginTime).Where(t => t.UsingBeginTime <= dt).LastOrDefault();
                            if (th == null) continue;
                            TubeLogEntityModel tmp = new TubeLogEntityModel();
                            tmp.scanParam = sp;
                            tmp.doseInfo = new DoseInfo();
                            tmp.tubeStatusAfter = new TubeStatus();
                            tmp.tubeStatusBefore = new TubeStatus();
                            tmp.viewCount = 0;
                            tmp.dateTime = dt;
                            _listArray[index][tID].Add(tmp);
                            tmp.tubeID = tID + 1;
                            tmp.tubeSN = th.SerialNumber;
                            tmp.UsingBeginTime = th.UsingBeginTime;
                            tmp.UsingEndTime = th.UsingEndTime;
                            if (tubeStatusBefore != null) tmp.tubeStatusBefore = tubeStatusBefore[tID];
                            if (tubeStatusAfter != null) tmp.tubeStatusAfter = tubeStatusAfter[tID];
                            if (tubeDoseInfo != null) tmp.doseInfo = tubeDoseInfo[tID];
                            if (detectorInfoBefore != null) tmp.tempInfoBefore = detectorInfoBefore;
                            if (detectorInfoAfter != null) tmp.tempInfoAfter = detectorInfoAfter;
                            if (tubeArcInfo != null) tmp.tubeArcing = tubeArcInfo[tID];
                        }
                    }
                }


            }
        }
    }
}