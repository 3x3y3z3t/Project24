/*  App/Identity/P24RoleUtils.cs
 *  Version: v1.3 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;

namespace Project24.App.Utils.Identity
{
    public static class P24RoleUtils
    {
        public static readonly List<string> AllRoleNames = new()
        {
            PageCollection.Home.Index,

            PageCollection.Home.Account.List,
            PageCollection.Home.Account.Manage,
            PageCollection.Home.Account.Create,

            PageCollection.Home.Management.ConfigPanel,
            PageCollection.Home.Management.Updater,

            PageCollection.Simulator.FinancialManagement.List,
            PageCollection.Simulator.FinancialManagement.Create,
            PageCollection.Simulator.FinancialManagement.Remove,


        };

        /// <summary>
        ///     NOTE: Only write to this list when you change a user's role.
        ///     Any user in this list will be force-signed out.
        /// </summary>
        public static HashSet<string> RolesDirtyUser { get; set; }
    }

}
