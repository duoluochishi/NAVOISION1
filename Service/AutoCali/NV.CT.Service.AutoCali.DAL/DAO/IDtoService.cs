namespace NV.CT.Service.AutoCali.DAL
{
    /// <summary>
    /// 通用对象（校准场景/校准历史/...）的数据访问接口
    /// </summary>
    public interface IDtoService<T> where T : class
    {
        /// <summary>
        /// 获取全部的通用对象
        /// </summary>
        /// <returns>通用对象列表</returns>
        List<T> Get();

        /// <summary>
        /// 添加一个新的通用对象
        /// </summary>
        /// <returns></returns>
        void Add(T item);

        /// <summary>
        /// 删除一个通用对象
        /// </summary>
        /// <returns></returns>
        void Delete(T item);

        /// <summary>
        /// 更新一个通用对象
        /// </summary>
        /// <returns></returns>
        void Update(T item);

        /// <summary>
        /// 保存全部的通用对象
        /// </summary>
        /// <returns></returns>
        void Save(List<T> items);
    }
}
