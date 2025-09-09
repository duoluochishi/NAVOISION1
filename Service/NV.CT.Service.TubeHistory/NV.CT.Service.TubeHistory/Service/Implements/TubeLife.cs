using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.TubeHistory.Service.Interfaces;
using NV.CT.Service.TubeHistory.Service.Models;
using NV.MPS.Configuration;

namespace NV.CT.Service.TubeHistory.Service.Implements
{
    public partial class TubeLog : ITubeLog
    {
        #region Field
        private IList<List<List<TubeLife>>> _tubeLife = new List<List<List<TubeLife>>>();
        private string[] _lifeFiles;
        private bool _resetTubeLife;
        private List<Task> _lifeTasks = new List<Task>();

        #endregion
        public IEnumerable<TubeLife> RestTubeLife()
        {
            _resetTubeLife = true;
            return GetTubeLife();
        }

        public IEnumerable<TubeLife> GetTubeLife()
        {

            _resetTubeLife = false;
            _tubeLife.Clear();
            _lifeTasks.Clear();
            _tubeHistory.Clear();
            for (int i = 1; i <= _tubeCount; i++)
            {
                var ll = SystemConfig.GetComponentHistory(DeviceComponentType.XRaySourceTankbox, i);
                _tubeHistory.Add(ll.ToList());
            }

            _lifeFiles = Directory.GetFiles(_path, "TubeLog_*.log");


            const int ThreadCount = 5;
            //int ThreadSum = files.Count / ThreadCount;
            int j = 0;
            Console.WriteLine($"from:0 to:{_lifeFiles.Length - 1}");
            _listArray.Clear();
            //for( int i = 0;i<ThreadCount;i++) _listArray.Add(new List<IList<TubeLogEntityModel>>());
            int ind = 0;
            while ((j + ThreadCount) < _lifeFiles.Length)
            {
                _tubeLife.Add(new List<List<TubeLife>>());
                StartProcessTubeLifeAsync(ind, j, j + ThreadCount);
                Console.WriteLine($"start:{j} end:{j + ThreadCount - 1}");
                j += ThreadCount;
                ind++;
            }
            if (_lifeFiles.Length > j)
            {
                _tubeLife.Add(new List<List<TubeLife>>());
                StartProcessTubeLifeAsync(ind, j, _lifeFiles.Length);
                Console.WriteLine($"start:{j} end:{_lifeFiles.Length - 1}");
            }
            if (_lifeTasks.Count > 0)
            {
                Task.WaitAll(_lifeTasks.ToArray());
            }
            _lifeTasks.Clear();
            IList<IList<TubeLife>> lst = new List<IList<TubeLife>>();
            for (int i = 0; i < _tubeCount; i++)
            {
                lst.Add(new List<TubeLife>());
            }
            for (int index = 0; index < _tubeLife.Count; index++)
            {
                for (int tID = 0; tID < _tubeCount; tID++)
                {
                    for (int m = 0; m < _tubeLife[index][tID].Count; m++)
                    {
                        var tb = _tubeLife[index][tID][m];
                        var tubeinfo = lst[tID].Where(x => x.tubeSN == tb.tubeSN).FirstOrDefault();
                        if (tubeinfo == null)
                        {
                            tubeinfo = new TubeLife();
                            tubeinfo.tubeSN = tb.tubeSN;
                            tubeinfo.totalMs = tb.totalMs;
                            tubeinfo.tubeID = tb.tubeID;
                            tubeinfo.UsingBeginTime = tb.UsingBeginTime;
                            lst[tID].Add(tubeinfo);
                        }
                        else
                        {
                            tubeinfo.totalMs += tb.totalMs;
                        }
                    }
                }
            }
            List<TubeLife> lstmp = new List<TubeLife>();

            for (int m = 0; m < lst.Count; m++)
            {
                lstmp.AddRange(lst[m]);
            }
            return lstmp;

        }
        private async void StartProcessTubeLifeAsync(int index, int start, int end)
        {
            Task tskStartCropAsync = Task.Run(
                () =>
                {
                    for (int f = start; f < end; ++f)
                    {
                        for (int i = 0; i < _tubeCount; i++)
                        {
                            _tubeLife[index].Add(new List<TubeLife>());
                        }
                        ProcessTubeLifeFile(index, _lifeFiles[f]);
                    }

                });
            _lifeTasks.Add(tskStartCropAsync);
            await tskStartCropAsync;
        }

