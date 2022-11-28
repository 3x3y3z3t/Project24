/*  CommonNotFoundModel.cs
 *  Version: 1.0 (2022.11.20)
 *
 *  Contributor
 *      Arime-chan
 */

namespace Project24.Models.ClinicManager
{
    public struct CommonNotFoundModel
    {
        public string EntityType;
        public string Code;

        public string ReturnUrl;

        public CommonNotFoundModel(string _entityType, string _code, string _returnUrl = "")
        {
            EntityType = _entityType;
            Code = _code;
            ReturnUrl = _returnUrl;
        }
    }

}
