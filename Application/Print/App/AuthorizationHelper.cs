//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Print
{
    public static class AuthorizationHelper
    {
        private static IAuthorization _authorizationService;

        public static bool ValidatePermission(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                return false;
            }

            if (_authorizationService is null)
            {
                _authorizationService = CTS.Global.ServiceProvider.GetService<IAuthorization>();
            }

            return _authorizationService.IsAuthorized(permissionName);
        }

    }
}
