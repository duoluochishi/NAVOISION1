using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.MPS.Environment;

namespace NV.CT.Service.TubeWarmUp.DAL
{
    public class WarmUpHistoryRepository : IRepository<WarmUpHistoryDto>
    {
        private string _fileName = "WarmUpHistory.txt";
        private string _filePath;
        private int _loadMaxCount = 500;

        //消息最多50个字符串
        //2022-11-16 17:00:00,message
        //19+1+50
        private const int DTLENGTH = 19;//时间所占位数

        private const char SEPARATOR = ',';//分割符
        private const int SEPARATORlENGTH = 1;//分隔符所占位数
        private const int MSGLENGTH = 50;//消息所占位数
        private int recordLength = DTLENGTH + SEPARATORlENGTH + MSGLENGTH;//记录所占位数

        public WarmUpHistoryRepository()
        {
            this._filePath = Path.Combine(Directory.GetParent(RuntimeConfig.Console.HardwareHistory.Path)?.FullName ?? Environment.CurrentDirectory, "Warmup", this._fileName);
        }

        public bool Delete(WarmUpHistoryDto entity)
        {
            throw new NotImplementedException();
        }

        public WarmUpHistoryDto Get(int id)
        {
            throw new NotImplementedException();
        }

        private void EnsureFileExist()
        {
            if (Directory.GetParent(_filePath) == null)
            {
                throw new ArgumentNullException($"parent directory of {_filePath} is null");
            }
            if (!Directory.GetParent(_filePath)!.Exists)
            {
                Directory.GetParent(_filePath)!.Create();
            }
            if (!File.Exists(this._filePath))
            {
                var file = File.Create(this._filePath);
                file.Dispose();
            }
        }

        public IEnumerable<WarmUpHistoryDto> GetList()
        {
            EnsureFileExist();
            char[] buf = new char[recordLength];
            Span<char> bufSpan = new Span<char>(buf);
            var fi = new FileInfo(this._filePath);
            var totalRecordCount = fi.Length / recordLength;
            var loadRecordCount = Math.Min(totalRecordCount, _loadMaxCount);
            var listHistories = new List<WarmUpHistoryDto>((int)loadRecordCount);
            using (var fileStream = new FileStream(this._filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek((totalRecordCount - loadRecordCount) * recordLength, SeekOrigin.Begin);
                using (var reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        bufSpan.Clear();
                        var cnt = reader.ReadBlock(bufSpan);
                        //Console.WriteLine($"read {cnt} char");
                        var index = bufSpan.IndexOf(SEPARATOR);
                        var dateTimeSpan = bufSpan.Slice(0, index);
                        var msgSpan = bufSpan.Slice(index + 1, buf.Length - DTLENGTH - SEPARATORlENGTH);
                        var dt = DateTime.ParseExact(dateTimeSpan, "yyyy-MM-dd HH:mm:ss", null);
                        var his = new WarmUpHistoryDto()
                        {
                            DateTime = dt,
                            Message = msgSpan.ToString()
                        };
                        listHistories.Add(his);
                    }
                }
            }
            return listHistories;
        }

        public WarmUpHistoryDto Add(WarmUpHistoryDto entity)
        {
            EnsureFileExist();
            char[] buf = new char[recordLength];
            Span<char> bufSpan = new Span<char>(buf);
            var str = $"{entity.DateTime:yyyy-MM-dd HH:mm:ss},{entity.Message}".ToCharArray();
            Buffer.BlockCopy(str, 0, buf, 0, str.Length * 2);
            var fi = new FileInfo(this._filePath);
            var totalRecord = fi.Length / recordLength;
            var listHistories = new List<WarmUpHistoryDto>((int)totalRecord);
            using (var fileStream = new FileStream(this._filePath, FileMode.Open, FileAccess.Write))
            {
                fileStream.Seek(0, SeekOrigin.End);
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(buf);
                }
            }
            return entity;
        }

        public bool Update(WarmUpHistoryDto entity)
        {
            throw new NotImplementedException();
        }
    }
}