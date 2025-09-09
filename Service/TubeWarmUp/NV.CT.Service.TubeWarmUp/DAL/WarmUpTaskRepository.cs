using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NV.CT.Service.TubeWarmUp.Utilities;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using AutoMapper;
using NV.CT.Service.Common;

namespace NV.CT.Service.TubeWarmUp.DAL
{
    public class WarmUpTaskRepository : IRepository<WarmUpTaskDto>,
        IScanParamRespository
    {
        [Serializable]
        public class DBFormat
        {
            public DBFormat()
            {
                this.NextId = 1;
                this.Tasks = new List<WarmUpTaskDto>(0);
                this.ScanParam = new ScanParamDto();
            }

            public int NextId { get; set; }
            public List<WarmUpTaskDto> Tasks { get; set; }
            public ScanParamDto ScanParam { get; set; }
        }

        private string _fileName = "WarmUpTask.json";
        private string _filePath;
        private readonly IMapper _mapper;

        public WarmUpTaskRepository(IMapper mapper)
        {
            this._mapper = mapper;
            this._filePath = ConfigPathService.Instance.GetConfigPath("TubeWarmUp", _fileName);
        }

        private DBFormat ReadFile()
        {
            if (!File.Exists(this._filePath))
            {
                File.WriteAllText(this._filePath, "{}");
            }
            var content = File.ReadAllText(this._filePath);
            var res = SerializeUtility.Deserialize<DBFormat>(content);
            if (res == null)
            {
                res = new DBFormat();
            }
            return res;
        }

        private void WriteFile(DBFormat instance)
        {
            var content = SerializeUtility.Serialize<DBFormat>(instance);
            File.WriteAllText(this._filePath, content);
        }

        public ScanParamDto GetScanParam()
        {
            var format = ReadFile();
            return format.ScanParam;
        }

        public bool Delete(WarmUpTaskDto entity)
        {
            var format = ReadFile();
            var current = format.Tasks.FirstOrDefault(p => p.Id == entity.Id);
            if (current == null)
            {
                return false;
            }
            else
            {
                format.Tasks.Remove(current);
                WriteFile(format);
                return true;
            }
        }

        public WarmUpTaskDto Get(int id)
        {
            var format = ReadFile();
            return format.Tasks.FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<WarmUpTaskDto> GetList()
        {
            var format = ReadFile();
            return format.Tasks;
        }

        public WarmUpTaskDto Add(WarmUpTaskDto entity)
        {
            var format = ReadFile();
            entity.Id = format.NextId;
            format.Tasks.Add(entity);
            format.NextId += 1;
            WriteFile(format);

            return entity;
        }

        public bool Update(WarmUpTaskDto entity)
        {
            var format = ReadFile();
            var current = format.Tasks.FirstOrDefault(p => p.Id == entity.Id);
            if (current == null)
            {
                return false;
            }
            else
            {
                this._mapper.Map(entity, current);
                WriteFile(format);
                return true;
            }
        }
    }
}