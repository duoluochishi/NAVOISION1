using System;
using System.Collections.Generic;
using NV.CT.Service.TubeHistory.Enums;
using NV.CT.Service.TubeHistory.Service.Models;

namespace NV.CT.Service.TubeHistory.Service.Interfaces
{
    public interface ITubeLog
    {
        public IEnumerable<TubeLife> GetTubeLife();

        //当历史球管编号发生变更，请调用该接口对统计数据重置，确保球管编号的变更生效
        ///public IEnumerable<IList<TubeLife>> RestTubeLife();
        public IEnumerable<IList<TubeLogEntityModel>> GetTubeLog(DateTime? startTime, DateTime? endTime, IList<int> tubeID, IList<string> tubeSN, int TinyArcingCount = -1, CompareType TinyArcing = CompareType.Equal, int StrongArcingCount = -1, CompareType StrongArcing = CompareType.Equal);

        //public event Action<int, int> TubeLogProgressListHandler;
    }
}
