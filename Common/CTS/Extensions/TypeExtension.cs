//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Extensions;

namespace NV.CT.CTS.Extensions;

public static class TypeExtension
{
    public static Dictionary<string, object?> ToDictionary(this object entity)
    {
        var dictionary = entity.GetType().GetProperties().OrderBy(p => p.Name).ToDictionary(q => q.Name, q => q.GetValue(entity));
        return dictionary;
    }

    public static TEntity ToEntity<TEntity>(this Dictionary<string, object> dictionary) where TEntity : class, new()
    {
        var entity = new TEntity();
        var properties = typeof(TEntity).GetProperties();
        foreach (var property in properties)
        {
            if (dictionary.ContainsKey(property.Name))
            {
                property.SetValue(entity, dictionary[property.Name]);
            }
        }
        return entity;
    }
}
