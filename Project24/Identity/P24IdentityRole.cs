/*  P24IdentityRole.cs
 *  Version: 1.0 (2022.09.06)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Identity;

namespace Project24.Identity
{
    public class P24IdentityRole : IdentityRole
    {
        public int Level { get; set; }

        public P24IdentityRole()
        {

        }


    }
}
