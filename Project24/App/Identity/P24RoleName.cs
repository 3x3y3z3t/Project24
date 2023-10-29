/*  App/Identity/P24RoleName.cs
 *  Version: v1.1 (2023.10.29)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;
using Project24.App.Utils;

namespace Project24.App.Identity
{
    public static class P24RoleName
    {
        public static readonly List<string> AllRoleNames = new()
        {
            PageCollection.Home.Index,

            PageCollection.Home.Account.List,

            PageCollection.Home.Management.ConfigPanel,

            PageCollection.Simulator.FinancialManagement.List,
            PageCollection.Simulator.FinancialManagement.Create,
            PageCollection.Simulator.FinancialManagement.Remove,


        };
    }

}