        private void ProcessTubeLifeFile(int index, string filename)
        {
            for (int n = 0; n < _tubeCount; n++)
            {
                _tubeLife[index].Add(new List<TubeLife>());
            }

            if ((_resetTubeLife == false) && File.Exists(filename + ".xcc"))
            {
                using (StreamReader sr = new StreamReader(filename + ".xcc"))
                {
                    string? line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var objs = JsonUtil.Deserialize<List<List<TubeLife>>>(line);
                        if (objs == null) return;
                        for (int tID = 0; tID < objs.Count; tID++)
                        {
                            foreach (var obj in objs[tID])
                            {
                                var tubeinfo = _tubeLife[index][tID].Where(x => x.tubeSN == obj.tubeSN).FirstOrDefault();
                                if (tubeinfo == null)
                                {
                                    tubeinfo = new TubeLife();
                                    tubeinfo.tubeSN = obj.tubeSN;
                                    tubeinfo.totalMs = obj.totalMs;
                                    tubeinfo.tubeID = obj.tubeID;
                                    _tubeLife[index][tID].Add(tubeinfo);
                                }
                                else
                                {
                                    tubeinfo.totalMs += obj.totalMs;
                                }
                            }
                        }
                    }
                }

                return;
            }

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string? scanBegin;
                string? parameters;
                string? tubeInfoBeforeScan;
                string? tubeInfoAfterScan;
                string? detectorInfoBeforeScan;
                string? detectorInfoAfterScan;
                string? tubeDoseInfoDuringScan;
                string? end;
                IList<List<TubeLife>> mS = new List<List<TubeLife>>();
                for (int i = 0; i < _tubeCount; i++)
                {
                    mS.Add(new List<TubeLife>());
                }
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
                        end = sr.ReadLine();

                        int jsonBegin = tubeDoseInfoDuringScan.IndexOf(':', 25);
                        var tubeDoseInfo = JsonSerializer.Deserialize<DoseInfo[]>(tubeDoseInfoDuringScan.Substring(jsonBegin + 1, tubeDoseInfoDuringScan.Length - jsonBegin - 1));
                        if (tubeDoseInfo == null) continue;
                        DateTime.TryParse(scanBegin.Substring(0, 24), out DateTime dt);

                        for (int tID = 0; tID < _tubeCount; tID++)
                        {
                            var th = _tubeHistory[tID].OrderBy(t => t.UsingBeginTime).Where(t => t.UsingBeginTime <= dt).LastOrDefault();
                            string tubesn = "XXXXXXXXXXX";
                            DateTime tubedt = DateTime.Now;
                            if (th != null)
                            {
                                tubesn = th.SerialNumber;
                                tubedt = th.UsingBeginTime;
                            }

                            var tubeinfo = _tubeLife[index][tID].Where(x => x.tubeSN == tubesn).FirstOrDefault();
                            if (tubeinfo == null)
                            {
                                tubeinfo = new TubeLife();
                                tubeinfo.tubeSN = tubesn;
                                tubeinfo.totalMs = tubeDoseInfo[tID].mS;
                                tubeinfo.tubeID = tID + 1;
                                tubeinfo.UsingBeginTime = tubedt;
                                _tubeLife[index][tID].Add(tubeinfo);
                            }
                            else
                            {
                                tubeinfo.totalMs += tubeDoseInfo[tID].mS;
                            }
                            var tinfo = mS[tID].Where(x => x.tubeSN == tubesn).FirstOrDefault();
                            if (tinfo == null)
                            {
                                tinfo = new TubeLife();
                                tinfo.tubeSN = tubesn;
                                tinfo.totalMs = tubeDoseInfo[tID].mS;
                                tinfo.tubeID = tID + 1;
                                tinfo.UsingBeginTime = tubedt;
                                mS[tID].Add(tubeinfo);
                            }
                            else
                            {
                                tinfo.totalMs += tubeDoseInfo[tID].mS;
                            }
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(filename + ".xcc"))
                    {
                        sw.WriteLine(JsonSerializer.Serialize(mS, new JsonSerializerOptions {WriteIndented = false }));
                    }
                }
            }
        }
    }
}