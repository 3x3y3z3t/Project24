/*  CommonNotFoundModel.cs
 *  Version: 1.1 (2023.01.01)
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

        public string ReturnPage;


        public CommonNotFoundModel(string _entityType, string _code, string _returnPage = "")
        {
            EntityType = _entityType;
            Code = _code;
            ReturnPage = _returnPage;
        }
    }

}
