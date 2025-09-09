using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTool
{
    public class JsonComparer
    {
        /// <summary>
        /// 递归比较两个JToken对象的差异
        /// </summary>
        public void CompareJson(JToken token1, JToken token2, string path, List<string> differences)
        {
            // 检查令牌类型是否相同
            if (token1.Type != token2.Type)
            {
                differences.Add($"{path}: 类型不同 - 左侧: {token1.Type}, 右侧: {token2.Type}");
                return;
            }

            // 根据令牌类型进行不同的比较
            switch (token1.Type)
            {
                case JTokenType.Object:
                    CompareJObject((JObject)token1, (JObject)token2, path, differences);
                    break;
                case JTokenType.Array:
                    CompareJArray((JArray)token1, (JArray)token2, path, differences);
                    break;
                default:
                    ComparePrimitiveValues(token1, token2, path, differences);
                    break;
            }
        }

        /// <summary>
        /// 比较JObject（JSON对象）
        /// </summary>
        private void CompareJObject(JObject obj1, JObject obj2, string path, List<string> differences)
        {
            // 检查左侧有而右侧没有的属性
            foreach (var prop in obj1.Properties())
            {
                string newPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                if (!obj2.ContainsKey(prop.Name))
                {
                    differences.Add($"{newPath}: 左侧存在，右侧不存在");
                }
                else
                {
                    // 递归比较属性值
                    CompareJson(prop.Value, obj2[prop.Name], newPath, differences);
                }
            }

            // 检查右侧有而左侧没有的属性
            foreach (var prop in obj2.Properties())
            {
                string newPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                if (!obj1.ContainsKey(prop.Name))
                {
                    differences.Add($"{newPath}: 右侧存在，左侧不存在");
                }
            }
        }

        /// <summary>
        /// 比较JArray（JSON数组）
        /// </summary>
        private void CompareJArray(JArray arr1, JArray arr2, string path, List<string> differences)
        {
            // 比较数组长度
            if (arr1.Count != arr2.Count)
            {
                differences.Add($"{path}: 数组长度不同 - 左侧: {arr1.Count}, 右侧: {arr2.Count}");
            }

            // 比较数组元素
            int minLength = Math.Min(arr1.Count, arr2.Count);
            for (int i = 0; i < minLength; i++)
            {
                CompareJson(arr1[i], arr2[i], $"{path}[{i}]", differences);
            }
        }

        /// <summary>
        /// 比较基本类型值
        /// </summary>
        static void ComparePrimitiveValues(JToken token1, JToken token2, string path, List<string> differences)
        {
            if (!JToken.DeepEquals(token1, token2))
            {
                differences.Add($"{path}: 值不同 - 左侧: {token1}, 右侧: {token2}");
            }
        }
    }
}
