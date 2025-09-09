using NV.CT.Service.Common;
using NV.CT.Service.HardwareTest.Services.Universal.RegionalStaticResource;
using System;

namespace NV.CT.Service.HardwareTest.Initializer
{
    public class RegionalStyleInitializer
    {
        public static void ConfigureRegionalResources() 
        {
            try
            {
                /** 资源路径 **/
                var path = "pack://application:,,,/NV.CT.Service.HardwareTest.UI;component/StyleCollection.xaml";
                /** 初始化 **/
                RegionalStaticResourceService.Instance.AddResourceDictionary(path);
            }
            catch (Exception ex)
            {
                /** 记录 **/
                LogService.Instance.Error(
                    ServiceCategory.HardwareTest, 
                    $"Something wrong when executing [ConfigureRegionalResources] in [RegionalStyleInitializer], [Stack]: {ex.ToString()}");
            }
        }
    }
}
