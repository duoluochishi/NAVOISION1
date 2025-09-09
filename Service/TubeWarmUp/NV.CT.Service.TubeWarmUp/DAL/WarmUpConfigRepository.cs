using NV.CT.Service.Common;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.DAL
{
    public class WarmUpConfigRepository : IRepository<WarmUpConfigDto>
    {
        private string _fileName = "WarmUpConfig.json";
        private string _filePath;
        public WarmUpConfigRepository()
        {
            this._filePath = ConfigPathService.Instance.GetConfigPath("TubeWarmUp", _fileName);
        }
        private WarmUpConfigDto ReadFile()
        {
            if (!File.Exists(this._filePath))
            {
                var data = SerializeUtility.Serialize(new WarmUpConfigDto());
                File.WriteAllText(this._filePath, data);
            }
            var content = File.ReadAllText(this._filePath);
            var res = SerializeUtility.Deserialize<WarmUpConfigDto>(content);
            if (res == null)
            {
                res = new WarmUpConfigDto();
            }
            return res;
        }
        public WarmUpConfigDto Add(WarmUpConfigDto entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(WarmUpConfigDto entity)
        {
            throw new NotImplementedException();
        }

        public WarmUpConfigDto Get(int id)
        {
            return ReadFile();
        }

        public IEnumerable<WarmUpConfigDto> GetList()
        {
            throw new NotImplementedException();
        }

        public bool Update(WarmUpConfigDto entity)
        {
            throw new NotImplementedException();
        }
    }
}
