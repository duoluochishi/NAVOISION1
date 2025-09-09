using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.Service.TubeWarmUp.DAL;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using AutoMapper;

namespace NV.CT.Service.TubeWarmUp.Services
{
    public class DataService : IDataService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<WarmUpHistoryDto> _historyRepository;
        private readonly IRepository<WarmUpTaskDto> _taskRepository;
        private readonly IScanParamRespository _scanParamRespository;

        public DataService()
        {
            var config = new MapperConfiguration(p =>
            {
                p.CreateMap<WarmUpTask, WarmUpTaskDto>();
                p.CreateMap<WarmUpTaskDto, WarmUpTask>();
                p.CreateMap<WarmUpTaskDto, WarmUpTaskDto>();

                p.CreateMap<WarmUpHistoryDto, WarmUpHistory>();
                p.CreateMap<WarmUpHistory, WarmUpHistoryDto>();
            });
            this._mapper = config.CreateMapper();
            this._historyRepository = new WarmUpHistoryRepository();
            var warmUpTaskRep = new WarmUpTaskRepository(this._mapper);
            this._taskRepository = warmUpTaskRep;
            this._scanParamRespository = warmUpTaskRep;
        }

        #region WarmUpHistory

        public IEnumerable<WarmUpHistory> GetWarmUpHistories()
        {
            var list = this._historyRepository.GetList();
            var res = new List<WarmUpHistory>(list.Count());
            foreach (var item in list)
            {
                res.Add(this._mapper.Map<WarmUpHistory>(item));
            }
            return res;
        }

        public WarmUpHistory AddWarmUpHistory(WarmUpHistory history)
        {
            var res = this._historyRepository.Add(this._mapper.Map<WarmUpHistoryDto>(history));
            return this._mapper.Map<WarmUpHistory>(res);
        }

        #endregion WarmUpHistory

        #region WarmUpTask

        public IEnumerable<WarmUpTask> GetWarmUpTasks()
        {
            var list = this._taskRepository.GetList();
            var res = new List<WarmUpTask>();
            foreach (var item in list)
            {
                res.Add(this._mapper.Map<WarmUpTaskDto, WarmUpTask>(item));
            }
            return res;
        }

        public WarmUpTask AddWarmUpTask(WarmUpTask task)
        {
            var dto = this._mapper.Map<WarmUpTask, WarmUpTaskDto>(task);
            var res = this._taskRepository.Add(dto);
            return this._mapper.Map<WarmUpTask>(res);
        }

        public bool UpdateWarmUpTask(WarmUpTask task)
        {
            var dto = this._mapper.Map<WarmUpTask, WarmUpTaskDto>(task);
            return this._taskRepository.Update(dto);
        }

        public bool DeleteWarmUpTask(WarmUpTask task)
        {
            var dto = this._mapper.Map<WarmUpTask, WarmUpTaskDto>(task);
            return this._taskRepository.Delete(dto);
        }

        #endregion WarmUpTask

        public ScanParamDto GetScanParam()
        {
            return this._scanParamRespository.GetScanParam();
        }
    }
}