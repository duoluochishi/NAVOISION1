//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace NV.MPS.UI.Dialog.Service;
public interface IDialogParameters
{
    int Count { get; }

    IEnumerable<string> Keys { get; }

    void Add(string key, object value);

    bool ContainsKey(string key);

    T GetValue<T>(string key);

    IEnumerable<T> GetValues<T>(string key);

    bool TryGetValue<T>(string key, out T value);
}