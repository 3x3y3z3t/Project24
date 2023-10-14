/*  App/Identity/P24RoleName.cs
 *  Version: v1.0 (2023.10.15)
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
            PageCollection.Home.Management.ConfigPanel,
        };
    }

}
