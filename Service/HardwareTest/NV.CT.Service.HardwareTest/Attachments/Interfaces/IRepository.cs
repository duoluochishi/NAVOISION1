using System;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Repository
{
    public interface IRepository<T> where T : class
    {
        public T? Add(T entity);
        public T Delete(T entity);
        public T? Get(T entity);
        public T Update(T entity);
        public IEnumerable<T> GetAll();   
        public IEnumerable<T> GetBetweenTimeSpan(DateTime start, DateTime end);
    }
}
