using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.Interfaces
{
    public interface IRepository<T> where T : class
    {
        //因为增加需要给T的Id赋值，所以需要返回T
        T Add(T entity);
        bool Update(T entity);
        bool Delete(T entity);
        T Get(int id);
        IEnumerable<T> GetList();
    }
}
