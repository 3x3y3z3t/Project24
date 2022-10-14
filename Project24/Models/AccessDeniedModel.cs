/*  AccessDeniedModel.cs
 *  Version: 1.0 (2022.09.03)
 *
 *  Contributor
 *      Arime-chan
 */

namespace Project24.Models
{
    public class AccessDeniedModel
    {
        public string Reason { get; set; }

        public AccessDeniedModel(string _reason = "")
        {
            Reason = _reason;
        }
    }

}
